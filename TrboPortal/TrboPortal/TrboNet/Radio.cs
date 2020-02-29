namespace TrboPortal.Controllers
{
    /// <summary>
    /// Class with (only) settings regarding this radio
    /// </summary>
    public partial class Radio
    {
        // TODO standard settings in constructor ?
        public Radio(int radioID)
        {
            this.Name = $"Radio {radioID}";
            this.RadioId = radioID;
            GpsMode = GpsModeEnum.None;
        }

        public Radio()
        {
            // empty constructor needed for reasons
        }
    }
}