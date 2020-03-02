namespace TrboPortal.Model.Api
{
    /// <summary>GPS polling mode</summary>
    
    public enum GpsModeEnum
    {
        [System.Runtime.Serialization.EnumMember(Value = @"none")]
        None = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"pull")]
        Pull = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"interval")]
        Interval = 2,

    }
}