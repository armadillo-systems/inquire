using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace iNQUIRE.Models
{
    public interface IUserCollectionRepository<T, U, V> // T: Workspace type, U: WorkspaceItem type, V: item id type
    {
        string CollectionNew(Guid application_id, string user_id, string title); // returns guid of new collection
        bool CollectionDelete(Guid collection_id);
        bool CollectionRename(Guid collection_id, string new_title);
        string CollectionAddItem(Guid collection_id, V item_id, string title, string search_term, string language_id, int position);
        bool CollectionDeleteItem(Guid collection_id, V item_id);
        
        bool CollectionUpdateItem(Guid collection_id, V item_id, string lang_id, string notes, int pos_x, int pos_y, int position, string keywords, string search_term);
        bool CollectionUpdateItemPosition(Guid collection_id, V item_id, int position);
        bool CollectionUpdateItemPositionXY(Guid collection_id, V item_id, int pos_x, int pos_y);

        T CollectionGet(Guid collection_id);
        List<T> CollectionsUser(Guid application_id, string user_id);

        List<U> CollectionGetItems(Guid collection_id);
    }
}