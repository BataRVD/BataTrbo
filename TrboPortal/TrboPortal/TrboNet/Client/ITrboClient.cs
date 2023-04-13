using NS.Enterprise.Objects;
using NS.Enterprise.Objects.Devices;
using NS.Enterprise.Objects.Event_args;
using NS.Enterprise.Objects.Users;
using NS.Shared.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrboPortal.TrboNet.Client
{
    interface ITrboClient
    {
        bool IsStarted { get; }

        void Disconnect();
        void Connect(NetworkConnectionParam networkConnectionParam, UserInfo userInfo, ClientInitFlags empty);
        List<DeviceCommand> GetAllWorkflowCommands();
        List<Device> LoadRegisteredDevicesFromServer();
        void DevicesChanged(EventHandler<BindableCollectionEventArgs2<Device>> devicesChanged);
        void DeviceLocationChanged(EventHandler<DeviceLocationChangedEventArgs> deviceLocationChanged);
        void DeviceStateChanged(EventHandler<DeviceStateChangedEventArgs> deviceStateChanged);
        void DeviceTelemetryChanged(EventHandler<DeviceTelemetryChangedEventArgs> deviceTelemetryChanged);
        void WorkflowCommandFinished(EventHandler<WorkflowCommandFinishedEventArgs> workflowCommandFinished);
        int QueryDeviceLocation(Device device, string v, out DeviceCommand cmd);
        long GetInternalQueueCount();

        bool ClearInternalQueue();
    }
}
