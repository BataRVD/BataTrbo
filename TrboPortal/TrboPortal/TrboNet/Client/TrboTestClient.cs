using NS.Enterprise.Objects;
using NS.Enterprise.Objects.Devices;
using NS.Enterprise.Objects.Event_args;
using NS.Enterprise.Objects.Users;
using NS.Shared.Network;
using System;
using System.Collections.Generic;

namespace TrboPortal.TrboNet.Client
{
    internal class TrboTestClient : ITrboClient
    {
        public bool IsStarted => throw new NotImplementedException();

        public void Connect(NetworkConnectionParam networkConnectionParam, UserInfo userInfo, ClientInitFlags empty)
        {
            throw new NotImplementedException();
        }

        public void DeviceLocationChanged(EventHandler<DeviceLocationChangedEventArgs> deviceLocationChanged)
        {
            throw new NotImplementedException();
        }

        public void DevicesChanged(EventHandler<BindableCollectionEventArgs2<Device>> devicesChanged)
        {
            throw new NotImplementedException();
        }

        public void DeviceStateChanged(EventHandler<DeviceStateChangedEventArgs> deviceStateChanged)
        {
            throw new NotImplementedException();
        }

        public void DeviceTelemetryChanged(EventHandler<DeviceTelemetryChangedEventArgs> deviceTelemetryChanged)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public List<DeviceCommand> GetAllWorkflowCommands()
        {
            throw new NotImplementedException();
        }

        public List<Device> LoadRegisteredDevicesFromServer()
        {
            throw new NotImplementedException();
        }

        public int QueryDeviceLocation(Device device, string v, out DeviceCommand cmd)
        {
            throw new NotImplementedException();
        }

        public void TransmitReceiveChanged(EventHandler<TransmitReceiveArgs> transmitReceiveChanged)
        {
            throw new NotImplementedException();
        }

        public void WorkflowCommandFinished(EventHandler<WorkflowCommandFinishedEventArgs> workflowCommandFinished)
        {
            throw new NotImplementedException();
        }
    }
}