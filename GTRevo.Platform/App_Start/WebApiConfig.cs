﻿using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using GTRevo.Platform.IO;
using GTRevo.Platform.Web;

namespace GTRevo.Platform
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.AddODataQueryFilter();
            config.EnableDependencyInjection();
            config.Select().Expand().Filter().OrderBy().MaxTop(null).Count(); //enable common OData options

            /*config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;*/
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new BracesGuidJsonConverter());

            config.Services.Replace(typeof(IHttpActionSelector), new HyphenApiControllerActionSelector());
            config.Services.Replace(typeof(IHttpControllerSelector), new ApiControllerSelector(config));
            config.Services.Replace(typeof(IHttpControllerTypeResolver), new ApiControllerTypeResolver());
        }
    }
}