public class RadioNotFoundException : RadioException
{
    public RadioNotFoundException(int radioId, string message) : base(radioId, message) { }
}