using iNQUIRE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace iNQUIRE.Controllers.WebApi
{
    [RoutePrefix("api/Notes")]
    public class NotesController : WebApiControllerBase
    {
        private readonly IUserNoteRepository<Note, string> _IUserNoteRepository;

        public NotesController(IUserNoteRepository<Note, string> iuser_note_repository) : base()
        {
            _IUserNoteRepository = iuser_note_repository;
        }

        [HttpPut, Route("{item_id}/{user_id}/{lang_id}/{text}/{public_note:bool}")]
        public IHttpActionResult AddNote(string item_id, string user_id, string lang_id, string text, bool public_note)
        {
            if (String.IsNullOrEmpty(item_id) || String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserNoteRepository.NoteAdd(ApplicationIdInquire, lang_id, user_id, item_id, text, public_note);
            return Ok(r);
        }

        [HttpPatch, Route("{note_id}/{user_id}/{text}/{public_note:bool}")]
        public IHttpActionResult UpdateNote(string note_id, string user_id, string text, bool public_note)
        {
            if (String.IsNullOrEmpty(note_id) || String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserNoteRepository.NoteUpdate(new Guid(note_id), user_id, text, public_note);
            return Ok(r);
        }

        [HttpDelete, Route("{note_id}/{user_id}")]
        public IHttpActionResult DeleteNote(string note_id, string user_id)
        {
            if (String.IsNullOrEmpty(note_id) || String.IsNullOrEmpty(user_id))
                return null;

            var r = _IUserNoteRepository.NoteDelete(new Guid(note_id), user_id);
            return Ok(r);
        }

        [AllowAnonymous]
        [HttpGet, Route("Item/{item_id}/{user_id?}")]
        public IHttpActionResult GetItemNotes(string item_id, string user_id = null)
        {
            List<NoteInfo> r;

            if (String.IsNullOrEmpty(user_id) || String.IsNullOrEmpty(item_id)) // user_id will be empty if not logged in, so quite common
                r = new List<NoteInfo>();
            else
                r = _IUserNoteRepository.GetNotesForItem(ApplicationIdInquire, user_id, item_id);

            return Ok(r);
        }

        [AllowAnonymous]
        [HttpGet, Route("Item/{item_id}/{approved:bool}")]
        public IHttpActionResult GetItemPublicNotes(string item_id, bool approved)
        {
            if (String.IsNullOrEmpty(item_id))
                return null;

            var r = _IUserNoteRepository.GetPublicNotesForItem(ApplicationIdInquire, item_id, approved);
            return Ok(r);
        }
    }
}