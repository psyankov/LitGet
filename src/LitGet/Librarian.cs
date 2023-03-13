using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LitGet
{
   internal class Librarian
   {
      private const string _DataFileName = "BookData";

      private string _DataFilePath;
      private string _DataIndentedFilePath;
      private JsonSerializerOptions _JsonOptions;
      private JsonSerializerOptions _JsonOptionsIndented;

      private BookData _Data;

      public Librarian()
      {
         _JsonOptions = new JsonSerializerOptions() { WriteIndented = false };
         _JsonOptionsIndented = new JsonSerializerOptions() { WriteIndented = true };
         _DataFilePath = Path.Combine(App.Config.DataFolder, $"{_DataFileName}.json");
         _DataIndentedFilePath = Path.Combine(App.Config.DataFolder, $"{_DataFileName}.json");

         try
         {
            _Data = JsonSerializer.Deserialize<BookData>(File.ReadAllText(_DataFilePath)) ?? new BookData();
         }
         catch
         {
            _Data = new BookData();
         }
      }

      public int BookCount => _Data.Books.Count;

      public void PersistBookData()
      {
         if (File.Exists(_DataFilePath))
         {
            var modified = File.GetLastWriteTime(_DataFilePath);
            File.Move(_DataFilePath, Path.Combine(App.Config.DataFolder, $"{_DataFileName}.{modified:yyyy.MM.dd.HH.mm.ss}.json"));
         }
         File.WriteAllText(_DataFilePath, JsonSerializer.Serialize(_Data, _JsonOptions));

         var indentedFileName = Path.Combine(App.Config.DataFolder, $"{_DataFileName}Indented.json");
         File.WriteAllText(indentedFileName, JsonSerializer.Serialize(_Data, _JsonOptionsIndented));
      }
   }
}
