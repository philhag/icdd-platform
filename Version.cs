using System.IO;
using Microsoft.Extensions.Configuration;

namespace IcddWebApp
{
    public static class Version
    {
        public static string GetVersionLink() {
            var builder = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("version.json");
            var configuration = builder.Build();
            string commit = configuration["GitCommit"];
            string url = configuration["GitUrl"];
            return url + commit;
        }

        public static bool ShowVersion()
        {
            var builder = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("version.json");
            var configuration = builder.Build();
            string show = configuration["GitReference"];
            return show=="true";
        }
    }
}
