using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace iNQUIRE.Models
{
    public class LinqToSqlUserNoteRepository : UserGeneratedTextBase, IUserNoteRepository<Note, string>
    {
        // iNQUIREDataContext _db = new iNQUIREDataContext();
        iNQUIRELiteDataContext _db = new iNQUIRELiteDataContext();

        public Note NoteAdd(Guid application_id, string lang_id, string user_id, string item_id, string text, bool public_note)
        {
            try
            {
                if (!IsTextModerationPassed(text)) // && public_note
                    throw new Exception(BANNED_WORD_DETECTED);

                var note = new Note();
                note.ApplicationID = application_id;
                note.NoteID = Guid.NewGuid();
                note.LanguageID = lang_id;
                note.ParentID = item_id;
                note.UserID = user_id;
                note.Text = text;
                note.PublicNote = public_note;
                var now = DateTime.Now;
                note.Created = now;
                note.LastEdited = now;
                
                if(public_note)
                    note.Approved = true; // needs to be false in deployed version
                else
                    note.Approved = true; // private notes don't need approval

                _db.Notes.InsertOnSubmit(note);
                _db.SubmitChanges();
                return note;
            }
            catch (Exception ex)
            {
                var err = new Note();
                err.NoteID = Guid.Empty;
                err.Text = string.Format("{0}. {1}", ex.Message, ex.InnerException?.Message);
                return err;
            }
        }

        public Note NoteUpdate(Guid note_id, string user_id, string text, bool public_note)
        {
            try
            {
                if (!IsTextModerationPassed(text))  // && public_note
                    throw new Exception(BANNED_WORD_DETECTED);

                // user_id not technically needed, but kept in for slightly more security
                var note = _db.Notes.Single(n => ((n.NoteID == note_id) && (n.UserID == user_id)));
                note.Text = text;
                note.PublicNote = public_note;
                note.LastEdited = DateTime.Now;
                _db.SubmitChanges();
                return note;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool NoteApprove(Guid note_id, string user_id, bool approved)
        {
            try
            {
                // user_id not technically needed, but kept in for slightly more security
                var note = _db.Notes.Single(n => ((n.NoteID == note_id) && (n.UserID == user_id)));
                note.Approved = approved;
                _db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool NoteDelete(Guid note_id, string user_id)
        {
            try
            {
                var note = _db.Notes.Single(n => ((n.NoteID == note_id) && (n.UserID == user_id)));
                _db.Notes.DeleteOnSubmit(note);
                _db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<NoteInfo> GetNotesForItem(Guid application_id, string user_id, string item_id)
        {
            var notes = from n in _db.Notes
                        where n.ApplicationID == application_id && n.UserID == user_id && n.ParentID == item_id && n.PublicNote == false
                        orderby n.LastEdited descending
                        select makeNoteInfo(n);
            return notes.ToList();
        }

        public List<NoteInfo> GetPublicNotesForItem(Guid application_id, string item_id, bool approved)
        {
            var notes = from n in _db.Notes
                        where n.ApplicationID == application_id && n.ParentID == item_id && n.PublicNote == true /*&& n.Approved == approved*/
                        orderby n.LastEdited descending
                        select makeNoteInfo(n);
            return notes.ToList();
        }

        private NoteInfo makeNoteInfo(Note n)
        {
            return new NoteInfo
                            {
                                NoteID = n.NoteID,
                                UserID = n.UserID,
                                LastEdited = n.LastEdited,
                                ObjectID = n.ParentID,
                                //ObjectTitle = n.ObjectLocale.Title,
                                Text = n.Text,
                                UserName = n.User == null ? "" : n.User.UserName,
                                PublicNote = n.PublicNote
                            };
        }
    }
}