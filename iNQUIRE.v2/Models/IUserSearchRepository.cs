using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public interface IUserSearchRepository
    {
        KeyValuePair<Guid, String> SearchSave(Guid application_id, string lang_id, string user_id, SearchQuery query, int num_found);
        List<SearchQuery> GetSearches(Guid application_id, string user_id, int num);
        bool SearchDelete(string user_id, Guid search_id);
    }
}