using System.Text.Json.Serialization;

namespace LitGet;

internal class BookFile
{
   public string? Format { get; set; }
   public string? FileName { get; set; }
   public string? FilePath { get; set; }
   public string? Url { get; set; }
   public bool Downloaded { get; set; }

   [JsonIgnore]
   public FileFormat GetFormat
   {
      get
      {
         switch (Format)
         {
            default: return FileFormat.Unknown;
            case "FB2": return FileFormat.FB2;
            case "EPUB": return FileFormat.EPUB;
            case "iOS.EPUB": return FileFormat.iOS_EPUB;
            case "RTF": return FileFormat.RTF;
            case "PDF A4": return FileFormat.PDF_A4;
            case "PDF A6": return FileFormat.PDF_A6;
            case "MOBI": return FileFormat.MOBI;
            case "TXT":
               if (Url?.EndsWith(".txt.zip") ?? false)
               {
                  return FileFormat.TXT_ZIP;
               }
               else
               {
                  return FileFormat.TXT;
               }
         }
      }
   }
}
