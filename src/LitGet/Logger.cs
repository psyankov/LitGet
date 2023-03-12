namespace LitGet
{
   internal class Logger : IDisposable
   {
      private string _LogFilePath;
      private StreamWriter _LogWriter;

      // Singleton class
      private static Logger? _Instance;
      public static Logger Instance
      {
         get
         {
            if (_Instance != null) return _Instance;
            else
            {
               _Instance = new Logger();
               return _Instance;
            }
         }
      }

      private Logger()
      {
         _LogFilePath = Path.Combine(Config.Instance.DataFolder, $"Log.{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt");
         _LogWriter = new StreamWriter(_LogFilePath);
      }

      public static void Write(string message)
      {
         Instance.WriteLine(message);
      }

      private void WriteLine(string message)
      {
         _LogWriter.WriteLine(message);
         Console.WriteLine(message);
      }

      public void Dispose()
      {
         _LogWriter.Flush();
         _LogWriter.Dispose();
      }
   }
}
