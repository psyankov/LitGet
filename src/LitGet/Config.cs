using System.ComponentModel;
using System.Configuration;
using System.Text.Json;

namespace LitGet
{
   internal sealed class Config
   {
      private const string _LoginUrl = "https://www.litres.ru/pages/login/";
      private const string _RecentUrl = "https://www.litres.ru/pages/my_books_fresh/";
      private const string _RemovedUrl = "https://www.litres.ru/pages/my_books_removed/";

      private const string _RootFolder = "LitGet";
      private const string _AudioFolder = "Audio";
      private const string _BooksFolder = "Books";
      private const string _DataFolder = "Data";
      private const string _UserCredentialsFile = "User.json";

      private const int _RequestIntervalMilliseconds = 10000;
      private const int _WaitPageLoadingMilliseconds = 10000;

      // Singleton
      private static Config? _Instance;
      public static Config Instance
      {
         get
         {
            if (_Instance != null) return _Instance;
            else
            {
               _Instance = new Config();
               return _Instance;
            }
         }
      }

      private Config()
      {
         var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
         var appSettings = configFile.AppSettings.Settings;

         User = new User();

         LoginUrl = GetSettingOrSetDefault(appSettings, nameof(LoginUrl), _LoginUrl);
         RecentUrl = GetSettingOrSetDefault(appSettings, nameof(RecentUrl), _RecentUrl);
         RemovedUrl = GetSettingOrSetDefault(appSettings, nameof(RemovedUrl), _RemovedUrl);

         AudioFolder = GetSettingOrSetDefault(appSettings, nameof(AudioFolder), Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _RootFolder, _AudioFolder));
         BooksFolder = GetSettingOrSetDefault(appSettings, nameof(BooksFolder), Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _RootFolder, _BooksFolder));
         DataFolder = GetSettingOrSetDefault(appSettings, nameof(DataFolder), Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _RootFolder, _DataFolder));

         RequestInterval = GetSettingOrSetDefault(appSettings, nameof(RequestInterval), _RequestIntervalMilliseconds);
         WaitPageLoading = GetSettingOrSetDefault(appSettings, nameof(WaitPageLoading), _WaitPageLoadingMilliseconds);

         configFile.Save(ConfigurationSaveMode.Modified);
         ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

         Directory.CreateDirectory(AudioFolder);
         Directory.CreateDirectory(BooksFolder);
         Directory.CreateDirectory(DataFolder);

         GetOrSetUserCredentials();
      }

      public string LoginUrl { get; private set; }
      public string RecentUrl { get; private set; }
      public string RemovedUrl { get; private set; }
      public string AudioFolder { get; private set; }
      public string BooksFolder { get; private set; }
      public string DataFolder { get; private set; }
      public int RequestInterval { get; private set; }
      public int WaitPageLoading { get; private set; }
      public User User { get; private set; }

      public void LogConfiguration()
      {
         var settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).AppSettings.Settings;

         Logger.Write(new string('_', 80));
         Logger.Write("Configuration:\n");
         foreach (var key in settings.AllKeys)
         {
            Logger.Write("{0,-25} {1}", key, settings[key].Value);
         }
         Logger.Write($"\nUsing site credentials for user: {User.Name}");
         Logger.Write(new string('_', 80));
      }

      private T GetSettingOrSetDefault<T>(KeyValueConfigurationCollection settings, string key, T defaultValue)
      {
         T value;
         var valueString = settings[key]?.Value ?? "";

         if (string.IsNullOrWhiteSpace(valueString))
         {
            value = defaultValue;
         }
         else
         {
            try
            {
               value = ParseFromString<T>(valueString) ?? defaultValue;
            }
            catch
            {
               value = defaultValue;
            }
         }

         valueString = value?.ToString() ?? "";

         AddOrUpdateSetting(settings, key, valueString);

         return value;
      }

      public static T? ParseFromString<T>(string valueString)
      {
         var converter = TypeDescriptor.GetConverter(typeof(T));
         return (T?)converter.ConvertFromInvariantString(valueString);
      }

      private void AddOrUpdateSetting(KeyValueConfigurationCollection settings, string key, string value)
      {
         if (settings[key] == null)
         {
            settings.Add(key, value);
         }
         else
         {
            settings[key].Value = value;
         }
      }

      private void GetOrSetUserCredentials()
      {
         var userFilePath = Path.Combine(DataFolder, _UserCredentialsFile);

         try
         {
            User = JsonSerializer.Deserialize<User>(File.ReadAllText(userFilePath)) ?? new User();
         }
         catch
         {
            User = new User();
         }

         var updatePassword = false;
         while (string.IsNullOrWhiteSpace(User.Name))
         {
            Console.Write("User name: ");
            User.Name = Console.ReadLine() ?? "";
            updatePassword = true;
         }
         while (updatePassword || string.IsNullOrWhiteSpace(User.Password))
         {
            Console.Write("Password: ");
            User.Password = Console.ReadLine() ?? "";
            updatePassword = false;
         }
         
         User.Name = User.Name;
         User.Password = User.Password;

         File.WriteAllText(userFilePath, JsonSerializer.Serialize(User, new JsonSerializerOptions() { WriteIndented = true }));
      }
   }
}
