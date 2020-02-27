using NLog;
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
        private static ConcurrentDictionary<int, RadioInfo> radios = new ConcurrentDictionary<int, RadioInfo>();


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
            //trboNetClient.DeviceLocationChanged += DeviceLocationChanged;
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
                    if (GetDeviceInformationByDeviceID(deviceID, out DeviceInformation deviceInformation))
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

        private bool GetDeviceInformationByDeviceID(int deviceID, out DeviceInformation deviceInformation)
        {
            throw new NotImplementedException();
            /*

            bool deviceFound = devices.TryGetValue(deviceID, out deviceInformation);
            if (!deviceFound)
            {
                logger.Warn($"Can't find device with deviceID {deviceID}");
            }
            return deviceFound;
            */
        }

        private bool GetDeviceInformationByRadioID(int radioID, out DeviceInformation deviceInformation)
        {
            throw new NotImplementedException();
            /*

            /*
            bool deviceFound = devices.TryGetValue(deviceID, out deviceInformation);
            if (!deviceFound)
            {
                logger.Warn($"Can't find device with radioID {radioID}");
            }
            return deviceFound;
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
                    
                    int deviceID = 


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
            /*
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
                        devices.Remove(deviceID, out DeviceInformation removedInformation);
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
            */
        }

        private void QueryLocation(int radioID)
        {
            logger.Info($"Getting location for device with radioID {radioID}");
            if (GetDeviceInformationByRadioID(radioID, out DeviceInformation deviceInfo))
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
            /*
            RequestMessage[] devicesToQueue = devices
                .Where(d => d.Value.GpsMode == GpsModeEnum.Interval) // Devices that are on interval mode
                .Where(d => d.Value.TimeTillUpdate() < 0) // That are due an update
                .Where(d => pollQueue.Select(pq => pq.Device.DeviceID).ToList()
                    .Contains(d.Key)) // That are not already in the queue
                .Select(d => CreateGpsRequestMessage(d.Value))
                .ToArray();

            AddToTheQueue(devicesToQueue);
            */
        }


        private RequestMessage CreateGpsRequestMessage(DeviceInformation device)
        {
            return new RequestMessage(device, RequestMessage.RequestType.Gps);
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
            int deviceID = rm.Device.DeviceID;
            logger.Info($"Getting location for device with radioID {rm.Device.RadioID}");
            if (GetDeviceInformationByDeviceID(deviceID, out DeviceInformation deviceInfo))
            {
                Connect();
                trboNetClient.QueryDeviceLocation(deviceInfo.Device, "", out DeviceCommand cmd);
                logger.Debug($"response from querydevicelocation {cmd}");
            }
            else
            {
                logger.Warn($"Could not query location for device with radioID {rm.Device.RadioID}");
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
                if (radios.TryAdd(radioID, new RadioInfo()))
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
        public void AddOrUpdateDeviceSettings(Controllers.Device device)
        {
            throw new NotImplementedException();
            /*

            var radioID = device.Id;
            if (GetDeviceInformationByRadioID(radioID, out DeviceInformation deviceInformation))
            {
                devices.AddOrUpdate(radioID, DeviceMapper.MapToDeviceInformation(device), (radioID, oldInfo) =>
                {
                    oldInfo.UpdateDeviceInfo(device);
                    return oldInfo;
                });

            }
            */
        }

        public List<DeviceInformation> GetDevices()
        {
            throw new NotImplementedException();
            /*
            return devices.Select(d => d.Value
            ).ToList();
            */
        }
    }
}