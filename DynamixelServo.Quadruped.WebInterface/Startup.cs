using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dynamixelServo.Quadruped.WebInterface.RTC;
using DynamixelServo.Driver;
using DynamixelServo.Quadruped;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dynamixelServo.Quadruped.WebInterface
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
            services.AddMvc();
            string portName = Configuration["SerialPortName"];
            services.AddSingleton(new DynamixelDriver(portName));
            services.AddSingleton<QuadrupedIkDriver>();
            services.AddSingleton<BasicQuadrupedGaitEngine>();
            services.AddSingleton<RobotController>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseWebSocketRtc();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
