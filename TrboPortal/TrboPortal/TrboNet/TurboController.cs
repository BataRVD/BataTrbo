﻿using NLog;
using NS.Enterprise.ClientAPI;
using NS.Enterprise.Objects;
using NS.Enterprise.Objects.Devices;
using NS.Enterprise.Objects.Event_args;
using NS.Enterprise.Objects.Users;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TrboPortal.Controllers;
using Device = NS.Enterprise.Objects.Devices.Device;

namespace TrboPortal.TrboNet
{
    public sealed class TurboController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TurboController instance = null;
        private static readonly object lockObject = new object();
        private static Client trboNetClient = new Client();

        private static ConcurrentDictionary<int, DeviceInformation> devices =
            new ConcurrentDictionary<int, DeviceInformation>();

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
                populateQueue();
                // Request info for next device
                handleQueue();
            } catch (Exception ex)
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
            trboNetClient.Connect(new NS.Shared.Network.NetworkConnectionParam(settings.Settings.Host, (int)settings.Settings.Port), new UserInfo(settings.Settings.User, settings.Settings.Password), ClientInitFlags.Empty);
            if (trboNetClient.IsStarted)
            {
                logger.Info("Connected to turbonet server");
            }

            trboNetClient.GetAllWorkflowCommands();

            LoadDeviceList();

            trboNetClient.DevicesChanged += DevicesChanged;
            //trboNetClient.DeviceLocationChanged += DeviceLocationChanged;
            /*
            trboNetClient.DeviceStateChanged += trboNetClient_DeviceStateChanged;
            trboNetClient.TransmitReceiveChanged += trboNetClient_TransmitReceiveChanged;
            trboNetClient.DeviceTelemetryChanged += trboNetClient_DeviceTelemetryChanged;
            trboNetClient.WorkflowCommandFinished += trboNetClient_WorkflowCommandFinished;
            */
        }
        /*
        private void DeviceLocationChanged(object sender, DeviceLocationChangedEventArgs e)
        {
            try
            {
                logger.Info("DeviceLocationChanged");

                foreach (var gpsInfo in e.GPSData)
                {
                    Device device;
                    devices.TryGetValue()
                        device = devices.FirstOrDefault(r => r.ID == gpsInfo.DeviceID);

                        if (device == null)
                        {
                            devices.Clear();
                            devices = m_client.LoadRegisteredDevicesFromServer();

                            foreach (var dev in m_client.LoadUnregisteredDevicesFromServer())
                            {
                                dev.Name = "Radio " + dev.RadioID;
                                devices.Add(dev);
                            }

                            device = devices.FirstOrDefault(r => r.ID == gpsInfo.DeviceID);
                        }
                    

                    if (device != null)
                    {
                        StringBuilder build = new StringBuilder();
                        build.Append("DeviceLocationChanged");
                        build.Append("device: " + device.Name + " ");
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

                        if (gpsInfo.Longitude != 0 && gpsInfo.Longitude != 0)
                        {
                            var gpsInfo = new GPSLocation();
                            gpsInfo.deviceName = device.Name;
                            gpsInfo.RadioID = device.RadioID;
                            gpsInfo.Latitude = gpsInfo.Latitude;
                            gpsInfo.Longitude = gpsInfo.Longitude;
                            gpsInfo.Rssi = gpsInfo.Rssi;

                            PostGpsLocation(gpsInfo);
                        }

                        PostDeviceLifeSign(device.Name, device.RadioID, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }
        */
        private void DevicesChanged(object sender, BindableCollectionEventArgs2<Device> e)
        {
            try
            {

                logger.Info("DevicesChanged");
                Device device = e.ChangedObject;
                int radioID = e.ChangedObject.RadioID;
                switch (e.Action)
                    {
                        case NS.Shared.Common.ChangeAction.Add:
                            AddOrUpdateDevice(device);
                            break;
                        case NS.Shared.Common.ChangeAction.Remove:
                            devices.Remove(radioID, out DeviceInformation removedInformation);
                            break;
                        case NS.Shared.Common.ChangeAction.ItemChanged:
                            AddOrUpdateDevice(device);
                            break;
                        case NS.Shared.Common.ChangeAction.MuchChanges:
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

        private void queryLocation(int deviceID)
        {
            logger.Info($"Getting location for device {deviceID}");
            if (devices.TryGetValue(deviceID, out DeviceInformation deviceInfo))
            {
                Connect();
                trboNetClient.QueryDeviceLocation(deviceInfo.device, "", out DeviceCommand cmd);
                logger.Debug($"response from querydevicelocation {cmd}");
            }
            else
            {
                logger.Warn($"Could not query location for device {deviceID}");
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
        private void populateQueue()
        {
            RequestMessage[] devicesToQueue = devices
                .Where(d => d.Value.gpsMode == GpsModeEnum.Interval) // Devices that are on interval mode
                .Where(d => d.Value.TimeTillUpdate() < 0) // That are due an update
                .Where(d => pollQueue.Select(pq => pq.DeviceId).ToList()
                .Contains(d.Key)) // That are not already in the queue
                .Select(d => ReturnBullshit(d.Key))
                .ToArray();

            addToTheQueue(devicesToQueue);
        }


        private RequestMessage ReturnBullshit(int deviceId)
        {
            return new RequestMessage(deviceId);
        }

        private RequestMessage peek()
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

        private void handleQueue()
        {
            var requestMessage = pop();
            queryLocation(requestMessage);
        }

        private void queryLocation(RequestMessage rm)
        {
            logger.Info($"Getting location for device {rm.DeviceId}");
            if (devices.TryGetValue(rm.DeviceId, out DeviceInformation deviceInfo))
            {
                Connect();
                trboNetClient.QueryDeviceLocation(deviceInfo.device, "", out DeviceCommand cmd);
                logger.Debug($"response from querydevicelocation {cmd}");
            }
            else
            {
                logger.Warn($"Could not query location for device {rm.DeviceId}");
            }
        }

        private void jumpTheQueue(params RequestMessage[] messages)
        {
            lock (pollQueue)
            {
                foreach (RequestMessage m in messages)
                {
                    pollQueue.AddFirst(m);
                }
            }
        }

        private void addToTheQueue(params RequestMessage[] messages)
        {
            lock (pollQueue)
            {
                foreach (RequestMessage m in messages)
                {
                    pollQueue.AddLast(m);
                }
            }
        }

        #endregion


        private void AddOrUpdateDevice(Device device)
        {
            int deviceID = device.RadioID;
            devices.AddOrUpdate(deviceID, new DeviceInformation(deviceID, device), (deviceID, oldInfo) =>
            {
                oldInfo.UpdateDevice(device);
                return oldInfo;
            });
        }

        public List<DeviceInformation> GetDevices()
        {
            return devices.Select(d => d.Value
            ).ToList();
        }
    }
}