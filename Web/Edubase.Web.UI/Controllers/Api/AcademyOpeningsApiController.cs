﻿using Edubase.Services.Approvals;
using Edubase.Services.Approvals.Models;
using Edubase.Services.Domain;
using Edubase.Services.Enums;
using Edubase.Services.Establishments;
using Edubase.Services.Establishments.Models;
using Edubase.Services.Establishments.Search;
using Edubase.Services.Lookup;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Edubase.Web.UI.Controllers.Api
{
    public class AcademyOpeningsApiController : ApiController
    {
        private readonly IEstablishmentReadService _establishmentReadService;
        private readonly IEstablishmentWriteService _establishmentWriteService;
        private readonly ICachedLookupService _lookupService;

        public AcademyOpeningsApiController(IEstablishmentReadService establishmentReadService,
            IEstablishmentWriteService establishmentWriteService,
            ICachedLookupService lookupService)
        {
            _establishmentReadService = establishmentReadService;
            _lookupService = lookupService;
            _establishmentWriteService = establishmentWriteService;
        }

        /// <summary>
        /// Treating myself in this one to a really nice URL. I deserve it. And so does Jon.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        [Route("api/academy-openings/list/{from:datetime}/{to:datetime}/{skip:int}/{take:int}"), HttpGet]
        public async Task<dynamic> GetListAsync(DateTime from, DateTime to, int skip, int take)
        {
            var estabTypes = await _lookupService.EstablishmentTypesGetAllAsync();

            var apiResult = (await _establishmentReadService.SearchAsync(new EstablishmentSearchPayload
            {
                Skip = skip,
                Take = take,
                SortBy = eSortBy.NameAlphabeticalAZ,
                Filters = new EstablishmentSearchFilters
                {
                    OpenDateMin = from,
                    OpenDateMax = to
                }
            }, User));
            
            return new
            {
                Items = apiResult.Items.Select(x => new
                {
                    x.Urn,
                    x.Name,
                    EstablishmentType = x.TypeId.HasValue ? estabTypes.FirstOrDefault(t => t.Id == x.TypeId)?.Name : null,
                    OpeningDate = x.OpenDate,
                    DisplayDate = x.OpenDate?.ToString("dd/MM/yyyy"),
                    PredecessorName = "Bob",
                    PredecessorUrn = "123445"
                }),
                Count = apiResult.Count
            };
        }

        /// <summary>
        /// PATCH api/academy/{urn}
        /// Takes a payload with openDate and Name properties.
        /// </summary>
        /// <param name="urn"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        [Route("api/academy/{urn:int}"), HttpPatch]
        public async Task<ApiResponse> SaveAsync(int urn, [FromBody] dynamic payload)
        {
            return await _establishmentWriteService.PartialUpdateAsync(new EstablishmentModel
            {
                OpenDate = payload.openDate,
                Name = payload.name,
                Urn = urn
            }, new EstablishmentFieldList
            {
                OpenDate = true,
                Name = true
            }, User);
        }

    }
}