using NS.Enterprise.Objects.Devices;
using Device = NS.Enterprise.Objects.Devices.Device;

namespace TrboPortal.TrboNet
{
    public class DeviceInfo
    {
        public Device Device { get; private set; }
        public int RadioID { get; private set; }

        public DeviceInfo(Device device)
        {
            this.Device = device;
            this.RadioID = device.RadioID;
        }

        internal void UpdateDevice(Device device)
        {
            Device = device;
            this.RadioID = device.RadioID;
        }
    }
}