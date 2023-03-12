namespace LitGet
{
   internal class App
   {
      internal static Config Config;

      static void Main(string[] args)
      {
         Console.OutputEncoding = System.Text.Encoding.UTF8;
         Config = Config.Instance;

         Run();

         Logger.Instance.Dispose();
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
