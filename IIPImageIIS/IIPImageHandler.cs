using System.IO;
using System.Net;
using System.Text;
using System.Web;
using iNQUIRE.Helper;

namespace IIPImageIIS
{
    public class IIPImageHandler : JP2HandlerBase
    {
        public override bool IsJpeg(string url)
        {
            return !string.IsNullOrEmpty(url) ? url.Contains("RGN") : false;
        }

        public override bool IsJson(string url)
        {
            return !string.IsNullOrEmpty(url) ? url.Contains("Max-size") : false;
        }

        public override string ProxyFixHTML(string content)
        {
            throw new System.NotImplementedException();
        }
    }
}