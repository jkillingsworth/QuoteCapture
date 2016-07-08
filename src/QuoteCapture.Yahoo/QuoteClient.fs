module QuoteCapture.Yahoo.QuoteClient

open System
open QuoteCapture
open QuoteCapture.Yahoo.Types

//-------------------------------------------------------------------------------------------------

let private url = "https://finance.yahoo.com/q"

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Client() =

    let client = new Browser.Client()
    do client.Navigate(url)

    interface IDisposable with
        member this.Dispose() =
            let disposable = client :> IDisposable
            disposable.Dispose()

    member this.GetData(issue : Issue, dateStart : DateTime, dateFinal : DateTime) =
        ()
