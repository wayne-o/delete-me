using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Microsoft.Practices.Unity;
using System.Data.Entity;
using System.Net.Http.Headers;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Implementation;
using Infrastructure.BlobStorage;
using Infrastructure.Sql.BlobStorage;
using Registration.ReadModel.Implementation;
using Payments.ReadModel.Implementation;
using Registration.ReadModel;
using Payments.ReadModel;
using System.Runtime.Caching;
using System.Web.Http.Cors;
using Infrastructure.Azure.Messaging;
using Infrastructure.Azure;
using System.Web;
using Infrastructure;
using Conference.Common;

namespace Conference.Api.Public
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.

            // Web API routes
            config.MapHttpAttributeRoutes();

            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            config.Filters.Add(new ExceptionHandlingAttribute());
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var container = new UnityContainer();

            try
            {
                // repositories used by the application

                container.RegisterType<ConferenceRegistrationDbContext>(new TransientLifetimeManager(), new InjectionConstructor("ConferenceRegistration"));
                container.RegisterType<PaymentsReadDbContext>(new TransientLifetimeManager(), new InjectionConstructor("Payments"));

                var cache = new MemoryCache("ReadModel");
                container.RegisterType<IOrderDao, OrderDao>();
                container.RegisterType<IConferenceDao, CachingConferenceDao>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionConstructor(new ResolvedParameter<ConferenceDao>(), cache));
                container.RegisterType<IPaymentDao, PaymentDao>();

                // configuration specific settings

                var serializer = new JsonTextSerializer();
                container.RegisterInstance<ITextSerializer>(serializer);

                container.RegisterType<IBlobStorage, SqlBlobStorage>(new ContainerControlledLifetimeManager(), new InjectionConstructor("BlobStorage"));
                container.RegisterType<Infrastructure.Sql.Messaging.IMessageSender, MessageSender>(
                    "Commands", new TransientLifetimeManager(), new InjectionConstructor(Database.DefaultConnectionFactory, "SqlBus", "SqlBus.Commands"));
                container.RegisterType<ICommandBus, Infrastructure.Sql.Messaging.CommandBus>(
                    new ContainerControlledLifetimeManager(), new InjectionConstructor(new ResolvedParameter<Infrastructure.Sql.Messaging.IMessageSender>("Commands"), serializer));


                config.DependencyResolver = new UnityResolver(container);

                IEventBus eventBus = null;
//#if LOCAL
            eventBus = new Infrastructure.Sql.Messaging.EventBus(new MessageSender(Database.DefaultConnectionFactory, "SqlBus", "SqlBus.Events"), serializer);
//#else
//                var settings = InfrastructureSettings.Read(HttpContext.Current.Server.MapPath(@"~\bin\Settings.xml")).ServiceBus;

//                if (!MaintenanceMode.IsInMaintainanceMode)
//                {
//                    new ServiceBusConfig(settings).Initialize();
//                }

//                eventBus = new Infrastructure.Azure.Messaging.EventBus(new TopicSender(settings, "conference/events"), new StandardMetadataProvider(), serializer);
//#endif

                container.RegisterInstance<IEventBus>(eventBus);

                var conferenceService = new ConferenceService(eventBus);

                container.RegisterInstance<ConferenceService>(conferenceService);
            }
            catch
            {
                container.Dispose();
                throw;
            }
        }
    }
}
