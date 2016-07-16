module QuoteCapture.Yahoo.Ticker

//-------------------------------------------------------------------------------------------------

let ofYahoo (ticker : string) = ticker.Replace("-", ".")
let toYahoo (ticker : string) = ticker.Replace(".", "-")
