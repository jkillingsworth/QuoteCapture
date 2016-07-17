module QuoteCapture.Yahoo.Ticker

open System
open System.Text.RegularExpressions

//-------------------------------------------------------------------------------------------------

let private validateMatchSuccess (match' : Match) =

    if (not match'.Success) then
        failwith "Input string did not match the expected pattern."

//-------------------------------------------------------------------------------------------------

let computeIssueId (input : string) =

    let pattern = "^(?<ticker>[\\.A-Z]{1,5})(,(?<number>[1-9]))?$"
    let match' = Regex.Match(input, pattern)

    validateMatchSuccess match'

    let ticker = match'.Groups.["ticker"].Value
    let number = match'.Groups.["number"].Value
    let number = Int32.TryParse(number) |> snd

    let characters = ".ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    let indexing i = if (i < ticker.Length) then characters.IndexOf(ticker.[i]) else 0

    indexing
    |> Seq.init 5
    |> Seq.fold (fun id i -> (id + i) <<< 5) 0
    |> (+) number
    |> (+) (1 <<< 30)

let ofYahoo (ticker : string) = ticker.Replace("-", ".")
let toYahoo (ticker : string) = ticker.Replace(".", "-")
