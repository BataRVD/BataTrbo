﻿using NLog;
using NS.Enterprise.ClientAPI;
using NS.Enterprise.Objects;
using NS.Enterprise.Objects.Devices;
using NS.Enterprise.Objects.Users;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Timers;
using TrboPortal.Controllers;
using TrboPortal.Mappers;
using Device = NS.Enterprise.Objects.Devices.Device;
using NS.Enterprise.Objects.Event_args;
using System.Text;

namespace TrboPortal.TrboNet
{
    public sealed partial class TurboController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TurboController instance = null;
        private static readonly object lockObject = new object();
        private static Client trboNetClient = new Client();

        // This is a dictionary with DeviceID --> Operational info
        private static ConcurrentDictionary<int, DeviceInfo> devices = new ConcurrentDictionary<int, DeviceInfo>();
        // This is a dictionary with RadioID --> Settings
        private static ConcurrentDictionary<int, Radio> radios = new ConcurrentDictionary<int, Radio>();

        private static LinkedList<RequestMessage> pollQueue = new LinkedList<RequestMessage>();
        private static Timer heartBeat;

        #region Instance

        public TurboController()
        {
            SystemSettings settings = new SystemSettings();
            heartBeat = new Timer();
            // lets say we want a minimum of 250 ms now
            int serverInterval = Math.Max(250, settings.ServerInterval ?? 0);
            heartBeat.Interval = serverInterval;
            heartBeat.Elapsed += TheServerDidATick;
            heartBeat.AutoReset = true;
            heartBeat.Enabled = true;
        }

        private void TheServerDidATick(object sender, ElapsedEventArgs e)
        {
            try
            {
                logger.Debug("The server did a tick");
                // populate the queue
                PopulateQueue();
                // Request info for next device
                HandleQueue();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Something went horribly wrong during the ServerTick");
            }
        }

        public static TurboController Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new TurboController();
                    }

                    return instance;
                }
            }
        }

        #endregion

        #region Connection

        private bool Connected = false;
        private object ConnectLock = new object();

        private void Connect()
        {
            try
            {
                lock (ConnectLock)
                {
                    if (!Connected)
                    {
                        ConnectToTurboNet();
                        Connected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void ConnectToTurboNet()
        {
            logger.Info("Connect to turbonet server");
            SystemSettings settings = new SystemSettings();
            trboNetClient.Disconnect();
            trboNetClient.Connect(
                new NS.Shared.Network.NetworkConnectionParam(settings.Settings.Host, (int)settings.Settings.Port),
                new UserInfo(settings.Settings.User, settings.Settings.Password), ClientInitFlags.Empty);
            if (trboNetClient.IsStarted)
            {
                logger.Info("Connected to turbonet server");
            }

            trboNetClient.GetAllWorkflowCommands();

            LoadDeviceList();

            trboNetClient.DevicesChanged += DevicesChanged;

            trboNetClient.DeviceLocationChanged += DeviceLocationChanged;
            trboNetClient.DeviceStateChanged += DeviceStateChanged;
            /*
            trboNetClient.TransmitReceiveChanged += trboNetClient_TransmitReceiveChanged;
            trboNetClient.DeviceTelemetryChanged += trboNetClient_DeviceTelemetryChanged;
            trboNetClient.WorkflowCommandFinished += trboNetClient_WorkflowCommandFinished;
            */
        }

        private void DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            try
            {
                logger.Info("DeviceStateChanged");

                foreach (var radio in e.Infos)
                {
                    int deviceID = radio.DeviceId;
                    if (GetDeviceInfoByDeviceID(deviceID, out DeviceInfo deviceInformation))
                    {
                        logger.Info($"DeviceStateChanged [{deviceInformation.RadioID}]: {radio.State.ToString()}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }


        private bool GetDeviceInfoByRadioID(int radioID, out DeviceInfo deviceInfo)
        {
            deviceInfo = devices.Where(d => d.Value.RadioID == radioID).FirstOrDefault().Value;
            return deviceInfo != null;
        }

        private bool GetDeviceInfoByDeviceID(int deviceID, out DeviceInfo deviceInfo)
        {
            bool deviceFound = devices.TryGetValue(deviceID, out deviceInfo);
            if (!deviceFound)
            {
                logger.Warn($"Can't find device with deviceID {deviceID}");
            }
            return deviceFound;
        }

        private bool GetRadioByRadioID(int radioID, out Radio radio)
        {
            bool radioFound = radios.TryGetValue(radioID, out radio);
            if (!radioFound)
            {
                logger.Warn($"Can't find radio settings for radioID {radioID}");
            }
            return radioFound;
        }



        private void DeviceLocationChanged(object sender, DeviceLocationChangedEventArgs e)
        {
            try
            {
                logger.Info("DeviceLocationChanged");

                foreach (var gpsInfo in e.GPSData)
                {
                    int deviceID = gpsInfo.DeviceID;

                    GpsMeasurement gpsMeasurement = new GpsMeasurement
                    {
                        Latitude = gpsInfo.Latitude,
                        Longitude = gpsInfo.Longitude,
                        Timestamp = gpsInfo.InfoDate.ToString(), // right formatting?
                        Rssi = gpsInfo.Rssi
                    };

                    DeviceInfo deviceInfo;
                    if (!GetDeviceInfoByDeviceID(deviceID, out deviceInfo))
                    {
                        logger.Warn($"Could not find deviceInfo to update GPSfor device with, created it for deviceID {deviceID} ");
                        devices.TryAdd(deviceID, new DeviceInfo(deviceID));
                    }

                    int radioID = deviceInfo.RadioID;
                    gpsMeasurement.RadioID = radioID;
                    deviceInfo.GpsLocations.Push(gpsMeasurement);

                    // Previous existing logging magic
                    StringBuilder build = new StringBuilder();
                    build.Append("DeviceLocationChanged");
                    build.Append("Altitude: " + gpsInfo.Altitude + " ");
                    build.Append("Description: " + gpsInfo.Description + " ");
                    build.Append("DeviceID: " + gpsInfo.DeviceID + " ");
                    build.Append("Direction: " + gpsInfo.Direction + " ");
                    build.Append("GpsSource: " + gpsInfo.GpsSource + " ");
                    build.Append("InfoDate: " + gpsInfo.InfoDate.ToString() + " ");
                    build.Append("InfoDateUtc: " + gpsInfo.InfoDateUtc.ToString() + " ");
                    build.Append("Latitude: " + gpsInfo.Latitude.ToString() + " ");
                    build.Append("Name: " + gpsInfo.Name + " ");
                    build.Append("Radius: " + gpsInfo.Radius.ToString() + " ");
                    build.Append("ReportId: " + gpsInfo.ReportId.ToString() + " ");
                    build.Append("Rssi: " + gpsInfo.Rssi.ToString() + " ");
                    build.Append("Speed: " + gpsInfo.Speed.ToString() + " ");
                    build.Append("StopTime: " + gpsInfo.StopTime.ToString() + " ");

                    logger.Info(build.ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void DevicesChanged(object sender, BindableCollectionEventArgs2<Device> e)
        {
            try
            {
                logger.Info("DevicesChanged");
                Device device = e.ChangedObject;
                int deviceID = e.ChangedObject.ID;
                switch (e.Action)
                {
                    case NS.Shared.Common.ChangeAction.Add:
                        AddOrUpdateDevice(device);
                        break;
                    case NS.Shared.Common.ChangeAction.Remove:
                        RemoveDevice(deviceID);
                        break;
                    case NS.Shared.Common.ChangeAction.ItemChanged:
                        AddOrUpdateDevice(device);
                        break;
                    case NS.Shared.Common.ChangeAction.MuchChanges:
                    case NS.Shared.Common.ChangeAction.SeveralChanges:
                        LoadDeviceList();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private void RemoveDevice(int deviceID)
        {
            devices.Remove(deviceID, out DeviceInfo deviceInfo);
            logger.Info($"Remove device for deviceID {deviceID} radio {deviceInfo?.RadioID}");
        }

        private void QueryLocation(int radioID)
        {
            logger.Info($"Getting location for device with radioID {radioID}");
            if (GetDeviceInfoByRadioID(radioID, out DeviceInfo deviceInfo) && deviceInfo?.Device != null)
            {
                Connect();
                trboNetClient.QueryDeviceLocation(deviceInfo.Device, "", out DeviceCommand cmd);
                logger.Debug($"response from querydevicelocation {cmd}");
            }
            else
            {
                logger.Warn($"Could not query location for device with radioID {radioID}");
            }
        }

        private void LoadDeviceList()
        {
            List<Device> registeredDevices = trboNetClient.LoadRegisteredDevicesFromServer();
            // Unregistered devices cannot be polled - let's not add them at the moment
            // List<Device> unregisteredDevices = trboNetClient.LoadUnregisteredDevicesFromServer();

            registeredDevices.ForEach(d => AddOrUpdateDevice(d));
            // unregisteredDevices.ForEach(d => AddOrUpdateDevice(d));
        }

        #endregion

        #region Queue-shizzle

        /// <summary>
        /// populates the queue with devices that need an locationUpdate
        /// </summary>
        private void PopulateQueue()
        {
            foreach (KeyValuePair<int, DeviceInfo> device in devices)
            {
                int deviceID = device.Key;
                DeviceInfo deviceInfo = device.Value;
                if (deviceInfo == null)
                {
                    // We don't have deviceinfo
                    continue;
                }

                int radioID = deviceInfo.RadioID;
                if (!GetRadioByRadioID(radioID, out Radio radio))
                {
                    // We have no settings
                    continue;
                }

                bool isInterval = radio.RadioSettings?.GpsMode == GpsModeEnum.Interval;
                int minimumInterval = (radio.RadioSettings?.RequestInterval ?? 0);
                bool hasValidInterval = minimumInterval > 0;
                if (!isInterval || !hasValidInterval)
                {
                    // Not an interval radio
                    continue;
                }

                double secondsSinceUpdate = (DateTime.Now - deviceInfo.LastUpdate).TotalSeconds;
                double secondsTillUpdate = minimumInterval - secondsSinceUpdate;
                if (secondsTillUpdate > 0)
                {
                    // Not ready for update
                    continue;
                }

                var requestMessage = CreateGpsRequestMessage(deviceID);
                AddToTheQueue(requestMessage);
            }

        }

        public void PollForGps(params int[] radioIds)
        {
            foreach (int radioID in radioIds)
            {
                if (GetDeviceInfoByRadioID(radioID, out DeviceInfo deviceInfo) && GetRadioByRadioID(radioID, out Radio radio))
                {
                    GpsModeEnum gpsMode = radio?.RadioSettings?.GpsMode ?? GpsModeEnum.None;
                    if (gpsMode == GpsModeEnum.None)
                    {
                        var requestMessage = CreateGpsRequestMessage(deviceInfo.DeviceID);
                        JumpTheQueue(requestMessage);
                    }
                    else
                    {
                        logger.Info($"Could not poll info for radioID {radioID} since GpsMode is none");
                    }
                }
                else
                {
                    logger.Info($"Could not poll info for radioID {radioID} since there was no deviceInfo or RadioSettings available");
                }
            }
        }

        private RequestMessage CreateGpsRequestMessage(int deviceID)
        {
            return new RequestMessage(deviceID, RequestMessage.RequestType.Gps);
        }

        private RequestMessage Peek()
        {
            lock (pollQueue)
            {
                return pollQueue.First();
            }
        }


        private RequestMessage pop()
        {
            lock (pollQueue)
            {
                RequestMessage requestMessage = pollQueue.First();
                pollQueue.RemoveFirst();
                return requestMessage;
            }
        }

        public List<RequestMessage> GetRequestQueue()
        {
            lock (pollQueue)
            {
                return new List<RequestMessage>(pollQueue);
            }
        }

        private void HandleQueue()
        {
            var requestMessage = pop();
            QueryLocation(requestMessage);
        }

        private void QueryLocation(RequestMessage rm)
        {
            int deviceID = rm.deviceID;
            logger.Info($"Getting location for device with deviceID {deviceID}");
            if (GetDeviceInfoByDeviceID(deviceID, out DeviceInfo deviceInfo) && deviceInfo?.Device != null)
            {
                Connect();
                trboNetClient.QueryDeviceLocation(deviceInfo.Device, "", out DeviceCommand cmd);
                logger.Debug($"response from querydevicelocation {cmd}");
            }
            else
            {
                logger.Warn($"Could not query location for device with deviceID {rm.deviceID}");
            }
        }

        private void JumpTheQueue(params RequestMessage[] messages)
        {
            lock (pollQueue)
            {
                foreach (RequestMessage m in messages)
                {
                    pollQueue.AddFirst(m);
                }
            }
        }

        private void AddToTheQueue(params RequestMessage[] messages)
        {
            lock (pollQueue)
            {
                foreach (RequestMessage m in messages)
                {
                    logger.Debug($"Added to the queue {m.deviceID}, {m.Type.ToString()}");
                    pollQueue.AddLast(m);
                }
            }
        }

        #endregion


        private void AddOrUpdateDevice(Device device)
        {
            if (device != null)
            {
                int deviceID = device.ID;
                int radioID = device.RadioID;

                logger.Info($"AddOrUpdate deviceinfo for device with deviceID {deviceID} and radioID {radioID}");

                // Add device
                devices.AddOrUpdate(deviceID, new DeviceInfo(device), (deviceID, oldInfo) =>
                {
                    logger.Info($"Updating deviceinfo for device with deviceID {deviceID} and radioID {radioID}");
                    oldInfo.UpdateDevice(device);
                    return oldInfo;
                });


                // Add settings
                if (radios.TryAdd(radioID, new Radio(radioID)))
                {
                    logger.Info($"Created standard settings for Radio with radioID {radioID}");
                }
            }
            else
            {
                logger.Warn("Can't add null device.");
            }
        }

        /// <summary>
        /// Updates configurable device settings from API object.
        /// If Device is not yet known (aka seen in TrboNet network), creates a stub with supplied settings in case we see it later.
        /// </summary>
        /// <param name="device">Device from API</param>
        public void AddOrUpdateDeviceSettings(Radio radio)
        {
            var radioID = radio.RadioId;
            radios.AddOrUpdate(radioID, radio, (radioID, oldInfo) =>
            {
                return radio;
            });

        }

        public List<DeviceInfo> GetDevices()
        {
            return new List<DeviceInfo>(devices.Values);
        }

        public List<Radio> GetSettings()
        {
            return new List<Radio>(radios.Values);
        }

    }
}