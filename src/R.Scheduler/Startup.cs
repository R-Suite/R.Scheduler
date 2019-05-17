using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Newtonsoft.Json.Serialization;
using Owin;
using StructureMap;

namespace R.Scheduler
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 

            var config = new HttpConfiguration();

            config.EnableCors();

            config.MessageHandlers.Add(new CorsHeaderHandler());

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();

            config.Services.Replace(typeof(IHttpControllerActivator),
                new StructureMapHttpControllerActivator(Scheduler.Container));

            appBuilder.UseWebApi(config);
        } 
    }

    public class StructureMapHttpControllerActivator : IHttpControllerActivator

    {
        private readonly IContainer _container;

        public StructureMapHttpControllerActivator(IContainer container)

        {
            this._container = container;
        }

        public IHttpController Create(

            HttpRequestMessage request,

            HttpControllerDescriptor controllerDescriptor,

            Type controllerType)

        {
            return (IHttpController) this._container.GetInstance(controllerType);
        }

    }

    public class CorsHeaderHandler : DelegatingHandler
    {
        private const string Origin = "Origin";
        private const string AccessControlRequestMethod = "Access-Control-Request-Method";
        private const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        private const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        private const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var isCorsRequest = request.Headers.Contains(Origin);
            var isPreflightRequest = request.Method == HttpMethod.Options;

            if (isCorsRequest)
            {
                if (isPreflightRequest)
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var response = new HttpResponseMessage(HttpStatusCode.OK);
                        response.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());

                        var currentAccessControlRequestMethod = request.Headers.GetValues(AccessControlRequestMethod).FirstOrDefault();

                        if (currentAccessControlRequestMethod != null)
                        {
                            response.Headers.Add(AccessControlAllowMethods, currentAccessControlRequestMethod);
                        }

                        var requestedHeaders = string.Join(", ", request.Headers.GetValues(AccessControlRequestHeaders));

                        if (!string.IsNullOrEmpty(requestedHeaders))
                        {
                            response.Headers.Add(AccessControlAllowHeaders,requestedHeaders);
                        }

                        return response;
                    }, cancellationToken);
                }

                return base.SendAsync(request, cancellationToken).ContinueWith(t =>
                {
                    var resp = t.Result;
                    resp.Headers.Add(AccessControlAllowOrigin,request.Headers.GetValues(Origin).First());
                    return resp;

                }, cancellationToken);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
