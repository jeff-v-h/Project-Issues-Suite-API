using AspNetCoreRateLimit;
using ProjectIssuesSuite.API.common.Models;
using ProjectIssuesSuite.API.domain.Frameworks;
using ProjectIssuesSuite.API.domain.Managers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace ProjectIssuesSuite.API.presentation
{
    public class Startup
    {
        // Initiate config with appSettings.json through IConfiguration via the Dependency Injection system
        // appsettings.json file's are linked to ASPNETCORE_ENVIRONMENT variable
        // which can be access by right clicking .sln and going to properties, then Debug tab
        public static IConfiguration Configuration { get; private set; }
        public IHostingEnvironment Environment { get; private set; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add middleware - in-memory cache and rate limiting
            services.AddLazyCache();

            services.AddMvc()
                .AddMvcOptions(o => o.OutputFormatters.Add(
                    new XmlDataContractSerializerOutputFormatter()));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Project Issues Suite", Version = "v1" });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    builder =>
                    {
                        builder.WithOrigins(
                            "https://project-issues-suite.azurewebsites.net",
                            "http://localhost:8080")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
            });

            // Sets limit for each multipart body (each individual video file size). Total request size limit set in web.config
            services.Configure<FormOptions>(x => x.MultipartBodyLengthLimit = 324288000); // 300 MB

            // Configure DB settings. First get the sections from appSettings.json
            // Then bind the section to the class model and register it to the Configuration instance
            // Theses injected into classes with IOptions<MyClass> and MyClass type
            services.Configure<DbSettings>(Configuration.GetSection("DbSettings"));
            services.Configure<DbData>(Configuration.GetSection("DbData"));
            services.Configure<VideoStorageSettings>(Configuration.GetSection("VideoStorageSettings"));

            // Do the following config unless in testing environment 
            if (!Environment.IsEnvironment("testing"))
            {
                services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
                services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
                services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
                services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

                // use appSettings values via Configuration to implement a
                // new DocumentClient and map it to IDocumentClient interface to be used via dependency injection
                services.AddTransient<IDocumentClient>(s =>
                    new DocumentClient(new System.Uri(Configuration["DbSettings:EndpointUri"]), Configuration["DbSettings:PrimaryKey"]));

                services.AddTransient<InitDatabaseManager>();
            }

            // Add services and Map the interfaces to classes
            ServiceManager.InjectServices(services);

            services.AddScoped<IProjectManager, ProjectManager>();
            services.AddScoped<ITicketManager, TicketManager>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IVideoManager, VideoManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (!env.IsEnvironment("testing"))
            {
                app.UseIpRateLimiting();

                // Get the required services before configuration has completed
                var idb = app.ApplicationServices.GetRequiredService<InitDatabaseManager>();
                // Create an initial database and seed data if it has not been created yet
                idb.InitDatabase().Wait();
            }

            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                //app.UseExceptionHandler("/error");
            }

            app.UseCors("AllowSpecificOrigins");

            // Status Code Pages middleware to be used before request handling middleware
            app.UseStatusCodePages();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Project Issues Suite API V1");
                // Set RoutePrefix to an empty string to serve sqagger UI as the app's root
                c.RoutePrefix = string.Empty;
            });

            app.UseMvc();
        }
    }
}
