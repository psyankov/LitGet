using System.ComponentModel;
using System.Configuration;

namespace LitGet
{
   internal sealed class Config
   {
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

         LoginUrl = GetSettingOrSetDefault(appSettings, nameof(LoginUrl), "");
         RecentUrl = GetSettingOrSetDefault(appSettings, nameof(RecentUrl), "");
         ArchivedUrl = GetSettingOrSetDefault(appSettings, nameof(ArchivedUrl), "");

         NewFolder = GetSettingOrSetDefault(appSettings, nameof(NewFolder), Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LitGet", "New"));
         BookFolder = GetSettingOrSetDefault(appSettings, nameof(BookFolder), Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LitGet", "Book"));
         DataFolder = GetSettingOrSetDefault(appSettings, nameof(DataFolder), Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LitGet", "Data"));

         RequestInterval = GetSettingOrSetDefault(appSettings, nameof(RequestInterval), 10000);
         WaitPageLoading = GetSettingOrSetDefault(appSettings, nameof(WaitPageLoading), 10000);

         UserName = appSettings[nameof(UserName)]?.Value ?? "";
         Password = appSettings[nameof(Password)]?.Value ?? "";

         if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
         {
            (UserName, Password) = UpdateCredentials(appSettings);
         }

         configFile.Save(ConfigurationSaveMode.Modified);
         ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);

         Directory.CreateDirectory(NewFolder);
         Directory.CreateDirectory(BookFolder);
         Directory.CreateDirectory(DataFolder);

         DisplayConfiguration();
      }

      public string LoginUrl { get; private set; }
      public string RecentUrl { get; private set; }
      public string ArchivedUrl { get; private set; }
      public string NewFolder { get; private set; }
      public string BookFolder { get; private set; }
      public string DataFolder { get; private set; }
      public string UserName { get; private set; }
      public string Password { get; private set; }
      public int RequestInterval { get; private set; }
      public int WaitPageLoading { get; private set; }

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
         return (T?) converter.ConvertFromInvariantString(valueString);
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

      private void DisplayConfiguration()
      {
         var appSettings = ConfigurationManager.AppSettings;
         Console.WriteLine("\n\nConfiguration:");
         Console.WriteLine(new string('_', 80));
         foreach(var key in appSettings.AllKeys)
         {
            Console.WriteLine("{0,-25} {1}", key, appSettings[key]);
         }
      }

      private Tuple<string, string> UpdateCredentials(KeyValueConfigurationCollection settings)
      {
         string userName = "", password = "";
         while (string.IsNullOrWhiteSpace(userName))
         {
            Console.Write("User name: ");
            userName = Console.ReadLine() ?? "";
         }
         while (string.IsNullOrWhiteSpace(password))
         {
            Console.Write("Password: ");
            password = Console.ReadLine() ?? "";
         }

         AddOrUpdateSetting(settings, nameof(UserName), userName);
         AddOrUpdateSetting(settings, nameof(Password), password);

         return new Tuple<string, string>(userName, password);
      }
   }
}
