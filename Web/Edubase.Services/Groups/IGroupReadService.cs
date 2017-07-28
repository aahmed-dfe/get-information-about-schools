﻿using Edubase.Data.Entity;
using Edubase.Services.Domain;
using Edubase.Services.Groups.Models;
using Edubase.Services.Groups.Search;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Edubase.Services.Groups
{
    public interface IGroupReadService
    {
        Task<GroupModel> GetByEstablishmentUrnAsync(int urn, IPrincipal principal);

        Task<IEnumerable<GroupSuggestionItem>> SuggestAsync(string text, IPrincipal principal, int take = 10);

        Task<ApiSearchResult<SearchGroupDocument>> SearchAsync(GroupSearchPayload payload, IPrincipal principal);

        Task<ApiSearchResult<SearchGroupDocument>> SearchByIdsAsync(string groupId, int? groupUId, string companiesHouseNumber, IPrincipal principal);

        Task<IEnumerable<GroupModel>> GetAllByEstablishmentUrnAsync(int urn, IPrincipal principal);

        Task<ServiceResultDto<GroupModel>> GetAsync(int uid, IPrincipal principal);

        Task<bool> CanEditAsync(int uid, IPrincipal principal);

        /// <summary>
        /// Retrieves the list of Establishment Groups associated with a Group
        /// </summary>
        /// <param name="groupUid"></param>
        /// <returns></returns>
        Task<List<EstablishmentGroupModel>> GetEstablishmentGroupsAsync(int groupUid, IPrincipal principal, bool includeFutureDated = false);

        Task<bool> ExistsAsync(IPrincipal principal, CompaniesHouseNumber? companiesHouseNumber = null, string groupId = null, int? existingGroupUId = null, string name = null, int? localAuthorityId = null);

        Task<IEnumerable<GroupChangeDto>> GetChangeHistoryAsync(int uid, int take, IPrincipal user);
    }
}