using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iNQUIRE.Models
{
    public interface IInqItemMultiLingual
    {
        Dictionary<string, List<string>> ParseMultilingualStringListToDictionary(ICollection<string> strings);
        Dictionary<string, List<string>> MakeMultilingualStringListDictionary(string property_name);
    }
}