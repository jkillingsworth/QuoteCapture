module QuoteCapture.Browser

open System
open OpenQA.Selenium
open OpenQA.Selenium.Support.UI
open QuoteCapture.Logging

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Element(element : IWebElement) =

    do
        Log.Debug("Create new element: {0}", element.TagName)

    //---------------------------------------------------------------------------------------------

    member this.IsDisplayed
        with get () = element.Displayed

    member this.IsSelected
        with get () = element.Selected

    //---------------------------------------------------------------------------------------------

    member this.SendKeys(text : string) =

        Log.Debug("Sending keys: {0}", text)
        element.SendKeys(text)

    member this.PressEnter() =

        Log.Debug("Pressing enter.")
        element.SendKeys(Keys.Enter)

    member this.PressTab() =

        Log.Debug("Pressing tab.")
        element.SendKeys(Keys.Tab)

    member this.Clear() =

        Log.Debug("Clearing element.")
        element.Clear()

    member this.Click() =

        Log.Debug("Clicking element.")
        element.Click()

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Client() =

    let driver = new Firefox.FirefoxDriver()
    let timeout = TimeSpan.FromSeconds(30.0)

    interface IDisposable with member this.Dispose() = driver.Dispose()

    //---------------------------------------------------------------------------------------------

    member this.Navigate(url : string) =

        Log.Debug("Navigating to URL.")
        Log.Extra(url)
        driver.Navigate().GoToUrl(url)

    member this.GoForward() =

        Log.Debug("Going forward.")
        driver.Navigate().Forward()

    member this.GoBack() =

        Log.Debug("Going back.")
        driver.Navigate().Back()

    member this.Refresh() =

        Log.Debug("Refreshing page.")
        driver.Navigate().Refresh()

    member this.WaitForPageLoad() =

        Log.Debug("Waiting for page load...")
        let condition = Func<_, bool>(Client.IsReadyStateComplete)
        let wait = new WebDriverWait(driver, timeout)
        wait.Until(condition) |> ignore
        Log.Debug("Page loaded.")

    //---------------------------------------------------------------------------------------------

    member this.FindElements(xpath : string) =

        Log.Debug("Finding elements.")
        Log.Extra(xpath)
        driver.FindElements(By.XPath(xpath))
        |> Seq.map Element
        |> Seq.toArray

    member this.FindElement(xpath : string) =

        Log.Debug("Finding element.")
        Log.Extra(xpath)
        driver.FindElement(By.XPath(xpath))
        |> Element

    //---------------------------------------------------------------------------------------------

    static member private IsReadyStateComplete(driver : IWebDriver) =

        let executor = driver :?> IJavaScriptExecutor
        let script = "if (document.readyState) return document.readyState;"
        let readyState = executor.ExecuteScript(script)
        let readyState = readyState :?> string
        Log.Debug("Document ready state: {0}", readyState)
        readyState = "complete"
