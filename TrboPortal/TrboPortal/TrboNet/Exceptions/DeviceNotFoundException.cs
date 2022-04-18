public class DeviceNotFoundException : RadioException
{
    public DeviceNotFoundException(int radioId, string message) : base(radioId, message) { }
}