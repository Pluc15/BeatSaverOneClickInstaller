using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BeatSaverOneClickInstaller
{
    internal class Program
    {
        private const string CustomLevelsPath =
            "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Beat Saber\\Beat Saber_Data\\CustomLevels\\";

        private static void Main(string[] args)
        {
            if (args.Length > 1)
                Console.WriteLine("Usage: TODO");
            else if (args.Length == 1)
                DownloadAndInstall(args[0]).GetAwaiter().GetResult();
            else
                RegisterScheme();
        }

        private static void RegisterScheme()
        {
            var applicationLocation = typeof(Program).Assembly.Location;
            using var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\beatsaver") ??
                            throw new ArgumentNullException("key");
            using var defaultIcon = key.CreateSubKey("DefaultIcon") ?? throw new ArgumentNullException("defaultIcon");
            using var commandKey = key.CreateSubKey(@"shell\open\command") ??
                                   throw new ArgumentNullException("commandKey");
            key.SetValue("", "URL:Beat Saver Song Map");
            key.SetValue("URL Protocol", "");
            defaultIcon.SetValue("", applicationLocation + ",1");
            commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
        }

        private static async Task DownloadAndInstall(string key)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync($"https://beatsaver.com/api/download/key/{key}");
            var fileContent = await response.Content.ReadAsByteArrayAsync();
            
            var fileName = response.RequestMessage.RequestUri.Segments.Last();
            var savePath = $"{CustomLevelsPath}{fileName}";
            var extractPath = $"{CustomLevelsPath}{fileName.Substring(0, fileName.LastIndexOf('.'))}";

            File.WriteAllBytes(savePath, fileContent);

            System.IO.Compression.ZipFile.ExtractToDirectory(savePath, extractPath);
        }
    }
}