module QuoteCapture.Oanda.InterestRateClient

open System
open QuoteCapture
open QuoteCapture.Oanda.Types

//-------------------------------------------------------------------------------------------------

let private url = "https://fx1.oanda.com/user/interestrate.html"

//-------------------------------------------------------------------------------------------------

[<Sealed>]
type Client() =

    let client = new Browser.Client()
    do client.Navigate(url)

    interface IDisposable with
        member this.Dispose() =
            let disposable = client :> IDisposable
            disposable.Dispose()

    member this.GetData() =
        ()
