module QuoteCapture.Yahoo.QuoteParser

open System
open System.Globalization
open System.Text.RegularExpressions
open QuoteCapture.Extensions
open QuoteCapture.Logging
open QuoteCapture.Yahoo.Types

//-------------------------------------------------------------------------------------------------

let private patternQuotes
    = "()()()()*<input(\\s((data-[^=]+=\"[^\"]*\")|(type=\"hidden\")|(name=\"g\")|(value=\"d\"))){4} />"
    + "((.|\\n)*<input(\\s((data-[^=]+=\"[^\"]*\")|(type=\"hidden\")|(name=\"s\")|(value=\"(?<ticker>([A-Z.-]+))\"))){4} />)"
    + "((.|\\n)*<th align=\"left\">Prices</th>)"
    + "((.|\\n)*<th[^>]*class=\"yfnc_tablehead1\"[^>]*>Adj Close\\*(\\s*)</th></tr>)"
    + "("
    + "<tr>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(?<quoteDate>(.{10,12}))</td>"
    + "("
    + "("
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(?<quoteOpen>([0-9.,]+))</td>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(?<quoteHi>([0-9.,]+))</td>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(?<quoteLo>([0-9.,]+))</td>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(?<quoteClose>([0-9.,]+))</td>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(?<quoteVolume>([0-9,]+))</td>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>([0-9.,]+)</td>"
    + ")"
    + "("
    + "</tr><tr>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(?<dividDate>(.{10,12}))</td>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(\\s*)(?<dividAmount>([0-9.]+))(\\s*)Dividend(\\s*)</td>"
    + ")?"
    + "("
    + "</tr><tr>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(?<splitDate>(.{10,12}))</td>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>(?<splitNew>([0-9]+))(\\s*):(\\s*)(?<splitOld>([0-9]+))(\\s*)Stock Split(\\s*)</td>"
    + ")?"
    + ")"
    + "</tr>"
    + ")+"
    + "<tr>"
    + "<td[^>]*class=\"yfnc_tabledata1\"[^>]*>"
    + "(\\s*)\\* <small>Close price adjusted for dividends and splits.</small>"
    + "</td>"
    + "</tr>"

let patternPagingSelectors
    = "<table[^>]*>"
    + "<tbody>"
    + "<tr>"
    + "<td[^>]*align=\"right\"[^>]*>"
    + "<[^>]+>First</[^>]+>"
    + "(\\s+)(\\|)(\\s+)"
    + "((?<prev>(<a[^>]*rel=\"prev\"[^>]*>Previous</a>))|(<span style=\"color:#999\">Previous</span>))"
    + "(\\s+)(\\|)(\\s+)"
    + "((?<next>(<a[^>]*rel=\"next\"[^>]*>Next</a>))|(<span style=\"color:#999\">Next</span>))"
    + "(\\s+)(\\|)(\\s+)"
    + "<[^>]+>Last</[^>]+>"
    + "</td>"
    + "</tr>"
    + "</tbody>"
    + "</table>"

//-------------------------------------------------------------------------------------------------

let private matchTicker      = "ticker"
let private matchQuoteDate   = "quoteDate"
let private matchQuoteOpen   = "quoteOpen"
let private matchQuoteHi     = "quoteHi"
let private matchQuoteLo     = "quoteLo"
let private matchQuoteClose  = "quoteClose"
let private matchQuoteVolume = "quoteVolume"
let private matchDividDate   = "dividDate"
let private matchDividAmount = "dividAmount"
let private matchSplitDate   = "splitDate"
let private matchSplitNew    = "splitNew"
let private matchSplitOld    = "splitOld"
let private matchNext        = "next"
let private matchPrev        = "prev"

//-------------------------------------------------------------------------------------------------

let private parseDate (value : string) =

    Log.Debug("Parsing date: '{0}'", value)

    let formats = [| "MMM d, yyyy"; "yyyy-MM-dd" |]
    let provider = DateTimeFormatInfo.InvariantInfo
    let style = DateTimeStyles.None
    DateTime.ParseExact(value, formats, provider, style)

let private parseDecimal (value : string) =

    Log.Debug("Parsing value: '{0}'", value)
    Decimal.Parse(value)

let private parseInt64 (value : string) =

    Log.Debug("Parsing value: '{0}'", value)
    Int64.Parse(value.Replace(",", ""))

let private parseInt32 (value : string) =

    Log.Debug("Parsing value: '{0}'", value)
    Int32.Parse(value.Replace(",", ""))

//-------------------------------------------------------------------------------------------------

let private parseQuoteItems (match' : Match) =

    Log.Debug("Parsing quote items...")
    let count = match'.Groups.[matchQuoteDate].Captures.Count
    let items = seq {
        for i = 0 to count - 1 do
            let quoteDate   = parseDate    <| match'.GetValue(i, matchQuoteDate)
            let quoteOpen   = parseDecimal <| match'.GetValue(i, matchQuoteOpen)
            let quoteHi     = parseDecimal <| match'.GetValue(i, matchQuoteHi)
            let quoteLo     = parseDecimal <| match'.GetValue(i, matchQuoteLo)
            let quoteClose  = parseDecimal <| match'.GetValue(i, matchQuoteClose)
            let quoteVolume = parseInt64   <| match'.GetValue(i, matchQuoteVolume)
            yield
                (quoteDate, quoteOpen, quoteHi, quoteLo, quoteClose, quoteVolume)
        }
    items |> Seq.toArray

let private parseDividItems (match' : Match) =

    Log.Debug("Parsing divid items...")
    let count = match'.Groups.[matchDividDate].Captures.Count
    let items = seq {
        for i = 0 to count - 1 do
            let dividDate   = parseDate    <| match'.GetValue(i, matchDividDate)
            let dividAmount = parseDecimal <| match'.GetValue(i, matchDividAmount)
            yield
                (dividDate, (dividAmount))
        }
    items |> Map.ofSeq

let private parseSplitItems (match' : Match) =

    Log.Debug("Parsing split items...")
    let count = match'.Groups.[matchSplitDate].Captures.Count
    let items = seq {
        for i = 0 to count - 1 do
            let splitDate   = parseDate    <| match'.GetValue(i, matchSplitDate)
            let splitNew    = parseInt32   <| match'.GetValue(i, matchSplitNew)
            let splitOld    = parseInt32   <| match'.GetValue(i, matchSplitOld)
            yield
                (splitDate, (splitNew, splitOld))
        }
    items |> Map.ofSeq

//-------------------------------------------------------------------------------------------------

let private validateMatchSuccess (match' : Match) =

    if (not match'.Success) then
        failwith "Input string did not match the expected pattern."

let private validateTicker issue (match' : Match) =

    let tickerMatch = match'.Groups.[matchTicker].Value |> Format.Ticker.ofYahoo
    let tickerValid = issue.Ticker
    if (tickerMatch <> tickerValid) then
        failwith "Incorrect ticker."

//-------------------------------------------------------------------------------------------------

let parseQuotes issue input =

    Log.Debug("Parsing quotes...")
    Log.Extra(input)

    let match' = Regex.Match(input, patternQuotes)

    match' |> validateMatchSuccess
    match' |> validateTicker issue

    let quoteItems = match' |> parseQuoteItems
    let dividItems = match' |> parseDividItems
    let splitItems = match' |> parseSplitItems

    let constructDivid (dividAmount) =
        { Amount = dividAmount }

    let constructSplit (splitNew, splitOld) =
        { New = splitNew
          Old = splitOld }

    let constructQuote (quoteDate, quoteOpen, quoteHi, quoteLo, quoteClose, quoteVolume) =
        { Issue  = issue
          Date   = quoteDate
          Open   = quoteOpen
          Hi     = quoteHi
          Lo     = quoteLo
          Close  = quoteClose
          Volume = quoteVolume
          Divid  = quoteDate |> dividItems.TryFind |> Option.map constructDivid
          Split  = quoteDate |> splitItems.TryFind |> Option.map constructSplit }

    let quotes =
        quoteItems
        |> Array.map constructQuote

    Log.Debug("Parsed quotes.")
    quotes

//-------------------------------------------------------------------------------------------------

let private hasPage (direction : string) input =

    Log.Debug("Parsing paging selectors...")

    let match' = Regex.Match(input, patternPagingSelectors)

    match' |> validateMatchSuccess

    let success = match'.Groups.[direction].Success
    Log.Debug("Parsed paging selectors.")
    success

let hasNextPage = hasPage matchNext
let hasPrevPage = hasPage matchPrev
