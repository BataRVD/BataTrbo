using NLog;
using NS.Enterprise.ClientAPI;
using NS.Enterprise.Objects;
using NS.Enterprise.Objects.Devices;
using NS.Enterprise.Objects.Event_args;
using NS.Enterprise.Objects.Users;
using NS.Shared.AsyncQueue;
using NS.Shared.Network;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TrboPortal.TrboNet.Client
{


    internal class TrboClient : ITrboClient
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly NS.Enterprise.ClientAPI.Client client;

        public TrboClient()
        {
            client = new NS.Enterprise.ClientAPI.Client();
        }

        public bool IsStarted => client.IsStarted;


        public void Connect(NetworkConnectionParam serverParams, UserInfo user, ClientInitFlags flags)
        {
            client.Connect(serverParams, user, flags);
        }

        public void DeviceLocationChanged(EventHandler<DeviceLocationChangedEventArgs> deviceLocationChanged)
        {
            client.DeviceLocationChanged += deviceLocationChanged;
        }

        public void DevicesChanged(EventHandler<BindableCollectionEventArgs2<Device>> devicesChanged)
        {
            client.DevicesChanged += devicesChanged;
        }

        public void DeviceStateChanged(EventHandler<DeviceStateChangedEventArgs> deviceStateChanged)
        {
            client.DeviceStateChanged += deviceStateChanged;
        }

        public void DeviceTelemetryChanged(EventHandler<DeviceTelemetryChangedEventArgs> deviceTelemetryChanged)
        {
            client.DeviceTelemetryChanged += deviceTelemetryChanged;
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        public List<DeviceCommand> GetAllWorkflowCommands()
        {
            return client.GetAllWorkflowCommands();
        }

        public List<Device> LoadRegisteredDevicesFromServer()
        {
            return client.LoadRegisteredDevicesFromServer();
        }

        public int QueryDeviceLocation(Device device, string caption, out DeviceCommand cmd)
        {
            var returnVal =  client.QueryDeviceLocation(device, caption, out cmd);
            var queue = GetDataQueue();
            return returnVal;
        }

        public void TransmitReceiveChanged(EventHandler<TransmitReceiveArgs> transmitReceiveChanged)
        {
            client.TransmitReceiveChanged += transmitReceiveChanged;
        }

        public void WorkflowCommandFinished(EventHandler<WorkflowCommandFinishedEventArgs> workflowCommandFinished)
        {
            client.WorkflowCommandFinished += workflowCommandFinished;
        }

        /// <summary>
        /// Returns TrboNet internal DataQueue sizer (using reflection)
        /// </summary>
        /// <returns></returns>
        public long GetInternalQueueCount()
        {
            var queue = GetDataQueue();
            if(queue == null)
            {
                return -1;
            }
            var queueSize = 0;
            foreach (var item in queue.EnumAllItems())
            {
                queueSize++;
            }
            return queueSize;
        }

        /// <summary>
        /// Clear internal DataQueue (using reflection) 
        /// </summary>
        /// <returns>Success or not</returns>
        public bool ClearInternalQueue()
        {
            var success = false;
            var queue = GetDataQueue();
            if (queue != null)
            {
                try
                {
                    queue.ClearPendingData();
                    success = true;
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to clear internal Queue. {ex}");
                }
            }
            return success;
        }

        /// <summary>
        /// Get TrboNet internal DataQueue using reflection
        /// </summary>
        /// <returns></returns>
        private AQueue<NetworkCommand> GetDataQueue()
        {
            try
            {
                var queue = client.GetFieldValue<AQueue<NetworkCommand>>("m_dataQueue");
                return queue;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to get DataQueue using reflection. {ex}");
                return null;
            }
        }

       
    }
    public static class ReflectionExtensions
    {
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }
    }
}