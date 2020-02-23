using NLog;
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

namespace TrboPortal.TrboNet
{
    public sealed class TurboController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TurboController instance = null;
        private static readonly object lockObject = new object();
        private static Client trboNetClient = new Client();
        private static ConcurrentDictionary<int, DeviceInformation> devices = new ConcurrentDictionary<int, DeviceInformation>();
        private static LinkedList<int> pollQueue = new LinkedList<int>();

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

            trboNetClient.Disconnect();
            //trboNetClient.Connect(new NS.Shared.Network.NetworkConnectionParam(Properties.Settings.Default.TurboNetHost, Properties.Settings.Default.TurboNetPort), new UserInfo(Properties.Settings.Default.TurboNetUser, Properties.Settings.Default.TurboNetPassword), ClientInitFlags.Empty);
            if (trboNetClient.IsStarted)
            {
                logger.Info("Connected to turbonet server");
            }

            trboNetClient.GetAllWorkflowCommands();

            LoadDeviceList();

            /*
            trboNetClient.BeaconSignal += trboNetClient_BeaconSignal;
            trboNetClient.DevicesChanged += DevicesChanged;
            trboNetClient.DeviceLocationChanged += DeviceLocationChanged;
            trboNetClient.DeviceStateChanged += trboNetClient_DeviceStateChanged;
            trboNetClient.TransmitReceiveChanged += trboNetClient_TransmitReceiveChanged;
            trboNetClient.DeviceTelemetryChanged += trboNetClient_DeviceTelemetryChanged;
            trboNetClient.WorkflowCommandFinished += trboNetClient_WorkflowCommandFinished;
            */
        }

        /// <summary>
        /// populates the queue with devices that need an locationUpdate
        /// </summary>
        private void populateQueue()
        {
            int[] devicesToQueue = devices
                .Where(d => d.Value.gpsMode == GpsMode.interval)    // Devices that are on interval mode
                .Where(d => d.Value.TimeTillUpdate() < 0)           // That are due an update
                .Where(d => pollQueue.Contains(d.Key))              // That are not already in the queue
                .Select(d => d.Key)
                .ToArray();

            addToTheQueue(devicesToQueue);
        }

        private int peek()
        {
            lock (pollQueue)
            {
                int deviceID = pollQueue.First();
                return deviceID;
            }
        }


        private int pop()
        {
            lock (pollQueue)
            {
                int deviceID = pollQueue.First();
                pollQueue.RemoveFirst();
                return deviceID;
            }
        }

        private void handleQueue()
        {
            int deviceID = pop();
            queryLocation(deviceID);
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

        private void jumpTheQueue(params int[] deviceIDs)
        {
            lock (pollQueue)
            {
                foreach (int deviceID in deviceIDs)
                {
                    pollQueue.AddFirst(deviceID);
                }
            }
        }

        private void addToTheQueue(params int[] deviceIDs)
        {
            lock (pollQueue)
            {
                foreach (int deviceID in deviceIDs)
                {
                    pollQueue.AddLast(deviceID);
                }
            }
        }

        private void LoadDeviceList()
        {
            devices.Clear();

            List<Device> registeredDevices = trboNetClient.LoadRegisteredDevicesFromServer();
            List<Device> unregisteredDevices = trboNetClient.LoadUnregisteredDevicesFromServer();

            registeredDevices.ForEach(d => AddOrUpdateDevice(d));
            unregisteredDevices.ForEach(d => AddOrUpdateDevice(d));
        }

        private void AddOrUpdateDevice(Device device)
        {
            int deviceID = device.RadioID;
            devices.TryAdd(deviceID, new DeviceInformation(deviceID, device));
            devices.AddOrUpdate(deviceID, new DeviceInformation(deviceID, device), (deviceID, oldInfo) => { oldInfo.UpdateDevice(device); return oldInfo; });
        }
    }
}
