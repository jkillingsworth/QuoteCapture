module Program

open System
open QuoteCapture.Logging
open QuoteCapture.Oanda

//-------------------------------------------------------------------------------------------------

let private execute = function

    | [| "update"; date; pair |]
        ->
        let date = DateTime.ParseExact(date, "d", null)
        let currencyNames = pair.Split('/')
        let currencyNameBase = currencyNames.[0]
        let currencyNameQuot = currencyNames.[1]
        let pair = Persistence.selectPairByCurrencyNames currencyNameBase currencyNameQuot
        Update.updateQuotesOne date pair
        Update.updateInterestRates ()

    | [| "update"; date |]
        ->
        let date = DateTime.ParseExact(date, "d", null)
        Update.updateQuotesAll date
        Update.updateInterestRates ()

    | [| "update" |]
        ->
        let date = Date.getMaximumDate ()
        Update.updateQuotesAll date
        Update.updateInterestRates ()

    | argv
        ->
        failwith "Invalid parameters."

[<EntryPoint>]
let main argv =
    try
        execute argv
    with ex ->
        Log.Fatal(ex)
    0
