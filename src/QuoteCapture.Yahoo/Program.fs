module Program

open QuoteCapture.Yahoo

//-------------------------------------------------------------------------------------------------

[<EntryPoint>]
let main argv =
    use client = new QuoteClient.Client()
    0
