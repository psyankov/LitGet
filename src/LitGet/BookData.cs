namespace LitGet;

internal class BookData
{
   public BookData()
   {
      Books = new List<Book>();
   }
   public List<Book> Books { get; set; }

   public void AddOrUpdateBook(Book book)
   {
      Books.Add(book);
   }
}
