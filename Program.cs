using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Asp
{
    public class Program
    {
        internal static int Instances = int.Parse(Environment.GetEnvironmentVariable("INSTANCES") ?? "1");
        internal static Dictionary<string, TaskCompletionSource<JObject>> PendingResponses = new Dictionary<string, TaskCompletionSource<JObject>>();

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
