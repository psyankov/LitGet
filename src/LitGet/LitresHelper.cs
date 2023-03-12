using System.Text.Json;
using OpenQA.Selenium;

namespace LitGet
{
   internal sealed class LitresHelper
   {
      private const string _BookDataFileName = "BookData";
      private string _BookDataFilePath;

      private ChromeHelper? _ChromeHelper;
      private BookData _BookData;

      private Random _Random;
      private JsonSerializerOptions _JsonOptions;

      // Singleton class
      private static LitresHelper? _Instance;
      public static LitresHelper Instance
      {
         get
         {
            if (_Instance != null) return _Instance;
            else
            {
               _Instance = new LitresHelper();
               return _Instance;
            }
         }
      }

      private LitresHelper()
      {
         _Random = new Random((int)DateTime.Now.ToBinary());        
         _JsonOptions = new JsonSerializerOptions() { WriteIndented = true };

         _BookDataFilePath = Path.Combine(App.Config.DataFolder, $"{_BookDataFileName}.json");
         if (File.Exists(_BookDataFilePath))
         {
            _BookData = JsonSerializer.Deserialize<BookData>(File.ReadAllText(_BookDataFilePath)) ?? new BookData();
         }        
         else
         {
            _BookData = new BookData();
         }
      }

      public bool Connected { get; private set; }
      public bool ExecutingGetBooks { get; private set; }

      public bool Connect()
      {
         ExecutingGetBooks = false;

         _ChromeHelper = new ChromeHelper(App.Config.AudioFolder);

         if (!Login(App.Config.User.Name, App.Config.User.Password))
         {
            Logger.Write("\n\n ********* Login failed *********");
            _ChromeHelper.Quit();
            return false;
         }
         else
         {
            Connected = true;
            return true;
         }
      }

      public void Disconnect()
      {
         _ChromeHelper?.Quit();
         Connected = false;
      }

      public void PersistBookData()
      {
         if (File.Exists(_BookDataFilePath))
         {
            var modified = File.GetLastWriteTime(_BookDataFilePath);
            File.Move(_BookDataFilePath, Path.Combine(App.Config.DataFolder, $"{_BookDataFileName}.{modified:yyyy.MM.dd.HH.mm.ss}.json"));
         }
         File.WriteAllText(_BookDataFilePath, JsonSerializer.Serialize(_BookData, _JsonOptions));
      }

      internal void GetBooks()
      {
         // Only one instance of this method should run at any given time!
         if (ExecutingGetBooks) return;
         ExecutingGetBooks = true;

         try
         {
            UpdateBookData();
         }
         catch (Exception exception)
         {
            Logger.Write("\n\n ********* Something went wrong while updating book data... ********* ");
            Logger.Write(exception.Message);
         }
         var downloadableCount = _BookData.Books.FindAll(b => b.Type == BookType.Text && b.Files.Count > 0).Count;
         Logger.Write($"\n\n======== Total number of updated book records: {_BookData.Books.Count}");        
         Logger.Write($"======== Number of downlodable books:          {downloadableCount}");

         try
         {
            DownloadBooks();
         }
         catch (Exception exception)
         {
            Logger.Write("\n\n ********* Something went wrong while downloading book files... ********* ");
            Logger.Write(exception.Message);
         }

         ExecutingGetBooks = false;
      }

      private bool Login(string user, string password)
      {
         if (_ChromeHelper is null) throw new InvalidOperationException("ChromeHelper is null");

         _ChromeHelper.GoToUrl(App.Config.LoginUrl);

         var userBox = _ChromeHelper.TryGet(By.Name("login"));
         var pwdBox = _ChromeHelper.TryGet(By.Id("open_pwd_main"));
         var loginBtn = _ChromeHelper.TryGet(By.Id("login_btn"));

         if (userBox is null || pwdBox is null || loginBtn is null) return false;

         userBox.SendKeys(user);
         RandomSleep((int) 0.5 * App.Config.RequestInterval);

         pwdBox.SendKeys(password);
         RandomSleep(2000);

         loginBtn.Click();
         Sleep(5000);

         var profileWrapper = _ChromeHelper.TryGet(By.ClassName("Profile-module__wrapper_1aXs2"));

         return profileWrapper is null ? false : true;
      }

      private void UpdateBookData()
      {
         if (_ChromeHelper is null) throw new InvalidOperationException("ChromeHelper is null");

         Logger.Write("\n\n ********* Updating book records ********* ");
         _ChromeHelper.GoToUrl(App.Config.RemovedUrl);
         Sleep(App.Config.WaitPageLoading);

         //var pageCountString = _ChromeHelper.TryGetAttribute(By.ClassName("books_container mgrid_wrapper_loader_container"), "data-pages") ?? "Not Found";
         //var bookCountString = _ChromeHelper.TryGetAttribute(By.ClassName("books_container mgrid_wrapper_loader_container"), "data-arts") ?? "Not Found";

         //int pageCount;
         //int bookCount;
         
         //try
         //{
         //   pageCount = int.Parse(pageCountString);
         //}
         //catch
         //{
         //   Logger.Write($"Could not parse the number of pages: '{pageCountString}'.");
         //   pageCount = 1;
         //}
         
         //try
         //{
         //   bookCount = int.Parse(bookCountString);
         //}
         //catch
         //{
         //   Logger.Write($"Could not parse the number of books: '{bookCountString}'.");
         //   bookCount = 0;
         //}
         //Logger.Write($"Expecting {bookCount} books on {pageCount} pages.\n\n");

         for (int i = 1; i <= 2; i++)
         {
            _ChromeHelper.GoToUrl($"{App.Config.RecentUrl}/page-{i}/");
            Sleep(App.Config.RequestInterval);
            GetBookDataFromPage();
         }

         Logger.Write("\n\n ********* Finished updating book data ********* ");
      }

      private void GetBookDataFromPage()
      {
         if (_ChromeHelper is null) throw new InvalidOperationException("ChromeHelper is null");

         var artItems = _ChromeHelper.FindElements(By.ClassName("art-item"));

         foreach (var item in artItems)
         {
            var book = new Book();

            book.Id = _ChromeHelper.TryGetAttribute(
               _ChromeHelper.TryGet(item, By.ClassName("cover-image-wrapper")),
               By.TagName("a"), "data-art") ?? "";
            book.TypeName = _ChromeHelper.TryGetAttribute(
               _ChromeHelper.TryGet(item, By.ClassName("cover-image-wrapper")),
               By.TagName("a"), "data-type") ?? "";
            book.Cover = _ChromeHelper.TryGetAttribute(
               _ChromeHelper.TryGet(item, By.ClassName("cover-image-wrapper")),
               By.TagName("img"), "src") ?? "";
            book.Author = _ChromeHelper.TryGetAttribute(
               _ChromeHelper.TryGet(item, By.ClassName("art__author")),
               By.TagName("a"), "title") ?? "";
            book.AuthorUrl = _ChromeHelper.TryGetAttribute(
               _ChromeHelper.TryGet(item, By.ClassName("art__author")),
               By.TagName("a"), "href") ?? "";
            book.Title = _ChromeHelper.TryGetAttribute(
               item,
               By.ClassName("art__name__href"), "title") ?? "";
            book.TitleUrl = _ChromeHelper.TryGetAttribute(
               item,
               By.ClassName("art__name__href"), "href") ?? "";

            var links = _ChromeHelper.FindElements(
               _ChromeHelper.TryGet(item, By.ClassName("art-buttons__download")),
               By.ClassName("art-download__format"));

            foreach (var link in links)
            {
               var bookFile = new BookFile();
               bookFile.FormatName = _ChromeHelper.TryGetAttribute(link, "textContent") ?? string.Empty;
               bookFile.Url = _ChromeHelper.TryGetAttribute(link, "href") ?? string.Empty;
               book.Files.Add(bookFile);
            }

            _BookData.AddOrUpdateBook(book);
            Logger.Write(book.ToString());
         }
      }

      private void DownloadBooks()
      {
         if (_ChromeHelper is null) throw new InvalidOperationException("ChromeHelper is null");

         Logger.Write("\n\n ********* Starting to download books ********* \n\n");

         int downloadCount = 0;
         int failedCount = 0;
         int exceptionCount = 0;

         foreach (var book in _BookData.Books
            .FindAll(b => b.Type == BookType.Text && b.Files.Count > 0))
         {
            foreach (var format in new List<BookFormat>
               {
                  BookFormat.EPUB,
                  BookFormat.FB2,
                  BookFormat.MOBI,
                  BookFormat.PDF_A4,
                  BookFormat.PDF_A6,
                  BookFormat.TXT_ZIP,
               })
            {
               var file = book.Files.Find(f => f.Format == format);
               if (file == null) continue;
               if (file.Downloaded) continue;

               try
               {
                  RandomSleep(App.Config.RequestInterval);
                  file.FileName = _ChromeHelper.Download(file.Url) ?? "FAILED";
                  file.Downloaded = file.FileName == "FAILED" ? false : true;
                  if (file.Downloaded)
                  {
                     downloadCount++;
                  }
                  else
                  {
                     failedCount++;
                  }
               }
               catch (Exception exception)
               {
                  file.Downloaded = false;
                  exceptionCount++;
                  Logger.Write($"\n\n ********* Something went wrong while downloading '{book.Id} {book.Title}' *********");
                  Logger.Write(exception.Message);
               }
            }

            Logger.Write(JsonSerializer.Serialize(book, _JsonOptions));

            if (Console.KeyAvailable)
            {
               var key = Console.ReadKey();
               if (key.KeyChar == 'q')
               {
                  Logger.Write("\n\n Quitting on user request...");
                  break;
               }
            }
         }

         Logger.Write($"\n\nDownloaded {downloadCount} book files.\n\n");
         Logger.Write($"\n\nFailed {failedCount}.\n\n");
         Logger.Write($"\n\nFailed {exceptionCount}.\n\n");
      }

      private void RandomSleep(int maxSleepMilliseconds)
      {
         Sleep((int)(0.2 * maxSleepMilliseconds + 0.8 * maxSleepMilliseconds * _Random.NextDouble()));
      }

      private void Sleep(int milliseconds)
      {
         Thread.Sleep(milliseconds);
      }
   }
}
