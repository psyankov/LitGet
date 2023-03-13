using System.Text.Json.Serialization;

namespace LitGet;

internal class Book
{
   public Book()
   {
      Files = new List<BookFile>();
   }

   public string? Id { get; set; }
   public string? Type { get; set; }
   public string? CoverUrl { get; set; }
   public string? Author { get; set; }
   public string? AuthorUrl { get; set; }
   public string? Title { get; set; }
   public string? TitleUrl { get; set; }
   public List<BookFile> Files { get; set; }

   [JsonIgnore]
   public BookType EType
   {
      get
      {
         switch (Type)
         {
            case "Audio": return BookType.Audio;
            case "Ebook": return BookType.Ebook;
            default: return BookType.Unknown;
         }
      }
   }

   public void SetType(string type)
   {
      switch (type)
      {
         case "audiokniga":
            Type = "Audio";
            break;
         case "elektronnaya-kniga":
            Type = "EBook";
            break;
         default:
            Type = type;
            break;
      }
   }

   public override string ToString()
   {
      return
         "_________________________________________________________________________________________\n"
         + $"ID                 {Id}\n"
         + $"Type               {EType}\n"
         + $"Author             {Author}\n"
         + $"Title              {Title}\n";
   }
}
