using eShopDashboard.Forecast;
using eShopDashboard.Infrastructure.Data.Catalog;
using eShopDashboard.Infrastructure.Data.Ordering;
using eShopDashboard.Infrastructure.Setup;
using eShopDashboard.Queries;
using eShopDashboard.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML;
using Serilog;

using Common;

namespace eShopDashboard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CatalogContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<OrderingContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IOrderingQueries, OrderingQueries>();
            services.AddScoped<ICatalogQueries, CatalogQueries>();
            services.AddScoped<CatalogContextSetup>();
            services.AddScoped<OrderingContextSetup>();


            //MLContext created as singleton for the whole ASP.NET Core app
            services.AddSingleton<MLContext, MLContext>((ctx) =>
            {
                //Seed set to any number so you have a deterministic environment
                return new MLContext(seed: 1);
            });

            services.AddSingleton <MLModelEngine<ProductData, ProductUnitPrediction>>((ctx) =>
            {
                MLContext mlContext = ctx.GetRequiredService<MLContext>();
                string modelFolder = Configuration["ForecastModelsPath"];
                string modelFilePathName = $"{modelFolder}/product_month_fastTreeTweedie.zip";
                return new MLModelEngine<ProductData, ProductUnitPrediction>(mlContext, modelFilePathName);
            });

            services.AddSingleton<MLModelEngine<CountryData, CountrySalesPrediction>>((ctx) =>
            {
                MLContext mlContext = ctx.GetRequiredService<MLContext>();
                string modelFolder = Configuration["ForecastModelsPath"];
                string modelFilePathName = $"{modelFolder}/country_month_fastTreeTweedie.zip";
                return new MLModelEngine<CountryData, CountrySalesPrediction>(mlContext, 
                                                                              modelFilePathName, 
                                                                              minPredictionEngineObjectsInPool:15,
                                                                              maxPredictionEngineObjectsInPool:1000,
                                                                              expirationTime:60000);
            });

            services.Configure<CatalogSettings>(Configuration.GetSection("CatalogSettings"));

            services.AddMvc();

            services.Configure<AppSettings>(Configuration);

            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "eShopDashboard - API",
                    Version = "v1",
                    Description = "Web Dashboard REST HTTP API.",
                    TermsOfService = "Terms Of Service"
                });
            });

            //Get info on Thread Pooling just for debugging/exploring, this code can be deleted:
            int worker = 0;
            int io = 0;
            System.Threading.ThreadPool.GetAvailableThreads(out worker, out io);

            Log.Information("Thread pool threads available at startup: ");
            Log.Information("   Worker threads: {0:N0}", worker);
            Log.Information("   Asynchronous I/O threads: {0:N0}", io);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc();

            var pathBase = Configuration["PATH_BASE"];

            app.UseSwagger()
              .UseSwaggerUI(c =>
              {
                  c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "eShopDashboard.API V1");
              });
        }
    }
}
