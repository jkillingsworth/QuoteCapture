module QuoteCapture.Oanda.QuoteClient

open System
open QuoteCapture
open QuoteCapture.Logging
open QuoteCapture.Browser
open QuoteCapture.Oanda.Types

//-------------------------------------------------------------------------------------------------

let private url = "https://www.oanda.com/currency/converter/"

let private prefixBase     = "quote"
let private prefixQuot     = "base"
let private xpathCodeField = "//*[@id='{0}_currency_input']";
let private xpathCodeValue = "//*[@id='{0}_currency_list_container']//span[.='{1}']";
let private xpathTrackBase = "//*[@id='scroll-track-bot-1']";
let private xpathTrackQuot = "//*[@id='scroll-track-bot-2']";
let private xpathDateInput = "//*[@id='end_date_input']";

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Client() =

    let client = new Browser.Client()
    do
        client.Navigate(url)

    interface IDisposable with member this.Dispose() = (client :> IDisposable).Dispose()

    //---------------------------------------------------------------------------------------------

    member this.GetData(pair : Pair, dateStart : DateTime, dateFinal : DateTime) =

        Log.Debug("Setting the base currency: {0}.", pair.Base)
        this.FindElement(xpathCodeField, prefixBase).Click()
        let elementValueBase = this.FindElement(xpathCodeValue, prefixBase, pair.Base.Name)
        let elementTrackBase = this.FindElement(xpathTrackBase)
        this.ScrollUntilDisplayed(elementTrackBase, elementValueBase)
        elementValueBase.Click()

        Log.Debug("Setting the quot currency: {0}.", pair.Quot)
        this.FindElement(xpathCodeField, prefixQuot).Click()
        let elementValueQuot = this.FindElement(xpathCodeValue, prefixQuot, pair.Quot.Name)
        let elementTrackQuot = this.FindElement(xpathTrackQuot)
        this.ScrollUntilDisplayed(elementTrackQuot, elementValueQuot)
        elementValueQuot.Click()

        let dateStart = dateStart.AddDays(+1.0)
        let dateFinal = dateFinal.AddDays(+1.0)
        let increment = if (dateStart <= dateFinal) then +1 else -1

        let generator = function
        | date when dateFinal.AddDays(float increment) = date -> None
        | date -> (date, date.AddDays(float increment)) |> Some

        let dates = dateStart |> Seq.unfold generator
        dates
        |> Seq.map this.NavigateToQuote
        |> Seq.map2 (QuoteParser.parseQuote pair) dates
        |> Seq.map (fun quote -> { quote with Date = quote.Date.AddDays(-1.0) })

    //---------------------------------------------------------------------------------------------

    member private this.NavigateToQuote(date : DateTime) =

        Log.Debug("Setting the date: {0:d}.", date)
        let element = client.FindElement(xpathDateInput)
        element.Click()
        element.Clear()
        element.SendKeys(date.ToString("M'/'d'/'yyyy"))
        element.PressEnter()
        element.PressTab()

        Log.Debug("Refreshing current page.")
        client.Refresh()

        client.PageSource

    //---------------------------------------------------------------------------------------------

    member private this.FindElement(xpathTemplate : string, [<ParamArray>] args : obj[]) : Element =

        let xpath = String.Format(xpathTemplate, args)
        client.FindElement(xpath)

    member private this.ScrollUntilDisplayed(elementToClick : Element, elementToDisplay : Element) =

        while (not elementToDisplay.IsDisplayed) do
            Log.Debug("Element not displayed. Continuing to scroll down.")
            elementToClick.Click()
