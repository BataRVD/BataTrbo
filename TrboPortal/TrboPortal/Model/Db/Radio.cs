using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrboPortal.Model.Db
{
    /// <summary>
    /// Model used for database storage, when changing please look into the default migration options of entityframework
    /// </summary>
    public class Radio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RadioId { get; set; }
        public string Name { get; set; }
        public string GpsMode { get; set; }
        public int RequestInterval { get; set; }
    }
}
