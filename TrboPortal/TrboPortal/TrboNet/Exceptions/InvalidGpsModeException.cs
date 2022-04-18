public class InvalidGpsModeException : RadioException
{
    public InvalidGpsModeException(int radioId, string message) : base(radioId, message) { }
}