using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using iNQUIRE.Models;

namespace iNQUIRE.Controllers.WebApi
{
    public abstract class WebApiControllerBase : ApiController
    {
        public static Guid ApplicationIdInquire { get; set; }

        // private readonly Helper.IJP2Helper _IJP2Helper;
        //private readonly IRepository _IRepository;
        //private readonly IUserCollectionRepository<Workspace, WorkspaceItem, string> _IUserCollectionRepository;
        //private readonly IUserSearchRepository _IUserSearchRepository;

        //public WebApiControllerBase()
        //{

        //}



    }
}