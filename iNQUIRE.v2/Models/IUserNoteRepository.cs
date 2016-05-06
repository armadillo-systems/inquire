using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public interface IUserNoteRepository<T, V>
    {
        T NoteAdd(Guid application_id, string lang_id, string user_id, V item_id, string text, bool public_note);
        T NoteUpdate(Guid note_id, string user_id, string text, bool public_note);
        bool NoteApprove(Guid note_id, string user_id, bool approved);
        bool NoteDelete(Guid note_id, string user_id);

        List<NoteInfo> GetNotesForItem(Guid application_id, string user_id, V item_id);
        List<NoteInfo> GetPublicNotesForItem(Guid application_id, V item_id, bool approved);
    }
}