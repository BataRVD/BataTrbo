using Radio = TrboPortal.Model.Api.Radio;

namespace TrboPortal.Mappers
{
    public class RadioMapper
    {
        public static Model.Db.Radio MapRadioSettings(Radio rs)
        {
            return new Model.Db.Radio
            {
                RadioId = rs.RadioId,
                Name = rs.Name,
                GpsMode = rs.GpsMode?.ToString(), 
                RequestInterval = (int)rs.RequestInterval
            };
            
        }
    }
}