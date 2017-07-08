﻿using System;
using Newtonsoft.Json;

namespace Edubase.Services.Governors.Models
{
    public class GovernorAppointment
    {
        public DateTime? AppointmentStartDate { get; set; }
        public DateTime? AppointmentEndDate { get; set; }
        public int? EstablishmentUrn { get; set; }

        [JsonIgnore]
        public string EstablishmentName { get; set; }
    }
}
