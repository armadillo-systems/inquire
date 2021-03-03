using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
//using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Commands.Parameters;
using iNQUIRE.Helper;

namespace iNQUIRE.Models
{
    public class iNQUIRESolrRepository : SolrRepository
    {
        public iNQUIRESolrRepository()
        {
            _solrIdField = ConfigurationManager.AppSettings["ObjectIdFieldName"];
            _solrFileField = "MediaFilename";
            _solrUri = ConfigurationManager.AppSettings["SolrUri"];
            // _solrJpeg2000IdPrefix = ConfigurationManager.AppSettings["SolrUriArmDb"];
        }

        public override bool UpdateFileFieldToJpeg2000(string id, string value)
        {
            throw new Exception("No longer implemented");

            // for data coming from sql server (eg BL crimea) need to update the media table with the new djatoka id
            // otherwise when data is re-imported from sql server to solr, the djatoka id will be lost from solr, and
            // the system will think the image is not a jp2 anymore
            
            // should not need to call dispose on DataContext object below (ie use "using" or "finally"), as resources are not held open
            //try
            //{
            //    var db = new iNQUIREDataContext();

            //    // check to see if tag already exists
            //    var media = db.Medias.SingleOrDefault(m => m.Filename == id); // will throw an error if more than 1 record found

            //    if ((media == null) || (media.MediaID == Guid.Empty)) // if none found
            //        throw new Exception(String.Format("Error, no Media item found with this existing filename to update {0}", media.MediaID));

            //    media.Filename = value;
            //    db.SubmitChanges();
            //    LogHelper.StatsLog(null, "ArmDbSolrRepository.UpdateFileFieldToJpeg2000() OK", String.Format("Existing file {0}, new value {1}", id, value), null, null);
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.StatsLog(null, "ArmDbSolrRepository.UpdateFileFieldToJpeg2000() failed", String.Format("Failed for existing file {0}, new value {1}, reason: {2}", id, value, ex.Message), null, null);
            //}

            //return base.UpdateFileFieldToJpeg2000(id, value);
        }


        /*public override SolrSearchResults GetRecord(string id)
        {
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemArmNode>>();
            var results = solr.Query(new SolrQuery(String.Format("ObjectLocaleID:{0}", id)), new QueryOptions { Rows = 1, Start = 0 });
            return makeSolrSearchResults(results);
        }

        public override SolrSearchResults GetSearchSuggestions(string str)
        {
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemArmNode>>();
            var results = solr.Query(new SolrQuery(str), new QueryOptions { SpellCheck = new SpellCheckingParameters { } });
            return makeSolrSearchResults(results);
        }*/
    }
}