namespace LitGet
{
   internal class App
   {
      internal static Config Config;

      static void Main(string[] args)
      {
         try
         {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Config = Config.Instance;
            Config.LogConfiguration();

            //Run();
         }
         catch(Exception ex)
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

      static void Run()
      {
         var helper = LitresHelper.Instance;

         if (helper.Connect())
         {
            helper.GetBooks();
            helper.Disconnect();
            helper.PersistBookData();
         }
      }
   }
}
