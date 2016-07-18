module QuoteCapture.Oanda.InterestRateClient

open System
open QuoteCapture
open QuoteCapture.Logging
open QuoteCapture.Browser
open QuoteCapture.Oanda.Types

//-------------------------------------------------------------------------------------------------

let private url = "https://fx1.oanda.com/user/interestrate.html"

let private xpathCurrency = "//*[@name='currency']/option"
let private xpathDateMin  = "//*[@name='startdate']"
let private xpathDateMax  = "//*[@name='enddate']"
let private xpathSubmit   = "//*[@name='submit']"

let private dateMin = DateTime(2001, 01, 01)
let private dateMax = DateTime.Now.AddYears(+1)

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Client() =

    let client = new Browser.Client()
    do
        client.Navigate(url)

    interface IDisposable with member this.Dispose() = (client :> IDisposable).Dispose()

    //---------------------------------------------------------------------------------------------

    member this.GetData() =

        Log.Debug("Selecting all available currencies.")
        let elementsCurrency = client.FindElements(xpathCurrency)
        for elementCurrency in elementsCurrency do
            if (elementCurrency.IsSelected = false) then
                elementCurrency.Click()

        Log.Debug("Setting the date minimum: {0:d}", dateMin)
        let elementDateMin = client.FindElement(xpathDateMin)
        elementDateMin.Clear()
        elementDateMin.SendKeys(dateMin.ToString("M'/'d'/'yyyy"))

        Log.Debug("Setting the date maximum: {0:d}", dateMax)
        let elementDateMax = client.FindElement(xpathDateMax)
        elementDateMax.Clear()
        elementDateMax.SendKeys(dateMax.ToString("M'/'d'/'yyyy"))

        Log.Debug("Navigating to interest rate list.")
        let elementSubmit = client.FindElement(xpathSubmit)
        elementSubmit.Click()

        Log.Debug("Waiting for page load.")
        client.WaitForPageLoad()

        let interestRates = client.PageSource |> InterestRateParser.parseInterestRates

        Log.Debug("Navigating back to interest rate form.")
        client.GoBack()

        interestRates
