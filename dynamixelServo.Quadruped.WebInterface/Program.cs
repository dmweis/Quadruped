using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace dynamixelServo.Quadruped.WebInterface
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SaveCorrectDxlLibrary();
            BuildWebHost(args).Run();
            DeleteDxlLibrary();
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
                .UseUrls("http://0.0.0.0:5001/")
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
        private static void DeleteDxlLibrary()
        {
            // This is an extremly ugly way to load the correct library for dynamixel
            // TODO: figure out a better way
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Deleting {DxlLib}");
            File.Delete(DxlLib);
            Console.ResetColor();
        }
    }
}
