using System;

public class RadioException : Exception
{
    public int RadioId { get; private set; }
    public string Message { get; private set; }
    public RadioException(int radioId, string message) : base()
    {
        RadioId = radioId;
        Message = message;
    }

    public override string ToString()
    {
        return $"Radio - {RadioId}: {Message}";
    }
}