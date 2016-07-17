module QuoteCapture.Oanda.InterestRateParser

open System
open System.Globalization
open System.Text.RegularExpressions
open QuoteCapture.Extensions
open QuoteCapture.Logging
open QuoteCapture.Oanda.Types

//-------------------------------------------------------------------------------------------------

let private patternInterestRates
    = "((\\s)*<table[^>]*bordercolor=\"#E8EBEE\"[^>]*>)"
    + "((\\s)*<tbody>)"
    + "((\\s)*<tr>)"
    + "((\\s)*<td[^>]*bgcolor=\"#D6D9DF\"[^>]*><font[^>]*> CURRENCY</font></td>)"
    + "((\\s)*<td[^>]*bgcolor=\"#D6D9DF\"[^>]*><font[^>]*> BID</font></td>)"
    + "((\\s)*<td[^>]*bgcolor=\"#D6D9DF\"[^>]*><font[^>]*> ASK</font></td>)"
    + "((\\s)*<td[^>]*bgcolor=\"#D6D9DF\"[^>]*><font[^>]*> DATE</font></td>)"
    + "((\\s)*</tr>)"
    + "((\\s)*<tr>(\\s)*"
    + "<td[^>]*bgcolor=\"#EFEFEF\"[^>]*><font[^>]*> (?<currencyName>([A-Z]{3}))</font></td>"
    + "<td[^>]*bgcolor=\"#EFEFEF\"[^>]*><font[^>]*> (?<bid>(.+))</font></td>"
    + "<td[^>]*bgcolor=\"#EFEFEF\"[^>]*><font[^>]*> (?<ask>(.+))</font></td>"
    + "<td[^>]*bgcolor=\"#EFEFEF\"[^>]*><font[^>]*> (?<dateTime>(.+))</font></td>"
    + "</tr>)+?"
    + "((\\s)*</tbody>)"
    + "((\\s)*</table>)"

//-------------------------------------------------------------------------------------------------

let private matchCurrencyName = "currencyName"
let private matchDateTime     = "dateTime"
let private matchBid          = "bid"
let private matchAsk          = "ask"

//-------------------------------------------------------------------------------------------------

let private parseDateTime (value : string) =

    Log.Debug("Parsing date: '{0}'", value)

    let formats = [| "ddd MMM d HH:mm:ss yyyy" |]
    let provider = DateTimeFormatInfo.InvariantInfo
    let style = DateTimeStyles.AllowInnerWhite
    DateTime.ParseExact(value, formats, provider, style)

let private parseValue (value : string) =

    Log.Debug("Parsing value: '{0}'", value)
    Decimal.Parse(value)

//-------------------------------------------------------------------------------------------------

let private validateMatchSuccess (match' : Match) =

    if (not match'.Success) then
        failwith "Input string did not match the expected pattern."

//-------------------------------------------------------------------------------------------------

let parseInterestRates input =

    Log.Debug("Parsing interest rates...")
    Log.Extra(input)

    let match' = Regex.Match(input, patternInterestRates)

    match' |> validateMatchSuccess

    let count = match'.Groups.[matchCurrencyName].Captures.Count
    let items = seq {
        for i = 0 to count - 1 do
            let currencyName = match'.GetValue(i, matchCurrencyName)
            let dateTime     = match'.GetValue(i, matchDateTime)
            let bid          = match'.GetValue(i, matchBid)
            let ask          = match'.GetValue(i, matchAsk)
            yield
                (currencyName, dateTime, bid, ask)
        }

    let constructInterestRate (currencyName, dateTime, bid, ask) =
        { Currency = Persistence.selectCurrencyByName currencyName
          DateTime = parseDateTime dateTime
          Bid      = parseValue bid
          Ask      = parseValue ask }

    let filterInterestRate interestRate =
        if (interestRate.Bid = 0m && interestRate.Ask = 0m) then
            Log.Debug("Skipping interest rate: {0}", interestRate)
            false
        else
            true

    let interestRates =
        items
        |> Seq.map constructInterestRate
        |> Seq.filter filterInterestRate
        |> Seq.toArray

    Log.Debug("Parsed interest rates.")
    interestRates
