using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpotOn.Web.Data;
using Blazor.FileReader;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Internal;

namespace SpotOn.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.AddServerSideBlazor().AddHubOptions(o =>
            {
                o.MaximumReceiveMessageSize = 50 * 1024 * 1024; // 50MB
            });
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<AppBase>();
            services.AddFileReaderService(options => options.InitializeOnFirstCall = true);
            
            // add syncfusion
            services.AddSyncfusionBlazor();
            services.AddMvc();
            string Antifor = Configuration.GetSection("ValidateAntiForgeryToken").Value;
            services.AddAntiforgery(x => { x.FormFieldName = Antifor; x.HeaderName = Antifor; });
            services.AddMvc(option => option.EnableEndpointRouting = false);
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // JT: Add Syncfusion license
            var sfLicense = Configuration["SpotOn:SfLicense"];
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(sfLicense);
      
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
           
        

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
