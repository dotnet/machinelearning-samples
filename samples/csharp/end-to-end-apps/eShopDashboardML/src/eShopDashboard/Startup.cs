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

            services.AddTransient<IProductSales, ProductSales>();
            services.AddTransient<ICountrySales, CountrySales>();
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

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
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
