﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrboPortal.Model.Db
{
    /// <summary>
    /// Model used for database storage, when changing please look into the default migration options of entityframework
    /// </summary>
    public class GpsEntry
    {
        [Key] public int GpsEntryId { get; set; }
        /*[ForeignKey("RadioId")]*/ public int RadioId { get; set; }
        public int? DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Rssi { get; set; }
    }
}