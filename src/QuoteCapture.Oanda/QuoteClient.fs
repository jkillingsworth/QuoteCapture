module QuoteCapture.Oanda.QuoteClient

open System
open QuoteCapture
open QuoteCapture.Oanda.Types

//-------------------------------------------------------------------------------------------------

let private url = "https://www.oanda.com/currency/converter/"

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Client() =

    let client = new Browser.Client()
    do client.Navigate(url)

    interface IDisposable with
        member this.Dispose() =
            let disposable = client :> IDisposable
            disposable.Dispose()

    member this.GetData(pair : Pair, dateStart : DateTime, dateFinal : DateTime) =
        ()
