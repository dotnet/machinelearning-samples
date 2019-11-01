using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using ImageClassification.WebApp.ML.DataModels;
using ImageClassification.DataModels;
using Microsoft.Extensions.Options;
using System.Drawing;
using TensorFlowImageClassification.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Internal;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace ImageClassification.WebApp
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            /////////////////////////////////////////////////////////////////////////////
            // Register the PredictionEnginePool as a service in the IoC container for DI.
            //
            services.AddPredictionEnginePool<InMemoryImageData, ImagePrediction>()
                    .FromFile(Configuration["MLModel:MLModelFilePath"]);

            // (Optional) Get the pool to initialize it and warm it up.       
            //WarmUpPredictionEnginePool(services);
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }

        public static void WarmUpPredictionEnginePool(IServiceCollection services)
        {
            //#1 - Simply get a Prediction Engine
            var predictionEnginePool = services.BuildServiceProvider().GetRequiredService<PredictionEnginePool<InMemoryImageData, ImagePrediction>>();
            var predictionEngine = predictionEnginePool.GetPredictionEngine();
            predictionEnginePool.ReturnPredictionEngine(predictionEngine);

            // #2 - Predict
            // Get a sample image
            //
            //Image img = Image.FromFile(@"TestImages/BlackRose.png");
            //byte[] imageData;
            //IFormFile imageFile;
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //    //To byte[] (#1)
            //    imageData = ms.ToArray();

            //    //To FormFile (#2)
            //    imageFile = new FormFile((Stream)ms, 0, ms.Length, "BlackRose", "BlackRose.png");
            //}

            //var imageInputData = new InMemoryImageData(image: imageData, label: null, imageFileName: null);

            //// Measure execution time.
            //var watch = System.Diagnostics.Stopwatch.StartNew();

            //var prediction = predictionEnginePool.Predict(imageInputData);

            //// Stop measuring time.
            //watch.Stop();
            //var elapsedMs = watch.ElapsedMilliseconds;
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
