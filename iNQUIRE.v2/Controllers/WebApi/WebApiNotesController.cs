using iNQUIRE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace iNQUIRE.Controllers.WebApi
{
    public class WebApiNotesController : WebApiControllerBase
    {
        private readonly IUserTagRepository<Tag, TaggedItem, string> _IUserTagRepository;
        private readonly IUserNoteRepository<Note, string> _IUserNoteRepository;

        public WebApiNotesController(IUserTagRepository<Tag, TaggedItem, string> iuser_tag_repository,
                                    IUserNoteRepository<Note, string> iuser_note_repository) : base()
        {
            _IUserTagRepository = iuser_tag_repository;
            _IUserNoteRepository = iuser_note_repository;
        }
        #region notes
        public IHttpActionResult NoteAddAjax(string lang_id, string user_id, string item_id, string text, bool public_note)
        {
            if (String.IsNullOrEmpty(item_id) || String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserNoteRepository.NoteAdd(ApplicationIdInquire, lang_id, user_id, item_id, text, public_note);
            return Ok(r);
        }

        public IHttpActionResult NoteUpdateAjax(string note_id, string user_id, string text, bool public_note)
        {
            if (String.IsNullOrEmpty(note_id) || String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserNoteRepository.NoteUpdate(new Guid(note_id), user_id, text, public_note);
            return Ok(r);
        }

        public IHttpActionResult NoteDeleteAjax(string note_id, string user_id)
        {
            if (String.IsNullOrEmpty(note_id) || String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserNoteRepository.NoteDelete(new Guid(note_id), user_id);
            return Ok(r);
        }

        public IHttpActionResult GetNotesForItemAjax(string user_id, string item_id)
        {
            List<NoteInfo> r;

            if (String.IsNullOrEmpty(user_id) || String.IsNullOrEmpty(item_id)) // user_id will be empty if not logged in, so quite common
                r = new List<NoteInfo>();
            else
                r = _IUserNoteRepository.GetNotesForItem(ApplicationIdInquire, user_id, item_id);

            return Ok(r);
        }

        public IHttpActionResult GetPublicNotesForItemAjax(string item_id, bool approved)
        {
            if (String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserNoteRepository.GetPublicNotesForItem(ApplicationIdInquire, item_id, approved);
            return Ok(r);
        }
#endregion

        #region tags ajax methods
        public IHttpActionResult TagItemAjax(string lang_id, string user_id, string title, string tag_id, string item_id)
        {
            if (String.IsNullOrEmpty(lang_id) || String.IsNullOrEmpty(item_id) || String.IsNullOrEmpty(user_id))
                return null;

            Guid tag_guid;

            if (String.IsNullOrEmpty(tag_id))
                tag_guid = Guid.Empty;
            else
                tag_guid = new Guid(tag_id);

            var r = _IUserTagRepository.TagItem(ApplicationIdInquire, lang_id, user_id, title, tag_guid, item_id);
            return Ok(r);
        }

        public IHttpActionResult GetItemTagsAjax(string lang_id, string item_id)
        {
            if (String.IsNullOrEmpty(lang_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserTagRepository.GetItemTags(ApplicationIdInquire, lang_id, item_id);
            return Ok(r);
        }

        public IHttpActionResult GetUserItemTagsAjax(string lang_id, string item_id, string user_id)
        {
            List<TagInfo> r;

            if (String.IsNullOrEmpty(lang_id) || String.IsNullOrEmpty(item_id) || String.IsNullOrEmpty(user_id))
                r = new List<TagInfo>();
            else
                r = _IUserTagRepository.GetUserItemTags(ApplicationIdInquire, lang_id, item_id, user_id);

            return Ok(r);
        }

        public IHttpActionResult UnTagItemAjax(string user_id, string tag_id, string item_id)
        {
            if (String.IsNullOrEmpty(user_id) || String.IsNullOrEmpty(tag_id) || String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserTagRepository.UnTagItem(user_id, new Guid(tag_id), item_id);
            return Ok(r);
        }
        #endregion

    }
}