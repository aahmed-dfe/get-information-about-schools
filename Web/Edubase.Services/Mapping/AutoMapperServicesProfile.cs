﻿using AutoMapper;
using Edubase.Common.Formatting.Json;
using Edubase.Common.Spatial;
using Edubase.Data;
using Edubase.Data.Entity;
using Edubase.Data.Entity.ComplexTypes;
using Edubase.Services.Domain;
using Edubase.Services.Enums;
using Edubase.Services.Establishments.Models;
using Edubase.Services.Groups.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.Entity.Spatial;

namespace Edubase.Services.Mapping
{
    public class AutoMapperServicesProfile : Profile
    {
        public AutoMapperServicesProfile()
        {
            CreateMap<DbGeography, LatLon>().ConvertUsing(x => x.ToLatLon());
            CreateMap<LatLon, DbGeography>().ConvertUsing(x => x.ToDBGeography());

            CreateMap<string, List<AdditionalAddressModel>>()
                .ConvertUsing<FromJsonTypeConverter<List<AdditionalAddressModel>>>();

            CreateMap<List<AdditionalAddressModel>, string>()
                .ConvertUsing<ToJsonTypeConverter<List<AdditionalAddressModel>>>();
            
            CreateMap<ContactDetail, ContactDetailDto>();
            CreateMap<Address, EstablishmentAddressModel>();
            CreateMap<Person, PersonDto>();

            CreateMap<Establishment, EstablishmentModel>() // out
                .ForMember(x => x.Location, opt => opt.Ignore())
                .ForMember(x => x.AdditionalAddressesCount, opt => opt.Ignore())
                .ForMember(x => x.GovernanceMode, opt => opt.MapFrom(x => (eGovernanceMode?)x.GovernanceMode))
                .AfterMap((s, d) =>
                {
                    d.Location = s.Location.ToLatLon();
                    if (d.AdditionalAddresses == null) d.AdditionalAddresses = new List<AdditionalAddressModel>();
                })
                .ReverseMap() // in
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.GovernanceMode, opt => opt.MapFrom(x => (byte?)x.GovernanceMode))
                .ForMember(x => x.LAESTAB, opt => opt.Ignore())
                .ForMember(x => x.Address, opt => opt.MapFrom(x => new Address
                {
                    Country = x.Address_Country,
                    County = x.Address_County,
                    Line1 = x.Address_Line1,
                    Line2 = x.Address_Line2,
                    Line3 = x.Address_Line3,
                    Locality = x.Address_Locality,
                    PostCode = x.Address_PostCode,
                    CityOrTown = x.Address_CityOrTown
                }))
                .ForMember(x => x.Contact, opt => opt.MapFrom(x => new ContactDetail
                {
                    EmailAddress = x.Contact_EmailAddress,
                    FaxNumber = x.Contact_FaxNumber,
                    TelephoneNumber = x.Contact_TelephoneNumber,
                    WebsiteAddress = x.Contact_WebsiteAddress
                }))
                .ForMember(x => x.ContactAlt, opt => opt.MapFrom(x => new ContactDetail
                {
                    EmailAddress = x.ContactAlt_EmailAddress,
                    FaxNumber = x.ContactAlt_FaxNumber,
                    TelephoneNumber = x.ContactAlt_TelephoneNumber,
                    WebsiteAddress = x.ContactAlt_WebsiteAddress
                }))
                .ForMember(x => x.AdditionalAddresses, opt => opt.MapFrom(x => x.AdditionalAddresses));

            CreateMap<GroupCollection, GroupModel>()
                .ForMember(x => x.Address, opt => opt.MapFrom(x => new AddressDto { Line1 = x.Address })); // TODO: KHD hack, as we're moving to Texuna - all this will be deleted very soon.
        }
    }
}