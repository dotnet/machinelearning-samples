using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using Microsoft.ML;
using LandUseML.Model;
using System.Reflection;

namespace LandUseAPI
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
            services.AddSingleton<PredictionEngine<ModelInput, ModelOutput>>(sp =>
            {
                // Initialize MLContext
                MLContext ctx = new MLContext();

                // Register NormalizeMapping
                ctx.ComponentCatalog.RegisterAssembly(typeof(NormalizeMapping).Assembly);
                
                // Register LabelMapping
                ctx.ComponentCatalog.RegisterAssembly(typeof(LabelMapping).Assembly);
                
                // Define model path
                var modelPath = Path.Join(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "MLModel.zip");
                
                //Load model
                ITransformer mlModel = ctx.Model.Load(modelPath, out var modelInputSchema);
                
                // Create prediction engine
                var predEngine = ctx.Model.CreatePredictionEngine<ModelInput,ModelOutput>(mlModel);

                return predEngine;
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
