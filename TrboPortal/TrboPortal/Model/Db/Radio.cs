using System.ComponentModel.DataAnnotations;

namespace TrboPortal.Model.Db
{
    /// <summary>
    /// Model used for database storage, when changing please look into the default migration options of entityframework
    /// </summary>
    public class Radio
    {
        [Key]
        public int RadioId { get; set; }
        public string Name { get; set; }
        public string GpsMode { get; set; }
        public int RequestInterval { get; set; }
    }
}
