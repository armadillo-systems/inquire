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
        public string UserId { get; set; }
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

    public class EmailExportParameters
    {
        public string UserId { get; set; }
        public string LangId { get; set; }
        public string ItemIds { get; set; }
        public string EmailTo { get; set; }
        public string Message { get; set; }
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

        [AllowAnonymous]
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


        [AllowAnonymous]
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


        [AllowAnonymous]
        [HttpPost, Route("Items")]// /{rows:int}/{row_start:int}/{user_id}/{lang_id}/{term}/{collection_ids}/{parent_id}/{sort_orders}/{facet_constraints}")]
        public IHttpActionResult GetItems([FromBody] SearchParameters search_params)
        {
            // we don't want a null value here as it causes clicking on past searches to break (pid+null rather than pid+)
            // this value comes through as null since changing the way the params come through to a json object
            var parent_id = search_params.ParentId == null ? "" : search_params.ParentId;
            var term = search_params.Term == null ? "" : search_params.Term;

            var sq = new SearchQuery(search_params.UserId, term, search_params.CollectionIds, search_params.Rows, search_params.RowStart, parent_id, search_params.SortOrders, search_params.FacetConstraints, search_params.LangId);

            var user_id = string.IsNullOrEmpty(search_params.UserId) ? Guid.Empty.ToString() : search_params.UserId;

            //XmlDataHelper.SearchXml(Request.GetBaseUri(Url), term, new List<string>(ids), collection_search)
            var results = SolrHelper.SearchSolr(sq, _IRepository, _IJP2Helper);

            // save all searches, even if user not logged in (for complete stats). if start row == 0 assume new search (and not a seach page nav click)
            var kvp = new KeyValuePair<Guid, String>(Guid.Empty, null);
            if (search_params.RowStart == 0)
                kvp = _IUserSearchRepository.SearchSave(ApplicationIdInquire, search_params.LangId, user_id, sq, results.NumFound);

            results.SearchID = kvp.Key; // if search was saved this will NOT be Guid.Empty, so then add it to users list as a newly saved search
            sq.SearchID = kvp.Key;

            results.SearchDisplayName = kvp.Value;
            sq.DisplayName = kvp.Value;

            sq.NumFound = results.NumFound;
            results.SearchQuery = sq;

            var results_vm = new SearchAjaxViewModel(results) { Rows = search_params.Rows, RowStart = search_params.RowStart };
            return Ok(results_vm);
        }


        [AllowAnonymous]
        [HttpPost, Route("Email")]
        public IHttpActionResult EmailExport([FromBody] EmailExportParameters email_params)
        {
            //email_to, string message, string id_str, string lang_id
            // setResolvers();
            if (email_params == null)
                return BadRequest();

            if (string.IsNullOrEmpty(email_params.EmailTo))
                return BadRequest("No destination email address");

            if (string.IsNullOrEmpty(email_params.ItemIds))
                return BadRequest("No items selected");

            var results = SolrHelper.GetItemsFromIDStringList(email_params.ItemIds, false, _IRepository, _IJP2Helper);

            var export_items = new List<ExportItem>();
            var count = 0;
            foreach (IInqItem r in results)
            {
                var img_src = ImageHelper.GetImageUri(r, ImageHelper.ExportImageWidth, ImageHelper.ExportImageHeight, Request.RequestUri.GetLeftPart(UriPartial.Authority), _IJP2Helper.MediaDirectory, _IJP2Helper);
                // var img_src = _IJP2Helper.GetImageUri(r.ImageMetadata, MediaDirectoryFullUri, _IJP2Helper.ResolverUri, ExportImageWidth, ExportImageHeight);
                export_items.Add(new ExportItem(r, img_src, email_params.LangId));
                count++;

                if (count >= ImageHelper.ExportMaxImages)
                    break;
            }

            var export = new EmailExport(email_params.EmailTo, email_params.Message, export_items, Url.Content("~/Content/images/export-image-not-found.png"));
            var queue = HttpContextCurrent.Application["email_queue"] as Queue<EmailExport>;
            queue.Enqueue(export);

            //var res = _IRepository.GetRecord(_IRepository.GetBaseUri(Request, Url), id);
            //var results_vm = new SearchAjaxViewModel(res) { Rows = 1, RowStart = 0 };

            return Ok("ok");
        }
    }
}