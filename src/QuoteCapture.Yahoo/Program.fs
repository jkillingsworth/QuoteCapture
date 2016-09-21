module Program

open System
open QuoteCapture.Logging
open QuoteCapture.Yahoo

//-------------------------------------------------------------------------------------------------

let private execute = function

    | [| "update"; date; ticker |]
        ->
        let date = DateTime.ParseExact(date, "d", null)
        let issue = Persistence.selectIssueByTicker ticker
        Update.updateQuotesOne date issue

    | [| "update"; date |]
        ->
        let date = DateTime.ParseExact(date, "d", null)
        Update.updateQuotesAll date

    | [| "update" |]
        ->
        let date = Date.getMaximumDate ()
        Update.updateQuotesAll date

    | [| "issueId"; input |]
        ->
        let issueId = Ticker.computeIssueId input
        printfn "%i" issueId

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
