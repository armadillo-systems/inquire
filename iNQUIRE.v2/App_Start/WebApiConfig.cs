using System.Web.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using iNQUIRE.Models;
using iNQUIRE.Helper;
using Unity;
using Microsoft.Practices.Unity.Configuration;

namespace iNQUIRE
{
    public static class WebApiConfig
    {
        public static IUnityContainer UnityContainer;

        public static void Register(HttpConfiguration config)
        {
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Re‌​ferenceLoopHandling = ReferenceLoopHandling.Ignore;

            var container = CreateConfiguredUnityContainer();
            // container.RegisterType<ISipRepository, SipDatabaseRepository>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);
            UnityContainer = container;

            // Web API configuration and services
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            // config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
            // config.Formatters.XmlFormatter.UseXmlSerializer = true;

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //if (System.Web.HttpContext.Current.IsDebuggingEnabled == false)
                config.Filters.Add(new AuthorizeAttribute());
        }

        private static IUnityContainer CreateConfiguredUnityContainer()
        {
            IUnityContainer container = new UnityContainer();

            // (optional) default mapping
            //container.RegisterType<ITest<Workspace, Workspace>, Test>();
            container.RegisterType<IUserCollectionRepository<Workspace, WorkspaceItem, string>, LinqToSqlUserCollectionRepository>();
            container.RegisterType<IUserTagRepository<Tag, TaggedItem, string>, LinqToSqlUserTagRepository>();
            container.RegisterType<IUserNoteRepository<Note, string>, LinqToSqlUserNoteRepository>();
            container.RegisterType<IRepository, iNQUIRESolrRepository>();
            container.RegisterType<IUserSearchRepository, LinqToSqlUserSearchRepository>();
            container.RegisterType<IJP2Helper, IIPImageHelper>();

            container.RegisterType<IInqItem, InqItemBase>();
            // (optional) load static config from the *.xml file
            container.LoadConfiguration();

            return container;
        }
    }
}
