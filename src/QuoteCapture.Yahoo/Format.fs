module QuoteCapture.Yahoo.Format

//-------------------------------------------------------------------------------------------------

module Ticker =

    let ofYahoo (ticker : string) =
        ticker.Replace("-", ".")

    let toYahoo (ticker : string) =
        ticker.Replace(".", "-")
