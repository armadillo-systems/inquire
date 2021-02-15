using iNQUIRE.Helper;
using iNQUIRE.Helpers;
using iNQUIRE.Models;
using iNQUIRE.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace iNQUIRE.Controllers.WebApi
{
    public class SearchParameters
    {
        public string IserId { get; set; }
        public string LangId { get; set; }
        public string Term { get; set; }
        public string CollectionIds { get; set; }
        public int Rows { get; set; }
        public int RowStart { get; set; }
        public string ParentId { get; set; }
        public string SortOrders { get; set; }
        public string FacetConstraints { get; set; }
        // SearchFacetViewModel facet)
    }

    [RoutePrefix("api/Search")]
    public class SearchController : WebApiControllerBase
    {
        private readonly IRepository _IRepository;
        private readonly IUserSearchRepository _IUserSearchRepository;
        private readonly IJP2Helper _IJP2Helper;

        public SearchController(IRepository irepository,
                                      IUserSearchRepository iuser_search_repository,
                                      IJP2Helper ijp2helper)
        {
            _IRepository = irepository;
            _IUserSearchRepository = iuser_search_repository;
            _IJP2Helper = ijp2helper;
        }

        #region search methods
        [HttpDelete, Route("{search_id}/{user_id}")]
        public IHttpActionResult DeleteSearch(string search_id, string user_id)
        {
            if ((String.IsNullOrEmpty(user_id)) || (String.IsNullOrEmpty(search_id)))
                return null;

            var r = _IUserSearchRepository.SearchDelete(user_id, new Guid(search_id));
            return Ok(r);
        }

        [HttpGet, Route("User/{user_id}")]
        public IHttpActionResult GetUserSearches(string user_id)
        {
            if (String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserSearchRepository.GetSearches(ApplicationIdInquire, user_id, DiscoverController.SavedSearchesDisplayMax);
            return Json(r);
        }

        [HttpGet, Route("Suggestions/{lang_id}/{term}")]
        public IHttpActionResult GetSearchSuggestions(string lang_id, string term)
        {
            if (String.IsNullOrEmpty(term))
                return null;

            var res = _IRepository.GetSearchSuggestions(lang_id, term);
            var sug = new List<string>();

            foreach (SolrNet.Impl.SpellCheckResult s in res.SpellChecking)
                sug.AddRange(s.Suggestions);

            return Json(sug);
        }

        [HttpGet, Route("Item/{item_id}/{lang_id}")]
        public IHttpActionResult GetItem(string item_id, string lang_id)
        {
            if (String.IsNullOrEmpty(item_id))
                return null;

            var res = _IRepository.GetRecord(item_id);
            SolrHelper.ForceHttps(ref res);
            SolrHelper.AddImageMetaData(ref res, _IJP2Helper);
            SolrHelper.SetLanguageData(ref res, lang_id);
            var results_vm = new SearchAjaxViewModel(res) { Rows = 1, RowStart = 0 };
            return Json(results_vm);
        }
        #endregion


        //[HttpPost, Route("Items/{rows:int}/{row_start:int}")]// /{user_id}/{lang_id}/{term}/{collection_ids}/{parent_id}/{sort_orders}/{facet_constraints}")]
        //public IHttpActionResult GetItems([FromBody] SearchParameters)
        //{
        //    var sq = new SearchQuery(user_id, term, collection_ids, rows, row_start, parent_id, sort_orders, facet_constraints, lang_id);

        //    if (string.IsNullOrEmpty(user_id))
        //        user_id = Guid.Empty.ToString();

        //    //XmlDataHelper.SearchXml(Request.GetBaseUri(Url), term, new List<string>(ids), collection_search)
        //    var results = SolrHelper.SearchSolr(sq, _IRepository, _IJP2Helper);

        //    // save all searches, even if user not logged in (for complete stats). if start row == 0 assume new search (and not a seach page nav click)
        //    var kvp = new KeyValuePair<Guid, String>(Guid.Empty, null);
        //    if (row_start == 0)
        //        kvp = _IUserSearchRepository.SearchSave(ApplicationIdInquire, lang_id, user_id, sq, results.NumFound);

        //    results.SearchID = kvp.Key; // if search was saved this will NOT be Guid.Empty, so then add it to users list as a newly saved search
        //    sq.SearchID = kvp.Key;

        //    results.SearchDisplayName = kvp.Value;
        //    sq.DisplayName = kvp.Value;

        //    sq.NumFound = results.NumFound;
        //    results.SearchQuery = sq;

        //    var results_vm = new SearchAjaxViewModel(results) { Rows = rows, RowStart = row_start };
        //    return Ok(results_vm);
        //}

    }
}