module QuoteCapture.Yahoo.QuoteClient

open System
open QuoteCapture
open QuoteCapture.Logging
open QuoteCapture.Yahoo.Types

//-------------------------------------------------------------------------------------------------

let private url = "http://finance.yahoo.com/q/hp"

let private paramBypass     = "bypass=true"
let private paramTicker     = "s={0}"
let private paramDateStartY = "c={0:D4}"
let private paramDateStartM = "a={0:D2}"
let private paramDateStartD = "b={0:D1}"
let private paramDateFinalY = "f={0:D4}"
let private paramDateFinalM = "d={0:D2}"
let private paramDateFinalD = "e={0:D1}"
let private xpathNext       = "//a[@rel='next']"

//-------------------------------------------------------------------------------------------------

let private constructUrl issue (dateStart : DateTime) (dateFinal : DateTime) =

    let parameters =
        [ String.Format(paramTicker, issue.Ticker)
          String.Format(paramDateStartY, dateStart.Year)
          String.Format(paramDateStartM, dateStart.Month - 1)
          String.Format(paramDateStartD, dateStart.Day)
          String.Format(paramDateFinalY, dateFinal.Year)
          String.Format(paramDateFinalM, dateFinal.Month - 1)
          String.Format(paramDateFinalD, dateFinal.Day)
          String.Format(paramBypass) ]

    parameters
    |> Seq.reduce (fun acc param -> acc + "&" + param)
    |> (+) "?"
    |> (+) url

let private appendBypassFlag url =

    url + "&" + paramBypass

//-------------------------------------------------------------------------------------------------

let private normalize ratio quote =

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

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Client() =

    let client = new Browser.Client()

    interface IDisposable with member this.Dispose() = (client :> IDisposable).Dispose()

    //---------------------------------------------------------------------------------------------

    member this.GetData(issue : Issue, dateStart : DateTime, dateFinal : DateTime) =

        this.NavigateToHistoricalQuotes(issue, dateStart, dateFinal)

        let generator = function
            | false -> None
            | _ ->
                let pageSource = client.PageSource
                let hasNextPage = QuoteParser.hasNextPage pageSource
                if (hasNextPage = true) then this.NavigateToNextPage()
                Some (pageSource, hasNextPage)

        true
        |> Array.unfold generator
        |> Array.map (QuoteParser.parseQuotes issue)
        |> Array.collect id
        |> Array.filter (fun quote -> quote.Date >= dateStart)
        |> Array.filter (fun quote -> quote.Date <= dateFinal)
        |> Array.mapFold normalize 1m |> fst
        |> Array.rev

    //---------------------------------------------------------------------------------------------

    member private this.NavigateToHistoricalQuotes(issue : Issue, dateStart : DateTime, dateFinal : DateTime) =

        Log.Debug("Navigating to historical quotes: {0}", issue)
        let ticker = issue.Ticker |> Format.Ticker.toYahoo
        let url = constructUrl issue dateStart dateFinal
        client.Navigate(url)

        Log.Debug("Waiting for page load.")
        client.WaitForPageLoad()

    member private this.NavigateToNextPage() =

        Log.Debug("Navigating to next page.")
        let elementNext = client.FindElement(xpathNext)
        let url = elementNext.Href |> appendBypassFlag
        client.Navigate(url)

        Log.Debug("Waiting for page load.")
        client.WaitForPageLoad()
