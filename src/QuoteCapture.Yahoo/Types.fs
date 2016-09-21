module QuoteCapture.Yahoo.Types

open System

//-------------------------------------------------------------------------------------------------

type Issue =
    { IssueId : int
      Ticker  : string }

type Divid =
    { Amount  : decimal }

type Split =
    { New     : int
      Old     : int }

type Quote =
    { Issue   : Issue
      Date    : DateTime
      Open    : decimal
      Hi      : decimal
      Lo      : decimal
      Close   : decimal
      Volume  : int64
      Divid   : Divid option
      Split   : Split option }

//-------------------------------------------------------------------------------------------------

let rec format (record : obj) =
    match record with
    | :? DateTime as date -> date.ToShortDateString()
    | :? Issue as issue -> issue.Ticker
    | :? Quote as quote -> format quote.Date + ", " + format quote.Issue
    | _ -> record.ToString()
