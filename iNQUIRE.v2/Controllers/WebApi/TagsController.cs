using iNQUIRE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace iNQUIRE.Controllers.WebApi
{
    [RoutePrefix("api/Tags")]
    public class TagsController : WebApiControllerBase
    {
        private readonly IUserTagRepository<Tag, TaggedItem, string> _IUserTagRepository;

        public TagsController(IUserTagRepository<Tag, TaggedItem, string> iuser_tag_repository) : base()
        {
            _IUserTagRepository = iuser_tag_repository;
        }

        [HttpPut, Route("{item_id}/{user_id}/{lang_id}/{title}")]
        public IHttpActionResult AddTag(string item_id, string user_id, string lang_id, string title)
        {
            if (String.IsNullOrEmpty(lang_id) || String.IsNullOrEmpty(item_id) || String.IsNullOrEmpty(user_id))
                return null;

            Guid tag_guid;

            //if (String.IsNullOrEmpty(tag_id))
                tag_guid = Guid.Empty;
            //else
            //    tag_guid = new Guid(tag_id);

            var r = _IUserTagRepository.TagItem(ApplicationIdInquire, lang_id, user_id, title, tag_guid, item_id);
            return Ok(r);
        }

        [HttpGet, Route("Item/{item_id}/{lang_id}")]
        public IHttpActionResult GetItemTags(string item_id, string lang_id)
        {
            if (String.IsNullOrEmpty(lang_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserTagRepository.GetItemTags(ApplicationIdInquire, lang_id, item_id);
            return Ok(r);
        }

        [HttpGet, Route("Item/User/{item_id}/{lang_id}/{user_id?}")]
        public IHttpActionResult GetItemTagsForUser(string item_id, string lang_id, string user_id = null)
        {
            List<TagInfo> r;

            if (String.IsNullOrEmpty(lang_id) || String.IsNullOrEmpty(item_id) || String.IsNullOrEmpty(user_id))
                r = new List<TagInfo>();
            else
                r = _IUserTagRepository.GetUserItemTags(ApplicationIdInquire, lang_id, item_id, user_id);

            return Ok(r);
        }

        [HttpDelete, Route("Item/User/{item_id}/{user_id}/{tag_id}")]
        public IHttpActionResult DeleteTag(string item_id, string user_id, string tag_id)
        {
            if (String.IsNullOrEmpty(user_id) || String.IsNullOrEmpty(tag_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserTagRepository.UnTagItem(user_id, new Guid(tag_id), item_id);
            return Ok(r);
        }
    }
}