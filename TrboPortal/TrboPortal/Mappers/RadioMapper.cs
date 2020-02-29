using TrboPortal.Controllers;

namespace TrboPortal.Mappers
{
    public class RadioMapper
    {
        public static Model.Radio MapRadioSettings(Controllers.Radio rs)
        {
            return new Model.Radio
            {
                RadioId = (int)rs.RadioId,
                GpsMode = rs.GpsMode?.ToString(), //TODO: Valued enum?
                RequestInterval = (int)rs.RequestInterval
            };
            
        }
    }
}