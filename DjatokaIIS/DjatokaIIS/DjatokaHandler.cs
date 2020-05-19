using System.IO;
using System.Net;
using System.Text;
using System.Web;
using iNQUIRE.Helper;

namespace DjatokaIIS
{
    public class DjatokaHandler : JP2HandlerBase
    {
        public override bool IsJpeg(string url)
        {
            return !string.IsNullOrEmpty(url) ? url.Contains("getRegion") : false;
        }

        public override bool IsJson(string url)
        {
            return !string.IsNullOrEmpty(url) ? url.Contains("getMetadata") : false;
        }

        public override string ProxyFixHTML(string content)
        {
            // if we are here we are proxying the g:\tomcat\webapps\djatoka-viewer\viewer.html file,
            // but paths will be wrong if being proxyed as eg http://localhost/iNQUIRE/viewer.dja?rft_id=info:arm/test/greek_map
            // so need to fix all the paths via string replacement, otherwise javascript/images/css won't be found
            // plus need to make javascript look like it's coming from the same base uri, else will get ajax security exceptions
            content = content.Replace("href=\"css/", string.Format("href=\"{0}/Content/djatoka-viewer/css/", JP2ConfigHelper.ApplicationBaseUri));
            content = content.Replace("src=\"javascript/", string.Format("src=\"{0}/Scripts/djatoka-viewer/", JP2ConfigHelper.ApplicationBaseUri));
            content = content.Replace("href=\"images/", string.Format("href=\"{0}/Content/djatoka-viewer/images/", JP2ConfigHelper.ApplicationBaseUri));
            return content;
        }
    }
}