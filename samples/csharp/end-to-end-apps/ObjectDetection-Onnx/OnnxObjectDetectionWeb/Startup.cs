using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using OnnxObjectDetectionWeb.Infrastructure;
using OnnxObjectDetectionWeb.Services;
using OnnxObjectDetectionWeb.Utilities;
using OnnxObjectDetection;
using System.IO;
using Common;

namespace OnnxObjectDetectionWeb
{
    public class Startup
    {
        private readonly string _onnxModelFilePath;
        private readonly string _mlnetModelFilePath;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _onnxModelFilePath = CommonHelpers.GetAbsolutePath(Configuration["MLModel:OnnxModelFilePath"]);
            _mlnetModelFilePath = CommonHelpers.GetAbsolutePath(Configuration["MLModel:MLNETModelFilePath"]);
            
            if (!System.IO.File.Exists(_onnxModelFilePath))
            {
                var graphZip = "TinyYolo2_model.onnx";
                var graphUrl = "https://bit.ly/3rdrfKe";
                var commonGraphsRelativePath = @"../../../../../../../../graphs";
                var commonGraphsPath = CommonHelpers.GetAbsolutePath(commonGraphsRelativePath);
                var modelRelativePath = @"../../../../OnnxObjectDetection/ML/OnnxModels";
                string modelPath = CommonHelpers.GetAbsolutePath(modelRelativePath);
                Web.DownloadBigFile(modelPath, graphUrl, graphZip, commonGraphsPath);
                // Restart to copy TinyYolo2_model.onnx to bin\Debug\net6.0\ML\OnnxModels
                System.Environment.Exit(0);
            }

            var onnxModelConfigurator = new OnnxModelConfigurator(new TinyYoloModel(_onnxModelFilePath));

            onnxModelConfigurator.SaveMLNetModel(_mlnetModelFilePath);
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

            services.AddControllers();
            services.AddRazorPages();

            services.AddPredictionEnginePool<ImageInputData, TinyYoloPrediction>().
                FromFile(_mlnetModelFilePath);

            services.AddTransient<IImageFileWriter, ImageFileWriter>();
            services.AddTransient<IObjectDetectionService, ObjectDetectionService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == Environments.Development)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }       
    }
}
