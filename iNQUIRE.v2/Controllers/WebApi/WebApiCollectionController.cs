using iNQUIRE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace iNQUIRE.Controllers.WebApi
{
    [RoutePrefix("api/Collections")]
    public class WebApiCollectionController : WebApiControllerBase
    {
        private readonly IUserCollectionRepository<Workspace, WorkspaceItem, string> _IUserCollectionRepository;

        public WebApiCollectionController(IUserCollectionRepository<Workspace, WorkspaceItem, string> iuser_collection) : base()
        {
            _IUserCollectionRepository = iuser_collection;
        }

        #region collection ajax methods
        [HttpPut, Route("Collection/{user_id}/{title}")]
        public IHttpActionResult AddCollection(string user_id, string title)
        {
            // if ((!Request.IsAuthenticated) || (String.IsNullOrEmpty(user_id)))
            if (String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserCollectionRepository.CollectionNew(ApplicationIdInquire, user_id, title);
            return Ok(r);
        }

        [HttpDelete, Route("Collection/{user_id}/{collection_id:int}")]
        public IHttpActionResult DeleteCollection(string collection_id)
        {
            if (String.IsNullOrEmpty(collection_id))
                return null;

            var r = _IUserCollectionRepository.CollectionDelete(new Guid(collection_id));
            return Ok(r);
        }

        [HttpPatch, Route("Collection/{user_id}/{collection_id:int}/{new_title}")]
        public IHttpActionResult RenameCollection(string collection_id, string new_title)
        {
            if (String.IsNullOrEmpty(collection_id))
                return null;

            var r = _IUserCollectionRepository.CollectionRename(new Guid(collection_id), new_title);
            return Ok(r);
        }

        //[HttpPut, Route("Collection/{user_id}/{collection_id:int}/{new_title}")]
        public IHttpActionResult CollectionAddItemAjax(string collection_id, string item_id, string title, string search_term, string language_id, int position)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionAddItem(new Guid(collection_id), item_id, title, search_term, language_id, position);
            return Ok(r);
        }

        public IHttpActionResult CollectionDeleteItemAjax(string collection_id, string item_id)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionDeleteItem(new Guid(collection_id), item_id);
            return Ok(r);
        }

        public IHttpActionResult CollectionUpdateItemAjax(string collection_id, string item_id, string lang_id, string notes, int pos_x, int pos_y, int position, string keywords, string search_term)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionUpdateItem(new Guid(collection_id), item_id, lang_id, notes, pos_x, pos_y, position, keywords, search_term);
            return Ok(r);
        }

        public IHttpActionResult CollectionUpdateItemPositionAjax(string collection_id, string item_id, int position)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionUpdateItemPosition(new Guid(collection_id), item_id, position);
            return Ok(r);
        }

        public IHttpActionResult CollectionUpdateItemPositionXYAjax(string collection_id, string item_id, int pos_x, int pos_y)
        {
            if (String.IsNullOrEmpty(collection_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserCollectionRepository.CollectionUpdateItemPositionXY(new Guid(collection_id), item_id, pos_x, pos_y);
            return Ok(r);
        }

        public IHttpActionResult CollectionGetAjax(string collection_id)
        {
            if (String.IsNullOrEmpty(collection_id))
                return null;

            var r = _IUserCollectionRepository.CollectionGet(new Guid(collection_id));
            return Ok(r);
        }

        public IHttpActionResult CollectionGetItems(string collection_id)
        {
            if (String.IsNullOrEmpty(collection_id))
                return null;

            var r = _IUserCollectionRepository.CollectionGetItems(new Guid(collection_id));
            return Ok(r);
        }

        [HttpGet, Route("Collections/User/{user_id}")]
        public IHttpActionResult CollectionsListUser(string user_id)
        {
            if (String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserCollectionRepository.CollectionsUser(ApplicationIdInquire, user_id);
            return Ok(r);
        }
        #endregion

    }
}