using MessengerStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBitcoin;
using System;
using System.IO;

namespace CloudServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            string entryPoint = (string)configuration.GetValue(typeof(string), "EntryPoint", null);
            if (entryPoint == string.Empty)
                entryPoint = null;
            string privateKey = (string)configuration.GetValue(typeof(string), "PrivateKey", null);
            if (privateKey == string.Empty)
                privateKey = null;
            string appDataPath = (string)configuration.GetValue(typeof(string), "DataPath", null);
            if (appDataPath == string.Empty)
                appDataPath = null;
            string eraseAfterDaysString = (string)configuration.GetValue(typeof(string), "EraseAfterDays", 0);
            int eraseAfterDays = 0;
            if (int.TryParse(eraseAfterDaysString, out int days))
            {
                eraseAfterDays = days;
            }
#if DEBUG_RAM
            EraseNotUsedAccountTimer = new System.Threading.Timer(new System.Threading.TimerCallback((state) => EraseNotUsedAccount()), null, new TimeSpan(0, 1, 0), new TimeSpan(0, 1, 0));
#endif
            Storage = new RemoteStorage(appDataPath, entryPoint, eraseAfterDays, privateKey);
        }
        public static RemoteStorage Storage;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

        }
    }
}
