﻿using Edubase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edubase.Services.Governors.Models
{
    public class GovernorModel : GovernorModelBase
    {
        public string AppointingBodyName { get; set; }
        public IEnumerable<GovernorAppointment> Appointments { get; set; }
    }
}