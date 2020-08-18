using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Hosting;
using Microsoft.ML;
using TFImageClassification.ML;
using TFImageClassification.ML.DataModels;
using TFClassification.ML.DataModels;
using TFClassification.ML;

namespace TFImageClassification
{
    public class Startup
    {
        private readonly string _tensorFlowModelFilePath;
        private readonly ITransformer _mlnetModel;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _tensorFlowModelFilePath = GetAbsolutePath(Configuration["MLModel:TensorFlowModelFilePath"]);

            /////////////////////////////////////////////////////////////////
            //Configure the ML.NET model for the pre-trained TensorFlow model.
            var tensorFlowModelConfigurator = new TensorFlowModelConfigurator(_tensorFlowModelFilePath);
            _mlnetModel = tensorFlowModelConfigurator.Model;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddRazorPages();

            /////////////////////////////////////////////////////////////////////////////
            // Register the PredictionEnginePool as a service in the IoC container for DI.
            //
            services.AddPredictionEnginePool<ImageInputData, ImageLabelPredictions>();
            services.AddOptions<PredictionEnginePoolOptions<ImageInputData, ImageLabelPredictions>>()
                .Configure(options =>
                {
                    options.ModelLoader = new InMemoryModelLoader(_mlnetModel);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
        }
    }
}
