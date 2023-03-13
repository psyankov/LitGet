using System.CommandLine;

namespace LitGet;

internal class App
{
   internal static Config Config;

   static async Task<int> Main(string[] args)
   {
      Console.OutputEncoding = System.Text.Encoding.UTF8;
    
      var rootCmd = new RootCommand("LitGet - Download purchased books from Litres.ru");
      rootCmd.SetHandler(() => Console.WriteLine("\nRun litget --help for command line options\n"));

      var configCmd = new Command("config", "Show current configuration settings");
      configCmd.SetHandler(() => ConfigCmd());
      rootCmd.AddCommand(configCmd);

      var checkCmd = new Command("check", "Check and update the book data against the available book files");
      checkCmd.SetHandler(() => CheckCmd());
      rootCmd.AddCommand(checkCmd);

      var getBooksCmd = new Command("getbooks", "Get new (not previoulsy downloaded) books");
      getBooksCmd.SetHandler(() => GetBooksCmd());     
      rootCmd.AddCommand(getBooksCmd);

      return await rootCmd.InvokeAsync(args);
   }

   private static void ConfigCmd()
   {
      Config = Config.Instance;
      Config.LogConfiguration();
      Logger.Instance.Dispose();
   }

   private static void CheckCmd()
   {
      Config = Config.Instance;
      Config.LogConfiguration();

      var lib = new Librarian();
      lib.PersistBookData();

      Logger.Write($"Total book data record count is {lib.BookCount}");

      Logger.Instance.Dispose();
   }

   private static void GetBooksCmd()
   {
      Config = Config.Instance;
      Config.LogConfiguration();

      try
      {
         var helper = LitresHelper.Instance;

         if (helper.Connect())
         {
            helper.GetBooks();
            helper.Dispose();
            helper.PersistBookData();
         }
      }
      catch (Exception ex)
      {
         Logger.Write("\n\nUnhandled exception is raised in Main.Run(). Application is closing.");
         Logger.Write(ex.Message);
         while (ex.InnerException != null)
         {
            Logger.Write(ex.InnerException.Message);
            ex = ex.InnerException;
         }
      }
      finally
      {
         Logger.Instance.Dispose();
      }
   }
}
