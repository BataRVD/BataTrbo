using Radio = TrboPortal.Model.Api.Radio;

namespace TrboPortal.Mappers
{
    public class RadioMapper
    {
        public static Model.Db.Radio MapRadioSettings(Radio rs)
        {
            return new Model.Db.Radio
            {
                RadioId = (int)rs.RadioId,
                Name = rs.Name,
                GpsMode = rs.GpsMode?.ToString(), //TODO: Valued enum?
                RequestInterval = (int)rs.RequestInterval
            };
            
        }
    }
}