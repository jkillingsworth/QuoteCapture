module Program

open QuoteCapture

//-------------------------------------------------------------------------------------------------

[<EntryPoint>]
let main argv =
    use client = new Browser.Client()
    client.Navigate("https://finance.yahoo.com/q")
    0
