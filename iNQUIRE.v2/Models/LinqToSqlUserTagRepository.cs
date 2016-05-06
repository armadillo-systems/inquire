using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public class LinqToSqlUserTagRepository : UserGeneratedTextBase, IUserTagRepository<Tag, TaggedItem, string>
    {
        // iNQUIREDataContext _db = new iNQUIREDataContext();
        iNQUIRELiteDataContext dc = new iNQUIRELiteDataContext();

        public Guid TagNew(Guid application_id, string lang_id, string user_id, string title) 
        {
            try
            {
                // is new tag a naughty word
                if (IsRudeWord(title))
                    throw new Exception(BANNED_WORD_DETECTED);

                // check to see if tag already exists
                var o = dc.Tags.SingleOrDefault(t => t.ApplicationID == application_id && t.LanguageID == lang_id && t.Title == title);

                if ((o != null) && (o.TagID != Guid.Empty))
                    return o.TagID; // tag already exists, return the guid

                // new tag, add to Tag table and return the new id
                var new_id = Guid.NewGuid();

                var tag = new Tag();
                tag.ApplicationID = application_id;
                tag.TagID = new_id;
                tag.Title = title;
                tag.UserID = user_id;
                tag.LanguageID = lang_id;
                tag.Created = DateTime.Now;
                dc.Tags.InsertOnSubmit(tag);

                dc.SubmitChanges();
                return new_id;
            }
            catch (Exception e)
            {
                return Guid.Empty;
            }
        }

        public TagItemResult TagItem(Guid application_id, string lang_id, string user_id, string tag_title, Guid tag_id, string item_id)
        {
            if (tag_id == Guid.Empty)
                tag_id = TagNew(application_id, lang_id, user_id, tag_title);

            if (tag_id == Guid.Empty)
                return new TagItemResult(false, tag_id, tag_title, "Error: Tag not found and could not add tag");
            else
            {
                var tag = dc.TaggedItems.SingleOrDefault(ti => (ti.ApplicationID == application_id) && (ti.TagID == tag_id) && (ti.ObjectID == item_id) && (ti.UserID == user_id));
                if ((tag != null) && (tag.TagID != Guid.Empty))
                    return new TagItemResult(false, tag_id, tag_title, "Warning: Item already tagged with this tag by this user");

                var tagged_item = new TaggedItem();
                tagged_item.ApplicationID = application_id;
                tagged_item.ObjectToTagID = Guid.NewGuid();
                tagged_item.LanguageID = lang_id;
                tagged_item.ObjectID = item_id;
                tagged_item.UserID = user_id;
                tagged_item.TagID = tag_id;
                tagged_item.LastEdited = DateTime.Now;
                dc.TaggedItems.InsertOnSubmit(tagged_item);
                dc.SubmitChanges();
                return new TagItemResult(true, tag_id, tag_title, "Ok");
            }
        }

        public bool UnTagItem(string user_id, Guid tag_id, string item_id)
        {
            try
            {
                var tagged_item = dc.TaggedItems.Single(ti => (ti.UserID == user_id) && (ti.TagID == tag_id) && (ti.ObjectID == item_id));
                dc.TaggedItems.DeleteOnSubmit(tagged_item);
                dc.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<TaggedItem> GetUserTaggedItems(Guid application_id, string user_id)
        {
            var items = from ti in dc.TaggedItems
                        where ti.ApplicationID == application_id && ti.UserID == user_id
                        select ti;
            return items.ToList();
        }

        public List<Tag> GetUserCreatedTags(Guid application_id, string user_id)
        {
            var tags = from t in dc.Tags
                       where t.ApplicationID == application_id && t.UserID == user_id
                       select t;
            return tags.ToList();
        }

        public List<Tag> GetUserUsedTags(Guid application_id, string user_id)
        {
            var tags = from t in dc.Tags
                       join ti in dc.TaggedItems on t.TagID equals ti.TagID
                       where ti.ApplicationID == application_id && ti.UserID == user_id
                       select t;
            return tags.ToList();
        }

        public List<TagInfo> GetItemTags(Guid application_id, string lang_id, string item_id)
        {
            // use the TagInfo class here, as if use the Tag class it will have a property of TaggedItems
            // containing the full ObjectLocale table row for every tagged item (= potentially a lot of data!)
            var tags = from t in dc.Tags
                       join ti in dc.TaggedItems on t.TagID equals ti.TagID
                       where ti.ApplicationID == application_id && ti.ObjectID == item_id
                       select new TagInfo { Title = t.Title, TagID = t.TagID };

            return tags.ToList();
        }

        public List<TagInfo> GetUserItemTags(Guid application_id, string lang_id, string item_id, string user_id)
        {
            var tags = from t in dc.Tags
                       join ti in dc.TaggedItems on t.TagID equals ti.TagID
                       where ti.ApplicationID == application_id && ti.ObjectID == item_id && ti.UserID == user_id
                       select new TagInfo { Title = t.Title, TagID = t.TagID };

            return tags.ToList();
        }

        public List<TaggedItem> GetItemsFromTag(Guid tag_id)
        {
            var items = from ti in dc.TaggedItems
                        where ti.TagID == tag_id
                        select ti;

            return items.ToList();
        }

        public List<TaggedItem> GetUserItemsFromTag(Guid tag_id, string user_id)
        {
            var items = from ti in dc.TaggedItems
                        where ti.TagID == tag_id && ti.UserID == user_id
                        select ti;

            return items.ToList();
        }
    }
}