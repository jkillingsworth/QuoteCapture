module QuoteCapture.Yahoo.QuoteClient

open System
open QuoteCapture
open QuoteCapture.Logging
open QuoteCapture.Yahoo.Types

//-------------------------------------------------------------------------------------------------

let private url = "https://finance.yahoo.com/q"

let private xpathTicker           = "//*[@id='txtQuotes']"
let private xpathHistoricalPrices = "//a[.='Historical Prices']"
let private xpathDateM            = "//*[@id='sel{0}']/option[.='{1:MMM}']"
let private xpathDateY            = "//*[@id='{0}year']"
let private xpathDateD            = "//*[@id='{0}day']"
let private xpathGetPrices        = "//*[@value='Get Prices']"
let private xpathNext             = "//a[@rel='next']"
let private keyDateStart          = "start"
let private keyDateFinal          = "end"

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Client() =

    let client = new Browser.Client()
    do
        client.Navigate(url)

    interface IDisposable with member this.Dispose() = (client :> IDisposable).Dispose()

    //---------------------------------------------------------------------------------------------

    member this.GetData(issue : Issue, dateStart : DateTime, dateFinal : DateTime) =

        let normalize ratio quote =

            let recalculateVolume volume = int64 ((decimal volume) / ratio)
            let recalculateAmount amount = amount * ratio
            let adjustDividAmount divid = { divid with Amount = recalculateAmount divid.Amount }

            let divid = quote.Divid |> Option.map adjustDividAmount
            let quote = { quote with Divid = divid }
            let quote = { quote with Volume = recalculateVolume quote.Volume }

            let ratio =
                match quote.Split with
                | None
                    -> ratio
                | Some split
                    -> ratio * (decimal split.New / decimal split.Old)

            (quote, ratio)

        this.GetQuotes(issue, dateStart, dateFinal)
        |> Array.filter (fun quote -> quote.Date >= dateStart)
        |> Array.filter (fun quote -> quote.Date <= dateFinal)
        |> Array.mapFold normalize 1m |> fst
        |> Array.rev

    //---------------------------------------------------------------------------------------------

    member private this.GetQuotes(issue : Issue, dateStart : DateTime, dateFinal : DateTime) =

        this.NavigateToHistoricalQuoteLookup(issue)

        let quotes = client.PageSource |> QuoteParser.parseQuotes issue
        let dates = quotes |> Array.map (fun quote -> quote.Date)

        let hasDateStart = dates |> Array.contains dateStart
        let hasDateFinal = dates |> Array.contains dateFinal

        if (hasDateStart && hasDateFinal) then
            quotes
        else
            this.EnterDateStart(dateStart)
            this.EnterDateFinal(dateFinal)

            Log.Debug("Navigating to data points.")
            let elementGetQuotes = client.FindElement(xpathGetPrices)
            elementGetQuotes.Click()

            Log.Debug("Waiting for page load.")
            client.WaitForPageLoad()

            let rec loop = function
                | acc, false -> acc
                | acc, true
                    ->
                    let pageSource = client.PageSource
                    let acc = pageSource :: acc
                    if (QuoteParser.hasNextPage pageSource) then
                        this.NavigateToNextPage()
                        loop (acc, true)
                    else
                        loop (acc, false)

            loop ([], true)
            |> Seq.map (QuoteParser.parseQuotes issue)
            |> Seq.collect id
            |> Seq.toArray

    //---------------------------------------------------------------------------------------------

    member private this.NavigateToHistoricalQuoteLookup(issue : Issue) =

        Log.Debug("Navigating to historical quote lookup form: {0}", issue)

        let ticker = issue.Ticker |> Format.Ticker.toYahoo

        Log.Debug("Entering ticker symbol: {0}", ticker)
        let elementTicker = client.FindElement(xpathTicker)
        elementTicker.Click()
        elementTicker.Clear()
        elementTicker.SendKeys(ticker)
        elementTicker.PressEnter()

        Log.Debug("Waiting for page load.")
        client.WaitForPageLoad()

        Log.Debug("Clicking the historical prices link.")
        let elementHistoricalPrices = client.FindElement(xpathHistoricalPrices)
        elementHistoricalPrices.Click()

        Log.Debug("Waiting for page load.")
        client.WaitForPageLoad()

    member private this.NavigateToNextPage() =

        Log.Debug("Navigating to next page.")
        let elementNext = client.FindElement(xpathNext)
        elementNext.Click()

        Log.Debug("Waiting for page load.")
        client.WaitForPageLoad()

    //---------------------------------------------------------------------------------------------

    member private this.EnterDateStart(date : DateTime) =

        Log.Debug("Entering date (start): {0:d}", date)
        this.EnterDate(date, keyDateStart)

    member private this.EnterDateFinal(date : DateTime) =

        Log.Debug("Entering date (final): {0:d}", date)
        this.EnterDate(date, keyDateFinal)

    member private this.EnterDate(date : DateTime, key : string) =

        let elementM = client.FindElement(String.Format(xpathDateM, key, date))
        let elementY = client.FindElement(String.Format(xpathDateY, key))
        let elementD = client.FindElement(String.Format(xpathDateD, key))

        elementM.Click()
        elementY.Clear()
        elementY.SendKeys(date.Year.ToString())
        elementD.Clear()
        elementD.SendKeys(date.Day.ToString())
