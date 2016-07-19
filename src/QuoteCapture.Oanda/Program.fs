module Program

open System
open QuoteCapture.Oanda

//-------------------------------------------------------------------------------------------------

[<EntryPoint>]
let main = function

    | [| "update"; date; pair |]
        ->
        let date = DateTime.ParseExact(date, "d", null)
        let currencyNames = pair.Split('/')
        let currencyNameBase = currencyNames.[0]
        let currencyNameQuot = currencyNames.[1]
        let pair = Persistence.selectPairByCurrencyNames currencyNameBase currencyNameQuot
        Update.updateOne date pair
        Update.updateInterestRates ()
        0

    | [| "update"; date |]
        ->
        let date = DateTime.ParseExact(date, "d", null)
        Update.updateAll date
        Update.updateInterestRates ()
        0

    | [| "update" |]
        ->
        let date = Date.getMaximumDate ()
        Update.updateAll date
        Update.updateInterestRates ()
        0

    | argv
        ->
        failwith "Invalid parameters."
        1
