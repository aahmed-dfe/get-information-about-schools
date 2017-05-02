﻿using Edubase.Common;
using Edubase.Services.Establishments.Search;
using Edubase.Web.UI.Areas.Governors.Models;
using Edubase.Web.UI.Helpers.ModelBinding;
using System;
using System.Collections.Generic;
using static Edubase.Services.Establishments.Search.EstablishmentSearchPayload;
using MoreLinq;
using System.Linq;
using Edubase.Common.Spatial;
using Edubase.Web.UI.Models;
using Edubase.Web.UI.Models.Search;
using Edubase.Services.Core.Search;

namespace Edubase.Web.UI.Areas.Establishments.Models.Search
{
    public class EstablishmentSearchViewModel
    {
        private Dictionary<int, eTextSearchType> _textSearchTypeMap = new Dictionary<int, eTextSearchType>
        {
            { 5, eTextSearchType.URN },
            { 6, eTextSearchType.URN },
            { 7, eTextSearchType.LAESTAB },
            { 8, eTextSearchType.UKPRN }
        };

        private readonly int[] _radiuses = new int[] { 1, 3, 5, 10, 15, 20, 25 };

        private Dictionary<char, eSortBy> _sortByMap = new Dictionary<char, eSortBy>
        {
            { 'd', eSortBy.Distance },
            { 'a', eSortBy.NameAlphabeticalAZ },
            { 'z', eSortBy.NameAlphabeticalZA }
        };

        #region Bind alias constants
        public const string BIND_ALIAS_TYPEIDS = "a";
        public const string BIND_ALIAS_STATUSIDS = "b";
        public const string BIND_ALIAS_PHASEIDS = "c";
        public const string BIND_ALIAS_LAIDS = "d";
        public const string BIND_ALIAS_RELICHARIDS = "e";
        public const string BIND_ALIAS_GOONERES = "f";
        public const string BIND_ALIAS_ADMPOL = "g";
        public const string BIND_ALIAS_BOARDPOL = "h";
        public const string BIND_ALIAS_DIOCESE = "i";
        public const string BIND_ALIAS_DISTRICT = "j";
        public const string BIND_ALIAS_FETYPE = "k";
        public const string BIND_ALIAS_GENDER = "l";
        public const string BIND_ALIAS_GOR = "m";
        public const string BIND_ALIAS_NURSERY = "n";
        public const string BIND_ALIAS_RATING = "o"; 
        public const string BIND_ALIAS_PARLCONST = "p";
        public const string BIND_ALIAS_RELETHOS = "q";
        public const string BIND_ALIAS_RSCREG = "r";
        public const string BIND_ALIAS_SECT41 = "s";
        public const string BIND_ALIAS_SIXTHFORM = "t";
        public const string BIND_ALIAS_SPECLASS = "u";
        public const string BIND_ALIAS_TYPEOFSENPRV = "v";
        public const string BIND_ALIAS_URBRUR = "w";
        public const string BIND_ALIAS_WARD = "x";
        public const string BIND_ALIAS_SELECTED_SEARCH_FILTERS = "y";
        public const string BIND_ALIAS_SORT_BY = "z";
        public const string BIND_ALIAS_RADIUS = "aa";
        #endregion

        public enum eTextSearchType
        {
            Unknown,
            URN,
            LAESTAB,
            EstablishmentName,
            UKPRN
        }

        public IList<SearchEstablishmentDocument> Results { get; set; }
        public string Error { get; set; }

        public eTextSearchType TextSearchType
        {
            get
            {
                var text = TextSearchModel.Text.RemoveSubstring("/").Clean();
                return text.IsInteger() ? _textSearchTypeMap.Get(text.Length, eTextSearchType.Unknown) : eTextSearchType.EstablishmentName;
            }
        }

        public long Count { get; set; }
        public int PageCount => (int)Math.Ceiling(Count / (double)PageSize);
        public int PageSize { get; set; } = 50;
        public int StartIndex { get; set; }
        public bool HasError => !Error.IsNullOrEmpty();

        public int[] GetRadiusOptions() => _radiuses;

        [BindAlias(BIND_ALIAS_RADIUS)]
        public int? RadiusInMiles { get; set; }

        [BindAlias(BIND_ALIAS_SORT_BY)]
        public char? SortBy { get; set; }

        public eSortBy GetSortOption()
        {
            var retVal = SortBy.HasValue ? _sortByMap.Get(SortBy.Value, GetDefaultSortOption()) : GetDefaultSortOption();
            if (SearchType != eSearchType.Location && retVal == eSortBy.Distance) retVal = eSortBy.NameAlphabeticalAZ;
            return retVal;
        }

        private eSortBy GetDefaultSortOption() => SearchType == eSearchType.Location ? eSortBy.Distance : eSortBy.NameAlphabeticalAZ;
        

        public int GetRadiusOption()
        {
            if (!RadiusInMiles.HasValue || !_radiuses.Contains(RadiusInMiles.Value)) return _radiuses[1];
            else return RadiusInMiles.Value;
        }

        private Lazy<LatLon> _coordinate;

        public LatLon LocationSearchCoordinate => _coordinate.Value;

        public EstablishmentSearchViewModel()
        {
            _coordinate = new Lazy<LatLon>(() => LatLon.Parse(LocationSearchModel.AutoSuggestValue));
        }
        
        public SearchPayloadViewModel TextSearchModel { get; set; } = new SearchPayloadViewModel();
        public SearchPayloadViewModel LocationSearchModel { get; set; } = new SearchPayloadViewModel();
        
        public eSearchType SearchType { get; set; }

        #region Filters / bind aliases

        [BindAlias(BIND_ALIAS_SELECTED_SEARCH_FILTERS)]
        public string SelectedExtraSearchFilters { get; set; }

        public IEnumerable<LookupItemViewModel> EstablishmentTypes { get; set; }

        [BindAlias(BIND_ALIAS_TYPEIDS)]
        public List<int> SelectedEstablishmentTypeIds { get; set; } = new List<int>();

        public IEnumerable<LookupItemViewModel> EstablishmentStatuses { get; set; }

        [BindAlias(BIND_ALIAS_STATUSIDS)]
        public List<int> SelectedEstablishmentStatusIds { get; set; } = new List<int>();

        public IEnumerable<LookupItemViewModel> EducationPhases { get; set; }

        [BindAlias(BIND_ALIAS_PHASEIDS)]
        public List<int> SelectedEducationPhaseIds { get; set; } = new List<int>();

        public IEnumerable<LookupItemViewModel> LocalAuthorities { get; set; }

        [BindAlias(BIND_ALIAS_LAIDS)]
        public List<int> SelectedLocalAuthorityIds { get; set; } = new List<int>();

        public IEnumerable<LookupItemViewModel> ReligiousCharacters { get; set; }

        [BindAlias(BIND_ALIAS_RELICHARIDS)]
        public List<int> SelectedReligiousCharacterIds { get; set; } = new List<int>();
        
        public IEnumerable<LookupItemViewModel> GovernorRoles { get; set; }


        [BindAlias(BIND_ALIAS_ADMPOL)]
        public List<int> SelectedAdmissionsPolicyIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> AdmissionsPolicies { get; set; }


        [BindAlias(BIND_ALIAS_BOARDPOL)]
        public List<int> SelectedBoardingProvisionIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> BoardingProvisions { get; set; }


        [BindAlias(BIND_ALIAS_DIOCESE)]
        public List<int> SelectedDioceseIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> Dioceses { get; set; }


        [BindAlias(BIND_ALIAS_DISTRICT)]
        public List<int> SelectedDistrictIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> Districts { get; set; }


        [BindAlias(BIND_ALIAS_FETYPE)]
        public List<int> SelectedFETypeIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> FurtherEducationTypes { get; set; }


        [BindAlias(BIND_ALIAS_GENDER)]
        public List<int> SelectedGenderIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> Genders { get; set; }


        [BindAlias(BIND_ALIAS_GOR)]
        public List<int> SelectedGORIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> GORs { get; set; }


        [BindAlias(BIND_ALIAS_NURSERY)]
        public List<int> SelectedNurseryProvisionIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> NurseryProvisions { get; set; }


        [BindAlias(BIND_ALIAS_PARLCONST)]
        public List<int> SelectedParliamentaryConstituencyIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> ParliamentaryConstituencies { get; set; }


        [BindAlias(BIND_ALIAS_RELETHOS)]
        public List<int> SelectedReligiousEthosIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> ReligiousEthoses { get; set; }


        [BindAlias(BIND_ALIAS_RSCREG)]
        public List<int> SelectedRSCRegionIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> RSCRegions { get; set; }


        [BindAlias(BIND_ALIAS_SECT41)]
        public List<int> SelectedSection41Ids { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> Section41Designations { get; set; }


        [BindAlias(BIND_ALIAS_SIXTHFORM)]
        public List<int> SelectedSixthFormProvisionIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> SixthFormProvisions { get; set; }


        [BindAlias(BIND_ALIAS_SPECLASS)]
        public List<int> SelectedSpecialClassesProvisionIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> SpecialClassesProvisions { get; set; }


        [BindAlias(BIND_ALIAS_TYPEOFSENPRV)]
        public List<int> SelectedTypeOfSENProvisionIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> TypeOfSENProvisions { get; set; }


        [BindAlias(BIND_ALIAS_URBRUR)]
        public List<int> SelectedUrbanRuralIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> UrbanRuralDesignations { get; set; }


        [BindAlias(BIND_ALIAS_WARD)]
        public List<int> SelectedWardIds { get; set; } = new List<int>();
        public IEnumerable<LookupItemViewModel> Wards { get; set; }

        
        /// <summary>
        /// When one result is found, whether to redirect the user to the detail page
        /// </summary>
        [BindAlias(BIND_ALIAS_GOONERES)]
        public bool GoToDetailPageOnOneResult { get; set; }

        #endregion
    }
}