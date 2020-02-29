using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrboPortal.Model
{
    /// <summary>
    /// Model used for database storage, when changing please look into the default migration options of entityframework
    /// </summary>
    public class Radio
    {
        public int RadioId { get; set; }
        public string Name { get; set; }
        public string GpsMode { get; set; }
        public int RequestInterval { get; set; }
    }
}
