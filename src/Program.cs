using CliFx;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace BookmarkManager
{
    public class Program
    {
        internal static IConfiguration Configuration { get; set; }

        public static async Task<int> Main()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables().Build();
            // Commands are declared on Commands folder
            return await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
        }
    }
}
