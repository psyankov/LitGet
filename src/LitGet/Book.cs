namespace LitGet
{
   internal enum BookType
   {
      Audio,
      Text,
      Unknown
   }

   internal class Book
   {
      public Book()
      {
         Files = new List<BookFile>();
      }

      public string? Id { get; set; }
      public string? Cover { get; set; }
      public BookType Type { get => GetType(TypeName); }
      public string? TypeName { get; set; }
      public string? Author { get; set; }
      public string? AuthorUrl { get; set; }
      public string? Title { get; set; }
      public string? TitleUrl { get; set; }
      public List<BookFile> Files { get; set; }

      public override string ToString()
      {
         return
            "_________________________________________________________________________________________\n"
            + $"ID                 {Id}\n"
            + $"Type               {Type}\n"
            + $"Author             {Author}\n"
            + $"Title              {Title}\n"
            + $"Downloadable       {Files.Count > 0}\n";
      }

      private BookType GetType(string? type)
      {
         switch (type)
         {
            default: return BookType.Unknown;
            case "elektronnaya-kniga": return BookType.Text;
            case "audiokniga": return BookType.Audio;
         }
      }
   }
}
