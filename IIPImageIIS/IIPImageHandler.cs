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
            return url.Contains("RGN");
        }

        public override bool IsJson(string url)
        {
            return url.Contains("Max-size");
        }

        public override string ProxyFixHTML(string content)
        {
            throw new System.NotImplementedException();
        }
    }
}