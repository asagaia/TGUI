using Newtonsoft.Json;
using System;

namespace TGUI.Config
{
    internal static class Config
    {
        public class ExternalConfig
        {
            public string TelegramBotId { get; set; }
            public string DefaultUrlToPicture { get; set; }
        }

        private static FilePickerFileType GetFilePickerFileType()
        {
            var fileTypes = new Dictionary<DevicePlatform, IEnumerable<string>>(4)
                {
                    {
                        DevicePlatform.WinUI, [".txt", ".csv"]
                    },
                    {
                        DevicePlatform.macOS, [".txt"]
                    }
                };

            return new FilePickerFileType(fileTypes);
        }

        public static ExternalConfig UserConfig { get; }

        public static string PathToAppFolder { get; }
        public static FilePickerFileType UserListFileType => GetFilePickerFileType();

        static Config()
        {
            PathToAppFolder = AppContext.BaseDirectory;

            var serializer = new JsonSerializer();

            using (var stream = File.Open(Path.Combine(PathToAppFolder, "Config/Config.json"), FileMode.Open))
            using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                UserConfig = serializer.Deserialize<ExternalConfig>(jsonReader) ?? throw new Exception("Error 001 - Cannot read the configuration file.");
            }
        }
    }
}
