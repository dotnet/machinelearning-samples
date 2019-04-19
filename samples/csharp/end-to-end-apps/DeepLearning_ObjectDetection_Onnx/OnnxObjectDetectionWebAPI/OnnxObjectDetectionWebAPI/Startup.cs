using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using OnnxObjectDetectionWebAPI.Infrastructure;
using OnnxObjectDetectionWebAPI.OnnxModelScorers;
using Swashbuckle.AspNetCore.Swagger;

namespace OnnxObjectDetectionWebAPI
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "TensorFlow ImageClassification WebAPI", Version = "v1" });
            });

            // Register types (Interface/Class pairs) to use in DI/IoC
            services.AddTransient<IImageFileWriter, ImageFileWriter>();

            // Set TFModelScorer to be re-used across calls because there are expensive initializations
            // like when creating the prediciton function.
            // If set to be used as Singleton is very important to use critical sections in the code
            // because the 'Predict()' method is not reentrant. 
            //
            services.AddSingleton<IOnnxModelScorer, OnnxModelScorer>();

            // Another choice is to create the TFModelScorer as ServiceLifetime.Scoped or .AddScoped() when adding to services
            // In this case you don't need a critical section but every Http request will need to create a prediction function
            // You will benefit only in the cases where you do multiple predictions within the same Http request.
            //
            //services.AddScoped<ITFModelScorer, TFModelScorer>();

            // For further info on DI/IoC service lifetimes check this out:
            // https://blogs.msdn.microsoft.com/cesardelatorre/2017/01/26/comparing-asp-net-core-ioc-service-life-times-and-autofac-ioc-instance-scopes/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            //Use this to set path of files outside the wwwroot folder
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "ImagesTemp")),
                RequestPath = "/ImagesTemp"
            });

            //If using wwwroot/images folder
            //app.UseStaticFiles(); //letting the application know that we need access to wwwroot folder.

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TensorFlow ImageClassification WebAPI - V1");
            });

            app.UseMvc();
        }
    }
}
