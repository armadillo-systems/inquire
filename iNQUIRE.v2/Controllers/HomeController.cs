using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace iNQUIRE.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public void Index()
        {
            // Response.Redirect("~/Discover/Index");
             Response.Redirect("~/Discover/Search/"); //#/?p=c+1,t+,rsrs+0,rsps+10,fa+,scids+,pid+,vi+
            //Response.Redirect("~/Discover/Print/"); 
            // Response.Redirect("~/Discover/DeepZoom?id=info:arm/test/red_enzo");
        }

    }
}
