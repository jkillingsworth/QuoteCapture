module Program

open System
open QuoteCapture.Yahoo

//-------------------------------------------------------------------------------------------------

[<EntryPoint>]
let main = function

    | [| "update"; date; ticker |]
        ->
        let date = DateTime.ParseExact(date, "d", null)
        let issue = Persistence.selectIssueByTicker ticker
        Update.updateOne date issue
        0

    | [| "update"; date |]
        ->
        let date = DateTime.ParseExact(date, "d", null)
        Update.updateAll date
        0

    | [| "update" |]
        ->
        let date = Date.getMaximumDate ()
        Update.updateAll date
        0

    | [| "issueId"; input |]
        ->
        let issueId = Ticker.computeIssueId input
        printfn "%i" issueId
        0

    | argv
        ->
        failwith "Invalid parameters."
        1
