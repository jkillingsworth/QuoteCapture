module QuoteCapture.Browser

open System
open OpenQA.Selenium

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Client() =

    let driver = new Firefox.FirefoxDriver()
    let timeout = TimeSpan.FromSeconds(30.0)

    interface IDisposable with
        member this.Dispose() =
            driver.Dispose()

    member this.Navigate(url : string) =
        driver.Navigate().GoToUrl(url)
