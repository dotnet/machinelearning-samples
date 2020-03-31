using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using LandUseUWPML.Model;
using Microsoft.ML;
using Microsoft.Extensions.ML;

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
            services.AddControllers();
            services.AddSingleton<PredictionEngine<ModelInput, ModelOutput>>((opt) =>
            {
                MLContext mlContext = new MLContext();
                mlContext.ComponentCatalog.RegisterAssembly(typeof(NormalizeMapping).Assembly);
                mlContext.ComponentCatalog.RegisterAssembly(typeof(LabelMapping).Assembly);
                ITransformer model = mlContext.Model.Load("MLModel.zip", out var inputSchema);
                return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            });
            //services.AddPredictionEnginePool<ModelInput, ModelOutput>()
            //    .FromFile("MLModel.zip");
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
