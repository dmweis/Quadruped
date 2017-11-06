using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Quadruped.WebInterface
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                SaveCorrectDxlLibrary();
            }
            catch (Exception )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to load library!!!");
                Console.ResetColor();
            }
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("QuadrupedConfig.json");
                })
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:50093/")
                .Build();

        private const string DxlLib = "dxl_lib.ds";

        private static void SaveCorrectDxlLibrary()
        {
            // This is an extremly ugly way to load the correct library for dynamixel
            // TODO: figure out a better way
            Console.ForegroundColor = ConsoleColor.Yellow;
            var architecture = RuntimeInformation.OSArchitecture;
            Console.WriteLine($"Current architecture {architecture}");
            if (architecture == Architecture.Arm || architecture == Architecture.Arm64)
            {
                const string newFileName = "libdxl_sbc_c.so";
                Console.WriteLine($"Saving library as {newFileName}");
                File.Copy(newFileName, DxlLib, true);
            }
            else if (architecture == Architecture.X64)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Console.WriteLine($"Current OS is {OSPlatform.Windows}");
                    const string newFileName = "dxl_x64_c.dll";
                    Console.WriteLine($"Saving library as {newFileName}");
                    File.Copy(newFileName, DxlLib, true);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Console.WriteLine($"Current OS is {OSPlatform.Linux}");
                    const string newFileName = "libdxl_x64_c.so";
                    Console.WriteLine($"Saving library as {newFileName}");
                    File.Copy(newFileName, DxlLib, true);
                }
                else
                {
                    throw new NotSupportedException("This OS is not supported!");
                }
            }
            else
            {
                throw new NotSupportedException("This architecture is not supported!");
            }
            Console.ResetColor();
        }
    }
}
