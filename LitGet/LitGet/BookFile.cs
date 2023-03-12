namespace LitGet
{
   internal class BookFile
   {
      public string? FormatName { get; set; }
      public BookFormat Format { get => GetBookFormat(FormatName, Url); }
      public string? FileName { get; set; }
      public string? Url { get; set; }
      public bool Downloaded { get; set; }

      public BookFormat GetBookFormat(string? format, string? url)
      {
         switch (format)
         {
            default: return BookFormat.Unknown;
            case "FB2": return BookFormat.FB2;
            case "EPUB": return BookFormat.EPUB;
            case "iOS.EPUB": return BookFormat.iOS_EPUB;
            case "RTF": return BookFormat.RTF;
            case "PDF A4": return BookFormat.PDF_A4;
            case "PDF A6": return BookFormat.PDF_A6;
            case "MOBI": return BookFormat.MOBI;
            case "TXT":
               if (url?.EndsWith(".txt.zip") ?? false)
               {
                  return BookFormat.TXT_ZIP;
               }
               else if(url?.EndsWith(".txt") ?? false)
               {
                  return BookFormat.TXT;
               }
               else
               {
                  return BookFormat.Unknown;
               }
         }
      }
   }
}
