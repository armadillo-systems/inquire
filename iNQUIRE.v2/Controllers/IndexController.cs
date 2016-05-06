using System;
using System.Configuration;
using System.Web.Mvc;
using iNQUIRE.Helpers;
using iNQUIRE.Models;

namespace iNQUIRE.Controllers
{
    public class IndexController : Controller
    {
        private readonly IRepository _IRepository;

        public IndexController(IRepository irepository)
        {
            _IRepository = irepository;
        }

        public ActionResult Index()
        {
            // Indexes Solr from the xml data file taken from application config
            // to re-index Crimea from sql server (change app config to Solr core1) visit: http://armserv:8080/solr/core1/dataimport?command=full-import
            var solr_uri = ConfigurationManager.AppSettings["SolrUri"];
            var is_xml_data = (String.IsNullOrEmpty(_IRepository.AppDataXml) == false);
            if (is_xml_data)
            {
                _IRepository.Load();
                SolrHelper.AddInitialDocumentsFromXml(solr_uri);
            }
            return View();
        }

    }
}
