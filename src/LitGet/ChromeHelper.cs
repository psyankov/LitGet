using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LitGet
{
   internal class ChromeHelper
   {
      private const string _ChromeDownloads = "chrome://downloads/";
      private const int _DownloadTimeout = 600000;

      private IWebDriver _Driver;
      private IJavaScriptExecutor _JSE;

      public ChromeHelper(string downloadPath)
      {
         ChromeOptions options = new ChromeOptions();
         options.AddUserProfilePreference("download.default_directory", downloadPath);

         _Driver = new ChromeDriver(options);
         _Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
         _JSE = (IJavaScriptExecutor)_Driver;
      }

      public void Quit()
      {
         _Driver.Quit();
         _Driver.Dispose();
      }

      public void GoToUrl(string url)
      {
         _Driver.Navigate().GoToUrl(url);
      }

      public string? Download(string url)
      {
         GoToUrl(url);
         return WaitDownloadCompleted(url);
      }

      public ReadOnlyCollection<IWebElement> FindElements(By by)
      {
         return FindElements(_Driver, by);
      }

      public ReadOnlyCollection<IWebElement> FindElements(ISearchContext? context, By by)
      {
         ReadOnlyCollection<IWebElement>? element = context?.FindElements(by);
         return element ?? new ReadOnlyCollection<IWebElement>(new List<IWebElement> { });
      }

      public IWebElement? TryGet(By by)
      {
         return TryGet(_Driver, by);
      }

      public IWebElement? TryGet(ISearchContext? context, By by)
      {
         IWebElement? element = null;
         try
         {
            element = context?.FindElement(by);
         }
         catch (NoSuchElementException exception) { }
         
         return element;
      }
      
      public string? TryGetAttribute(IWebElement? element, string attribute)
      {
         string? value = null;
         try
         {
            value = element?.GetAttribute(attribute);
         }
         catch (NoSuchElementException exception) { }

         return value;  
      }

      public string? TryGetAttribute(By by, string attribute)
      {
         return TryGetAttribute(_Driver, by, attribute);
      }

      public string? TryGetAttribute(ISearchContext? context, By by, string attribute)
      {
         string? value = null;
         try
         {
            value = context
               ?.FindElement(by)
               ?.GetAttribute(attribute);
         }
         catch (NoSuchElementException exception) { }

         return value;
      }

      public string? TryGet(ISearchContext context, string className, string enclosedTagName, string attribute)
      {
         string value = null;
         try
         {
            if (enclosedTagName == "")
            {
               value = context
                  .FindElement(By.ClassName(className))
                  .GetAttribute(attribute);
            }
            else
            {
               value = context
                  .FindElement(By.ClassName(className))
                  .FindElement(By.TagName(enclosedTagName))
                  .GetAttribute(attribute);
            }
         }
         catch (NoSuchElementException exception) { }

         return value;
      }

      private string? WaitDownloadCompleted(string url)
      {
         Stopwatch stopwatch = new Stopwatch();
         stopwatch.Start();
         Console.Write($"Downloading {Path.GetFileName(url)} ... ");

         string? fileName = null;
         bool inProgress = true;
         var waitBetweenChecking = 500;

         while (inProgress &&
            stopwatch.ElapsedMilliseconds < _DownloadTimeout)
         {
            Thread.Sleep(waitBetweenChecking);
            fileName = CheckChromeDownload(url, out inProgress);
         }
         stopwatch.Stop();

         if (inProgress)
         {
            Console.WriteLine("timed out.");
         }
         else
         {
            Console.WriteLine($"in {(stopwatch.ElapsedMilliseconds / 1000):F1} seconds");
         }
         return fileName;
      }

      private string? CheckChromeDownload(string url, out bool inProgress)
      {
         GoToUrl(_ChromeDownloads);

         inProgress = false;
         string? fileName = null;

         var element = TryGet(By.TagName("downloads-manager"));
         var shadowRoot = TryGetShadowRoot(element);
         element = TryGet(shadowRoot, By.Id("downloadsList"));
         var downloadItems = FindElements(element, By.TagName("downloads-item"));

         foreach (var item in downloadItems)
         {
            shadowRoot = TryGetShadowRoot(item);
            var itemHref = TryGetAttribute(shadowRoot, By.Id("file-link"), "href");
            if (itemHref == url)
            {
               fileName = TryGetAttribute(shadowRoot, By.Id("file-link"), "textContent");
               inProgress = null != TryGet(shadowRoot, By.Id("progress"));
               break;
            }
         }
         return fileName;
      }

      private ISearchContext? TryGetShadowRoot(IWebElement? element)
      {
         ISearchContext? shadowRoot = null;
         try
         {
            shadowRoot = element?.GetShadowRoot();
         }
         catch { }

         return shadowRoot;
      }
   }
}
