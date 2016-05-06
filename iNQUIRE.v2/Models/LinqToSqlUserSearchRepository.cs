using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace iNQUIRE.Models
{
    public class LinqToSqlUserSearchRepository : UserGeneratedTextBase, IUserSearchRepository
    {
        iNQUIRELiteDataContext _db = new iNQUIRELiteDataContext();

        public List<SearchQuery> GetSearches(Guid application_id, string user_id, int num)
        {
            var searches = (from s in _db.Searches
                        where s.ApplicationID == application_id && s.UserId == user_id && s.Deleted == false && s.Empty == false && s.NumFound > 0 && s.Hidden == false
                        orderby s.SearchDate descending
                        select makeSearchQuery(s)).Take(num);
            return searches.ToList();
        }

        private SearchQuery makeSearchQuery(Search s)
        {
            var sq = new SearchQuery()
            {
                SearchID = s.SearchID,
                UserID = s.UserId,
                DisplayName = s.DisplayName,
                Term = s.Term,
                Rows = s.Rows,
                RowStart = s.RowStart,
                ParentID = s.ParentID,
                Date = s.SearchDate,
                NumFound = s.NumFound,
                Empty = s.Empty,
                Hidden = s.Hidden
            };

            using (var xml = new StringReader(s.IDs))
            {
                var ids = (List<String>)new XmlSerializer(typeof(List<String>)).Deserialize(xml);
                sq.IDs = ids;
            }

            using (var xml = new StringReader(s.SortFields))
            {
                var sfs = (List<SortField>)new XmlSerializer(typeof(List<SortField>)).Deserialize(xml);
                sq.SortFields = sfs;
            }

            using (var xml = new StringReader(s.FacetConstraints))
            {
                var fcs = (List<FacetConstraint>)new XmlSerializer(typeof(List<FacetConstraint>)).Deserialize(xml);
                sq.FacetConstraints = fcs;
            }

            return sq;
        }

        public KeyValuePair<Guid, String> SearchSave(Guid application_id, string lang_id, string user_id, SearchQuery query, int num_found)
        {
            var q = new Search();
            q.SearchID = Guid.Empty;

            try
            {
                q.SearchID = Guid.NewGuid();
                q.ApplicationID = application_id;
                q.UserId = query.UserID;
                q.DisplayName = query.DisplayName;
                q.Term = query.Term;
                q.SearchDate = DateTime.Now;
                q.ParentID = query.ParentID;
                q.Rows = query.Rows;
                q.RowStart = query.RowStart;
                q.NumFound = num_found;
                q.Empty = query.Empty;
                q.Hidden = query.Hidden;

                using (var writer = new StringWriter())
                {
                    new XmlSerializer(typeof(List<String>)).Serialize(writer, query.IDs);
                    q.IDs = writer.ToString();
                }

                using (var writer = new StringWriter())
                {
                    new XmlSerializer(typeof(List<SortField>)).Serialize(writer, query.SortFields);
                    q.SortFields = writer.ToString();
                }

                using (var writer = new StringWriter())
                {
                    new XmlSerializer(typeof(List<FacetConstraint>)).Serialize(writer, query.FacetConstraints);
                    q.FacetConstraints = writer.ToString();
                }
                
                _db.Searches.InsertOnSubmit(q);
                _db.SubmitChanges();
            }
            catch (Exception ex)
            {
                Helper.LogHelper.StatsLog(null, "SearchSave()", ex.Message, null, null);
            }
            return new KeyValuePair<Guid,string>(q.SearchID, q.DisplayName);
        }

        public bool SearchDelete(string user_id, Guid search_id)
        {
            try
            {
                var search = _db.Searches.Single(s => ((s.SearchID == search_id) && (s.UserId == user_id)));
                search.Deleted = true;
                _db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}