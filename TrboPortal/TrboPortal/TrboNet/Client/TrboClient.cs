using NS.Enterprise.ClientAPI;
using NS.Enterprise.Objects;
using NS.Enterprise.Objects.Devices;
using NS.Enterprise.Objects.Event_args;
using NS.Enterprise.Objects.Users;
using NS.Shared.Network;
using System;
using System.Collections.Generic;

namespace TrboPortal.TrboNet.Client
{


    internal class TrboClient : ITrboClient
    {
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
            return client.QueryDeviceLocation(device, caption, out cmd);
        }

        public void TransmitReceiveChanged(EventHandler<TransmitReceiveArgs> transmitReceiveChanged)
        {
            client.TransmitReceiveChanged += transmitReceiveChanged;
        }

        public void WorkflowCommandFinished(EventHandler<WorkflowCommandFinishedEventArgs> workflowCommandFinished)
        {
            client.WorkflowCommandFinished += workflowCommandFinished;
        }
    }
}