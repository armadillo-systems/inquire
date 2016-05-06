using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public class LinqToSqlUserCollectionRepository : UserGeneratedTextBase, IUserCollectionRepository<Workspace, WorkspaceItem, string>
    {
        // lot of debate about whether dispose should be called, or DataContext
        // created in every method and wrapped in a using, but consensus it that this
        // is not needed as DataContext is very efficient and won't leave open connections
        // iNQUIREDataContext _db = new iNQUIREDataContext();
        iNQUIRELiteDataContext _db = new iNQUIRELiteDataContext(); 

        public string CollectionNew(Guid application_id, string user_id, string title)
        {
            //var new_id = Guid.NewGuid();
            //_db.AddWorkspace(new_id, user_id, title); // did this one this way (directly using a stored procedure) just to be different
            //return new_id.ToString();
            try
            {
                var g = Guid.NewGuid();
                var w = new Workspace();
                w.WorkspaceID = g;
                w.UserID = user_id;
                w.Title = title;
                w.ApplicationID = application_id;
                
                var now = DateTime.Now;
                w.Created = now;
                w.LastEdited = now;
                _db.Workspaces.InsertOnSubmit(w);
                _db.SubmitChanges();
                return g.ToString();
            }
            catch (Exception ex)
            {
                return Guid.Empty.ToString();
            }
        }

        public bool CollectionDelete(Guid collection_id)
        {
            try
            {
                var workspace = _db.Workspaces.Single(w => w.WorkspaceID == collection_id);
                _db.Workspaces.DeleteOnSubmit(workspace);
                _db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CollectionRename(Guid collection_id, string new_title)
        {
            try
            {
                var workspace = _db.Workspaces.Single(w => w.WorkspaceID == collection_id);
                workspace.Title = new_title;
                _db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string CollectionAddItem(Guid collection_id, string item_id, string title, string search_term, string language_id, int position)
        {
            try
            {
                // check to see if item already exists in this workspace
                var o = from w in _db.WorkspaceItems
                        where w.WorkspaceID == collection_id && w.ObjectID == item_id
                        select w; 
                
                if (o.Count() > 0)
                    throw new Exception("Item already exists in Workspace");

                var item = new WorkspaceItem();
                item.ObjectID = (string)item_id;
                item.WorkspaceID = collection_id;
                item.LanguageID = language_id;
                item.ObjectWorkspaceID = Guid.NewGuid();
                item.Position = position;
                item.LastEdited = DateTime.Now;
                item.PosX = -1;
                item.PosY = -1;
                item.SearchTerm = search_term;
                item.Title = title;

                var workspace = _db.Workspaces.Single(w => w.WorkspaceID == collection_id);
                workspace.WorkspaceItems.Add(item);
                _db.SubmitChanges();
                return "ok";
            }
            catch (Exception e)
            {
                return String.Format("Error: {0}", e.Message);
            }
        }

        public bool CollectionDeleteItem(Guid collection_id, string item_id)
        {
            try
            {
                var item = _db.WorkspaceItems.Single(i => (i.WorkspaceID == collection_id) && (i.ObjectID == (string)item_id));
                _db.WorkspaceItems.DeleteOnSubmit(item);
                _db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CollectionUpdateItem(Guid collection_id, string item_id, string lang_id, string notes, int pos_x, int pos_y, int position, string keywords, string search_term)
        {
            try
            {
                var item = _db.WorkspaceItems.Single(i => i.ObjectID == (string)item_id);
                var workspace = _db.Workspaces.Single(w => w.WorkspaceID == collection_id);
                workspace.WorkspaceItems.Remove(item);
                _db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CollectionUpdateItemPosition(Guid collection_id, string item_id, int position)
        {
            try
            {
                var item = _db.WorkspaceItems.Single(i => (i.ObjectID == item_id) && (i.WorkspaceID == collection_id));
                item.Position = position;
                item.LastEdited = DateTime.Now;
                _db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CollectionUpdateItemPositionXY(Guid collection_id, string item_id, int pos_x, int pos_y)
        {
            try
            {
                var item = _db.WorkspaceItems.Single(i => (i.ObjectID == item_id) && (i.WorkspaceID == collection_id));
                item.PosX = pos_x;
                item.PosY = pos_y;
                item.LastEdited = DateTime.Now;
                _db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Workspace CollectionGet(Guid collection_id)
        {
            return _db.Workspaces.Single(w => w.WorkspaceID == collection_id);
        }

        public List<WorkspaceItem> CollectionGetItems(Guid collection_id)
        {
            var items = from w in _db.WorkspaceItems
                            where w.WorkspaceID == collection_id
                            orderby w.Position
                            select w;
            return items.ToList();
        }

        public List<Workspace> CollectionsUser(Guid application_id, string user_id)
        {
            var workspaces = from w in _db.Workspaces 
                             where w.ApplicationID == application_id && w.UserID == user_id
                             select w; // { WorkspaceID = w.WorkspaceID, Title = w.Title };
            return workspaces.ToList();
        }
    }
}