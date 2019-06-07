using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using WebApp.ML;
using WebApp.ML.DataModels;

namespace WebApp
{
    public class Startup
    {
        private readonly string _tensorFlowModelFilePath;
        private readonly string _mlnetModelFilePath;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _tensorFlowModelFilePath = GetAbsolutePath(Configuration["MLModel:TensorFlowModelFilePath"]);
            _mlnetModelFilePath = GetAbsolutePath(Configuration["MLModel:MLNETModelFilePath"]);

            /////////////////////////////////////////////////////////////////
            //Configure the ML.NET model for the pre-trained TensorFlow model
            TensorFlowModelConfigurator tensorFlowModelConfigurator = new TensorFlowModelConfigurator(_tensorFlowModelFilePath);

            // Save the ML.NET model .zip file based on the TensorFlow model and related configuration
            tensorFlowModelConfigurator.SaveMLNetModel(_mlnetModelFilePath);
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
            // Register the PredictionEnginePool as a service in the IoC container for DI
            //
            services.AddPredictionEnginePool<ImageInputData, ImageLabelPredictions>()
                    .FromFile(_mlnetModelFilePath);
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

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
        }
    }
}
