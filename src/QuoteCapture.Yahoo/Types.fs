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
