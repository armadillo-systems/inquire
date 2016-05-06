using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using iNQUIRE.Models;
using Microsoft.Practices.ServiceLocation;
//using Ninject;
using SolrNet;
using SolrNet.Exceptions;
using SolrNet.Impl;

namespace iNQUIRE.Helpers
{
    public static class SolrHelper
    {
        // if colons exist in the solr field names then we will have converted these manually
        // in the IInqItem implementation to underscores, so have to change them back in the
        // javascript code (field names can't have underscores AND colons because of this)
        public static bool ColonInFieldNames { get; set; }
        public static string FieldConcatenationString { get; set; }
        public static string ConcatenateFields { get; set; }
        private static bool _isSetup;

        static SolrHelper()
        {
            Setup();
        }

        public static void Setup()
        {
            if (!_isSetup)
            {
                try
                {
                    ColonInFieldNames = Convert.ToBoolean(ConfigurationManager.AppSettings["ColonInFieldNames"]);
                    FieldConcatenationString = ConfigurationManager.AppSettings["FieldConcatenationString"];
                    ConcatenateFields = ConfigurationManager.AppSettings["ConcatenateFields"];
                }
                catch (Exception e)
                {
                    iNQUIRE.Helper.LogHelper.StatsLog(null, String.Format("SolrHelper config error, check web.config parameters ColonInFieldNames: {0}, FieldConcatenationString: {1}, ConcatenateFields: {2}", ColonInFieldNames, FieldConcatenationString, ConcatenateFields), String.Format("Error {0}", e.Message), null, null);
                    ColonInFieldNames = false;
                }
                var solr_uri = ConfigurationManager.AppSettings["SolrUri"];

                //var connection1 = new SolrConnection(solr_uri);
                //var loggingConnection1 = new LoggingConnection(connection1);
                //Startup.Init<InqItemXml>(loggingConnection1);

                //using (IKernel kernel = new StandardKernel())
                //{
                //    kernel.Bind<IInqItem>()
                //        .To<InqItemXml>()
                //        .InSingletonScope();
                //    kernel.Load(new SolrNetModule(solr_uri));
                //}

                _isSetup = true;
            }
        }

        /// <summary>
        /// Adds some sample documents to Solr
        /// </summary>
        public static void AddInitialDocumentsFromXml(string solrUrl)
        {
            //try
            //{
            //    // var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemXml>>();

            //    using (IKernel kernel = new StandardKernel())
            //    {
            //        kernel.Bind<IInqItem>()
            //            .To<InqItemXml>()
            //            .InSingletonScope();
            //        kernel.Load(new SolrNetModule(solrUrl));
            //        var solr = ServiceLocator.Current.GetInstance<ISolrOperations<InqItemXml>>();

            //        solr.Delete(SolrQuery.All);
            //        solr.Commit();

            //        var items = XmlDataHelper.GetAllItemsFromXml();

            //        foreach (var item in items)
            //            solr.Add(item);
            //        solr.Commit();

            //        //solr.Add(new InqItemXml { Title = "Kodak EasyShare", ID = "17F595AF-293C-46F3-AB84-79A39FD579B5" });

            //        //var connection = ServiceLocator.Current.GetInstance<ISolrConnection>();
            //        //foreach (var file in Directory.GetFiles("", "*.xml"))
            //        //{
            //        //    connection.Post("/update", File.ReadAllText(file, Encoding.UTF8));
            //        //}
            //        //solr.Commit();
            //        //solr.BuildSpellCheckDictionary();
            //    }
            //}
            //catch (SolrConnectionException e)
            //{
            //    throw new Exception(string.Format("Solr {0}, error: {1}", solrUrl, e.Message));
            //}
        }
    }
}