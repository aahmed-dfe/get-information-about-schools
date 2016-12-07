﻿using Edubase.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edubase.Services.Domain
{
    public class ChildrensCentreLocalAuthorityDto
    {
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string FullName => Common.StringUtil.ConcatNonEmpties(" ", FirstName, Surname);
        public string EmailAddress { get; set; }
        public string TelephoneNumber { get; set; }

        public ChildrensCentreLocalAuthorityDto()
        {

        }

        public ChildrensCentreLocalAuthorityDto(LocalAuthority la)
        {
            FirstName = la.FirstName;
            Surname = la.LastName;
            EmailAddress = la.EmailAddress;
            //TelephoneNumber=la.telephonenumber//todo, not in source data at dev time.
        }
    }
}
