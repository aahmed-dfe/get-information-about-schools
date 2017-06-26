﻿using Edubase.Services.Domain;
using Edubase.Services.Establishments;
using Edubase.Services.Establishments.DisplayPolicies;
using Edubase.Services.Establishments.Models;
using Edubase.Services.Establishments.Search;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Linq;
using Edubase.Services.Texuna.Models;
using Edubase.Common.Reflection;
using Edubase.Services.Lookup;
using Edubase.Common;
using Edubase.Services.Enums;

namespace Edubase.Services.Texuna.Establishments
{
    using ET = eLookupEstablishmentType;
    using EP = eLookupEducationPhase;

    /// <summary>
    /// Implementation of IEstablishmentReadService that will gradually be changed to call the API rather than custom backend
    /// </summary>
    public class EstablishmentReadApiService : IEstablishmentReadService
    {
        private const string ApiSuggestPath = "suggest/establishment";

        private readonly HttpClientWrapper _httpClient;
        private readonly ICachedLookupService _cachedLookupService;

        public EstablishmentReadApiService(HttpClientWrapper httpClient, ICachedLookupService cachedLookupService)
        {
            _httpClient = httpClient;
            _cachedLookupService = cachedLookupService;
        }

        public async Task<ServiceResultDto<bool>> CanAccess(int urn, IPrincipal principal)
            => new ServiceResultDto<bool>((await _httpClient.GetAsync<BoolResult>($"establishment/{urn}/canaccess", principal)).GetResponse().Value);

        public async Task<bool> CanEditAsync(int urn, IPrincipal principal)
            =>  (await _httpClient.GetAsync<BoolResult>($"establishment/{urn}/canedit", principal)).Response.Value;

        public async Task<ServiceResultDto<EstablishmentModel>> GetAsync(int urn, IPrincipal principal)
            => new ServiceResultDto<EstablishmentModel>((await _httpClient.GetAsync<EstablishmentModel>($"establishment/{urn}", principal, false)).GetResponse());

        public async Task<IEnumerable<EstablishmentChangeDto>> GetChangeHistoryAsync(int urn, int take, IPrincipal user)
            => (await _httpClient.GetAsync<List<EstablishmentChangeDto>>($"establishment/{urn}/changes?take={take}", user)).GetResponse();

        public async Task<EstablishmentDisplayEditPolicy> GetDisplayPolicyAsync(EstablishmentModel establishment, IPrincipal user)
            => (await _httpClient.GetAsync<EstablishmentDisplayEditPolicy>($"establishment/{establishment.Urn}/display-policy", user)).GetResponse().Initialise(establishment);

        public async Task<EstablishmentDisplayEditPolicy> GetEditPolicyAsync(EstablishmentModel establishment, IPrincipal user) 
            => (await _httpClient.GetAsync<EstablishmentDisplayEditPolicy>($"establishment/{establishment.Urn}/edit-policy", user)).GetResponse().Initialise(establishment);

        public async Task<IEnumerable<LinkedEstablishmentModel>> GetLinkedEstablishmentsAsync(int urn, IPrincipal principal) 
            => (await _httpClient.GetAsync<List<LinkedEstablishmentModel>>($"establishment/{urn}/linked-establishments", principal)).GetResponse();

        public async Task<List<ChangeDescriptorDto>> GetModelChangesAsync(EstablishmentModel model, IPrincipal principal)
        {
            var originalModel = (await GetAsync(model.Urn.Value, principal)).GetResult();
            return await GetModelChangesAsync(originalModel, model);
        }

        public async Task<int[]> GetPermittedStatusIdsAsync(IPrincipal principal)
            => (await _httpClient.GetAsync<List<LookupDto>>("establishment/permittedstatuses", principal)).GetResponse().Select(x => x.Id).ToArray();

        public async Task<ApiSearchResult<EstablishmentModel>> SearchAsync(EstablishmentSearchPayload payload, IPrincipal principal)
            => (await _httpClient.PostAsync<ApiSearchResult<EstablishmentModel>>("establishment/search", payload, principal)).GetResponse();

        public async Task<IEnumerable<EstablishmentSuggestionItem>> SuggestAsync(string text, IPrincipal principal, int take = 10) 
            => (await _httpClient.GetAsync<List<EstablishmentSuggestionItem>>($"{ApiSuggestPath}?q={text}&take={take}", principal)).GetResponse();
        
        public async Task<List<ChangeDescriptorDto>> GetModelChangesAsync(EstablishmentModel original, EstablishmentModel model)
        {
            var changes = ReflectionHelper.DetectChanges(model, original);
            var retVal = new List<ChangeDescriptorDto>();

            foreach (var change in changes)
            {
                if (_cachedLookupService.IsLookupField(change.Name))
                {
                    change.OldValue = await _cachedLookupService.GetNameAsync(change.Name, change.OldValue.ToInteger());
                    change.NewValue = await _cachedLookupService.GetNameAsync(change.Name, change.NewValue.ToInteger());
                }

                if (change.Name.EndsWith("Id", StringComparison.Ordinal)) change.Name = change.Name.Substring(0, change.Name.Length - 2);
                change.Name = change.Name.Replace("_", "");
                change.Name = change.Name.ToProperCase(true);

                retVal.Add(new ChangeDescriptorDto
                {
                    Name = change.DisplayName ?? change.Name,
                    NewValue = change.NewValue.Clean(),
                    OldValue = change.OldValue.Clean()
                });
            }

            return retVal;
        }

        public async Task<FileDownloadDto> GetChangeHistoryDownloadAsync(int urn, eFileFormat format, IPrincipal principal) 
            => (await _httpClient.GetAsync<FileDownloadDto>($"establishment/{urn}/changes/download?format={format.ToString().ToLower()}", principal)).GetResponse();

        public async Task<FileDownloadDto> GetDownloadAsync(int urn, eFileFormat format, IPrincipal principal)
            => (await _httpClient.GetAsync<FileDownloadDto>($"establishment/{urn}/download?format={format.ToString().ToLower()}", principal)).GetResponse();

        public Dictionary<ET, EP[]> GetEstabType2EducationPhaseMap()
        {
            var retVal = new Dictionary<ET, EP[]>();
            retVal.Add(ET.CommunitySchool, new[] { EP.Nursery, EP.Primary, EP.MiddleDeemedPrimary, EP.Secondary, EP.MiddleDeemedSecondary, EP._16Plus, EP.AllThrough });
            retVal.Add(ET.VoluntaryAidedSchool, new[] { EP.Primary, EP.MiddleDeemedPrimary, EP.Secondary, EP.MiddleDeemedSecondary, EP._16Plus, EP.AllThrough });
            retVal.Add(ET.VoluntaryControlledSchool, new[] { EP.Primary, EP.MiddleDeemedPrimary, EP.Secondary, EP.MiddleDeemedSecondary, EP._16Plus, EP.AllThrough });
            retVal.Add(ET.FoundationSchool, new[] { EP.Primary, EP.MiddleDeemedPrimary, EP.Secondary, EP.MiddleDeemedSecondary, EP._16Plus, EP.AllThrough });

            retVal.Add(ET.CityTechnologyCollege, new[] { EP.NotApplicable });
            retVal.Add(ET.CommunitySpecialSchool, new[] { EP.NotApplicable });
            retVal.Add(ET.NonmaintainedSpecialSchool, new[] { EP.NotApplicable });
            retVal.Add(ET.OtherIndependentSpecialSchool, new[] { EP.NotApplicable });
            retVal.Add(ET.OtherIndependentSchool, new[] { EP.NotApplicable });
            retVal.Add(ET.FoundationSpecialSchool, new[] { EP.NotApplicable });
            retVal.Add(ET.PupilReferralUnit, new[] { EP.NotApplicable });

            retVal.Add(ET.LANurserySchool, new[] { EP.Nursery });
            retVal.Add(ET.FurtherEducation, new[] { EP._16Plus });

            retVal.Add(ET.SecureUnits, new[] { EP.NotApplicable });
            retVal.Add(ET.OffshoreSchools, new[] { EP.NotApplicable });
            retVal.Add(ET.ServiceChildrensEducation, new[] { EP.NotApplicable });
            retVal.Add(ET.Miscellaneous, new[] { EP.NotApplicable });

            retVal.Add(ET.AcademySponsorLed, new[] { EP.Primary, EP.MiddleDeemedPrimary, EP.Secondary, EP.MiddleDeemedSecondary, EP._16Plus, EP.AllThrough });

            retVal.Add(ET.HigherEducationInstitutions, new[] { EP.NotApplicable });
            retVal.Add(ET.WelshEstablishment, new[] { EP.NotApplicable });
            retVal.Add(ET.SixthFormCentres, new[] { EP.NotApplicable });
            retVal.Add(ET.SpecialPost16Institution, new[] { EP.NotApplicable });
            retVal.Add(ET.AcademySpecialSponsorLed, new[] { EP.NotApplicable });

            retVal.Add(ET.AcademyConverter, new[] { EP.Nursery, EP.Primary, EP.MiddleDeemedPrimary, EP.Secondary, EP.MiddleDeemedSecondary, EP._16Plus, EP.AllThrough });
            retVal.Add(ET.FreeSchools, new[] { EP.Primary, EP.MiddleDeemedPrimary, EP.Secondary, EP.MiddleDeemedSecondary, EP.AllThrough });

            retVal.Add(ET.FreeSchoolsSpecial, new[] { EP.NotApplicable });
            retVal.Add(ET.BritishSchoolsOverseas, new[] { EP.NotApplicable });
            retVal.Add(ET.FreeSchoolsAlternativeProvision, new[] { EP.NotApplicable });

            retVal.Add(ET.FreeSchools1619, new[] { EP._16Plus });
            retVal.Add(ET.UniversityTechnicalCollege, new[] { EP.Primary, EP.MiddleDeemedPrimary, EP.Secondary, EP.MiddleDeemedSecondary, EP._16Plus, EP.AllThrough });
            retVal.Add(ET.StudioSchools, new[] { EP.Primary, EP.MiddleDeemedPrimary, EP.Secondary, EP.MiddleDeemedSecondary, EP._16Plus, EP.AllThrough });
            
            retVal.Add(ET.AcademyAlternativeProvisionConverter, new[] { EP.NotApplicable });
            retVal.Add(ET.AcademyAlternativeProvisionSponsorLed, new[] { EP.NotApplicable });
            retVal.Add(ET.AcademySpecialConverter, new[] { EP.NotApplicable });

            retVal.Add(ET.Academy1619Converter, new[] { EP._16Plus });
            retVal.Add(ET.Academy1619SponsorLed, new[] { EP._16Plus });

            retVal.Add(ET.ChildrensCentre, new[] { EP.NotApplicable });
            retVal.Add(ET.ChildrensCentreLinkedSite, new[] { EP.NotApplicable });
            retVal.Add(ET.InstitutionFundedByOtherGovernmentDepartment, new[] { EP.NotApplicable });

            return retVal;
        }

        public async Task<IEnumerable<LookupDto>> GetPermissibleLocalGovernorsAsync(int urn, IPrincipal principal) => (await _httpClient.GetAsync<List<LookupDto>>($"establishment/{urn}/permissible-local-governors", principal)).GetResponse();

        
    }
}
