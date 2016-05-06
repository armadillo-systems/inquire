using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public interface IUserTagRepository<T, U, V>
    {
        Guid TagNew(Guid application_id, string lang_id, string user_id, string title); // returns guid of new tag, or existing tag
        // won't use this, once tags submitted that's it, if user doesn't like tag, will have to un-tag then re-tag with correct tag
        // no deleting of tags, once created that's it. avoids problem of the owner of the tag deleting the tag, thus either removing
        // or orphaning other users' tagged items
        // bool TagDelete(Guid tag_id);
        // bool TagRename(Guid tag_id, string new_title);

        TagItemResult TagItem(Guid application_id, string lang_id, string user_id, string tag_title, Guid tag_id, V item_id);
        bool UnTagItem(string user_id, Guid tag_id, V item_id);

        List<U> GetUserTaggedItems(Guid application_id, string user_id);
        
        /// <summary>
        ///  Gets a list of the Tags that the user has created
        /// </summary>
        List<T> GetUserCreatedTags(Guid application_id, string user_id);

        /// <summary>
        ///  Gets a list of the Tags that the user is using
        /// </summary>
        List<T> GetUserUsedTags(Guid application_id, string user_id);

        /// <summary>
        ///  Gets a list of the Tags for the item
        /// </summary>
        List<TagInfo> GetItemTags(Guid application_id, string lang_id, V item_id);

        /// <summary>
        ///  Gets a list of the Tags for the item for the given user
        /// </summary>
        List<TagInfo> GetUserItemTags(Guid application_id, string lang_id, V item_id, string user_id);

        /// <summary>
        ///  Gets a list of the Items for the Tag
        /// </summary>
        List<U> GetItemsFromTag(Guid tag_id);

        /// <summary>
        ///  Gets a list of the Items for the Tag for the given user
        /// </summary>
        List<U> GetUserItemsFromTag(Guid tag_id, string user_id);
    }
}