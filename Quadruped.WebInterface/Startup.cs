using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quadruped.Driver;
using Quadruped.WebInterface.RobotController;
using Quadruped.WebInterface.RTC;
using Quadruped.WebInterface.VideoStreaming;

namespace Quadruped.WebInterface
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddOptions();
            services.Configure<StreamerConfig>(Configuration.GetSection("streamerConfig"));

            if (HostingEnvironment.IsDevelopment())
            {
                services.AddSingleton<IRobot, MockRobot>();
                services.AddSingleton<IVideoService, MockVideoStream>();
                services.AddSingleton<ICameraController, CameraControllerMock>();
            }
            else
            {
                string portName = Configuration["SerialPortName"];
                services.AddSingleton(new DynamixelDriver(portName));
                services.AddSingleton<QuadrupedIkDriver>();
                services.AddSingleton<InterpolationGaitEngine>();
                services.AddSingleton<InterpolationController>();
                services.AddSingleton<IRobot, Robot>();
                services.AddSingleton<ICameraController, CameraController>();
                services.AddSingleton<IVideoService, VideoStreamingService>();
            }
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
