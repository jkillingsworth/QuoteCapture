module Program

open QuoteCapture.Oanda

//-------------------------------------------------------------------------------------------------

[<EntryPoint>]
let main argv =
    use client = new QuoteClient.Client()
    0
