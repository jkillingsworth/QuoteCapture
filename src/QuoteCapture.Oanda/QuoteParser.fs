module QuoteCapture.Oanda.QuoteParser

open System
open System.Globalization
open System.Text.RegularExpressions
open QuoteCapture.Extensions
open QuoteCapture.Logging
open QuoteCapture.Oanda.Types

//-------------------------------------------------------------------------------------------------

let private patternQuote
    = "()()()()*(window.model = new jOanda.ncc.Model\\(\\{)"
    + "((.|\\n)*('quoteDate': new Date\\("
    + "(?<dateY>(\\d+)), "
    + "(?<dateM>(\\d+)), "
    + "(?<dateD>(\\d+)), "
    + "\\d+, \\d+\\),))"
    + "((.|\\n)*('bid_ask_data': "
    + "\\{"
    + "("
    + "|(\"bid\":\"(?<medBid>([0-9.]+))\",?)"
    + "|(\"ask\":\"(?<medAsk>([0-9.]+))\",?)"
    + "|(\"min_bid\":\"(?<minBid>([0-9.]+|-))\",?)"
    + "|(\"min_ask\":\"(?<minAsk>([0-9.]+|-))\",?)"
    + "|(\"max_bid\":\"(?<maxBid>([0-9.]+|-))\",?)"
    + "|(\"max_ask\":\"(?<maxAsk>([0-9.]+|-))\",?)"
    + "){6}"
    + "\\}))"
    + "((.|\\n)*('baseCurrency': \"(?<quot>([A-Z]{3}))\",))"
    + "((.|\\n)*('quoteCurrency': \"(?<base>([A-Z]{3}))\",))"
    + "((.|\\n)*(\\}\\);))"

//-------------------------------------------------------------------------------------------------

let private matchMedBid = "medBid"
let private matchMedAsk = "medAsk"
let private matchMinBid = "minBid"
let private matchMinAsk = "minAsk"
let private matchMaxBid = "maxBid"
let private matchMaxAsk = "maxAsk"
let private matchDateY  = "dateY"
let private matchDateM  = "dateM"
let private matchDateD  = "dateD"
let private matchBase   = "base"
let private matchQuot   = "quot"

let private emptyQuoteValue = "-"

//-------------------------------------------------------------------------------------------------

let private parseDateY (value : string) =

    Log.Debug("Parsing date year: '{0}'", value)
    Int32.Parse(value)

let private parseDateM (value : string) =

    Log.Debug("Parsing date month: '{0}'", value)
    Int32.Parse(value)

let private parseDateD (value : string) =

    Log.Debug("Parsing date day: '{0}'", value)
    Int32.Parse(value)

let private parseValueRequired (value : string) =

    Log.Debug("Parsing required value: '{0}'", value)
    Decimal.Parse(value)

let private parseValueOptional (value : string) =

    Log.Debug("Parsing optional value: '{0}'", value)
    if (value = emptyQuoteValue) then
        None
    else
        Some <| Decimal.Parse(value)

//-------------------------------------------------------------------------------------------------

let private validateMatchSuccess (match' : Match) =

    if (not match'.Success) then
        failwith "Input string did not match the expected pattern."

let private validateBase pair (match' : Match) =

    let baseMatch = match'.Groups.[matchBase].Value
    let baseValid = pair.Base.Name
    if (baseMatch <> baseValid) then
        failwith "Incorrect pair."

let private validateQuot pair (match' : Match) =

    let quotMatch = match'.Groups.[matchQuot].Value
    let quotValid = pair.Quot.Name
    if (quotMatch <> quotValid) then
        failwith "Incorrect pair."

let private validateDate date (match' : Match) =

    let y = parseDateY match'.Groups.[matchDateY].Value
    let m = parseDateM match'.Groups.[matchDateM].Value + 1
    let d = parseDateD match'.Groups.[matchDateD].Value

    let dateMatch = DateTime(y, m, d)
    let dateValid = date
    if (dateMatch <> dateValid) then
        failwith "Incorrect date."

//-------------------------------------------------------------------------------------------------

let parseQuote pair date input =

    Log.Debug("Parsing quote...")
    Log.Extra(input)

    let match' = Regex.Match(input, patternQuote)

    match' |> validateMatchSuccess
    match' |> validateBase pair
    match' |> validateQuot pair
    match' |> validateDate date

    let medBid = parseValueRequired <| match'.Groups.[matchMedBid].Value
    let medAsk = parseValueRequired <| match'.Groups.[matchMedAsk].Value
    let minBid = parseValueOptional <| match'.Groups.[matchMinBid].Value
    let minAsk = parseValueOptional <| match'.Groups.[matchMinAsk].Value
    let maxBid = parseValueOptional <| match'.Groups.[matchMaxBid].Value
    let maxAsk = parseValueOptional <| match'.Groups.[matchMaxAsk].Value

    let quote =
        { Pair = pair
          Date = date
          MedBid = medBid
          MedAsk = medAsk
          MinBid = minBid
          MinAsk = minAsk
          MaxBid = maxBid
          MaxAsk = maxAsk }

    Log.Debug("Parsed quote: {0}", format quote)
    quote
