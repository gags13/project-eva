using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LUIS.Settings;
using Microsoft.Azure.Documents.Client;
using LUIS.Controllers;
using LUIS.Handler;
using LUIS.Intefaces;
using Microsoft.Azure.Documents;
using Microsoft.AspNetCore.Cors;
namespace LUIS
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add service and create Policy with options
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            // Add framework services.
            services.AddMvc();

            IOTHubSettings iotHubSettings = IOTHubConfig();
            services.AddSingleton<IIOTHubSettings>(iotHubSettings);
            var ih = new IotHubHandler(iotHubSettings);
            services.AddSingleton(ih);

            AzureSqlSettings azureSqlSettings = AzureSqlConfig();
            services.AddSingleton<IAzureSqlSettings>(azureSqlSettings);
            var ass = new AzureSqlHandler(azureSqlSettings);
            services.AddSingleton(ass);

            DocumentDBSettings ddbSettings = DocumentDBConfig();
            services.AddSingleton<IDocumentDBSettings>(ddbSettings);
            DocumentClient ddbClient = new DocumentClient(new Uri(ddbSettings.EndpointUrl), ddbSettings.PrimaryKey);
            services.AddSingleton<IDocumentClient>(ddbClient);
            services.AddSingleton(new AlexaController(ddbClient, ddbSettings,ih,ass));

            TwilioSettings twilioSetting = TwilioConfig();
            services.AddSingleton<ITwilioSettings>(twilioSetting);
            services.AddSingleton(new TwilioController(twilioSetting));

            LuisSettings luisSettings = LuisConfig();
            services.AddSingleton<ILuisSettings>(luisSettings);
            services.AddSingleton(new LuisController(luisSettings,ddbClient,ddbSettings,ih,ass));




            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }

        /* User Defined Configurations */

        private DocumentDBSettings DocumentDBConfig()
        {
            var ddbSection = Configuration.GetSection("DocDB");
            DocumentDBSettings ddbSettings = new DocumentDBSettings
            {
                EndpointUrl = ddbSection["EndpointUrl"],
                PrimaryKey = ddbSection["PrimaryKey"],
                database = ddbSection["database"],
                logCollection = ddbSection["logCollection"],
                slotValuesCollection = ddbSection["slotValuesCollection"]

            };
            return ddbSettings;

        }

        private IOTHubSettings IOTHubConfig()
        {

            var IOTHubSection = Configuration.GetSection("IotHub");
            return new IOTHubSettings
            {
                connectionString = IOTHubSection["connectionString"]
            };
        }

        private TwilioSettings TwilioConfig()
        {
            var twilioSection = Configuration.GetSection("Twilio");
            return new TwilioSettings
            {
                accountSid= twilioSection["accountSid"],
                authToken= twilioSection["authToken"],
                from= twilioSection["from"]
            };

        }

        private LuisSettings LuisConfig() {

            return new LuisSettings {
                LuisEndpoint = (string)Configuration.GetValue(typeof(string), "LuisEndpoint")
            };
        }

        private AzureSqlSettings AzureSqlConfig() {

            var azureSqlSection = Configuration.GetSection("AzureSQL");
            return new AzureSqlSettings
            {
                DataSource = azureSqlSection["DataSource"],
                InitialCatalog=azureSqlSection["InitialCatalog"],
                Password= azureSqlSection["Password"],
                UserID= azureSqlSection["UserID"]
            };
        }
    }
}
