module Program

open QuoteCapture

//-------------------------------------------------------------------------------------------------

[<EntryPoint>]
let main argv =
    use client = new Browser.Client()
    client.Navigate("https://www.oanda.com/currency/converter/")
    0
