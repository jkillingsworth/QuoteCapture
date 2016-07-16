module QuoteCapture.Yahoo.Persistence

open FSharp.Data
open QuoteCapture.Yahoo.Types

//-------------------------------------------------------------------------------------------------

[<Literal>]
let private connectionName = @"name=database"

[<Literal>]
let private configFile = @"..\..\private\Yahoo\App.config"

//-------------------------------------------------------------------------------------------------

module private SelectHolidays =

    [<Literal>]
    let private sql = @"..\..\sql\Yahoo\SelectHolidays.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let execute () =
        use command = new CommandProvider()
        let records = command.Execute()
        records
        |> Seq.toArray

let selectHolidays =
    SelectHolidays.execute

//-------------------------------------------------------------------------------------------------

module private SelectIssueByTicker =

    [<Literal>]
    let private sql = @"..\..\sql\Yahoo\SelectIssueByTicker.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let private ofRecord (record : CommandProvider.Record) : Issue =

      { IssueId = record.IssueId
        Ticker  = record.Ticker }

    let execute ticker =
        use command = new CommandProvider()
        let records = command.Execute(ticker)
        records
        |> Seq.map ofRecord
        |> Seq.exactlyOne

let selectIssueByTicker =
    SelectIssueByTicker.execute

//-------------------------------------------------------------------------------------------------

module private SelectIssuesActive =

    [<Literal>]
    let private sql = @"..\..\sql\Yahoo\SelectIssuesActive.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let private ofRecord (record : CommandProvider.Record) : Issue =

      { IssueId = record.IssueId
        Ticker  = record.Ticker }

    let execute () =
        use command = new CommandProvider()
        let records = command.Execute()
        records
        |> Seq.map ofRecord
        |> Seq.toArray

let selectIssuesActive =
    SelectIssuesActive.execute

//-------------------------------------------------------------------------------------------------

module private SelectQuoteLatestDate =

    [<Literal>]
    let private sql = @"..\..\sql\Yahoo\SelectQuoteLatestDate.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let execute issue =
        use command = new CommandProvider()
        let records = command.Execute(issue.IssueId)
        if (records |> Seq.isEmpty) then
            None
        else
            records
            |> Seq.exactlyOne
            |> Some

let selectQuoteLatestDate =
    SelectQuoteLatestDate.execute

//-------------------------------------------------------------------------------------------------

module private InsertQuote =

    [<Literal>]
    let private sql = @"..\..\sql\Yahoo\InsertQuote.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let execute quote =
        let command = new CommandProvider()
        command.Execute(quote.Issue.IssueId, quote.Date, quote.Open, quote.Hi, quote.Lo, quote.Close, quote.Volume)
        |> ignore

let insertQuote =
    InsertQuote.execute

//-------------------------------------------------------------------------------------------------

module private InsertDivid =

    [<Literal>]
    let private sql = @"..\..\sql\Yahoo\InsertDivid.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let execute quote divid =
        let command = new CommandProvider()
        command.Execute(quote.Issue.IssueId, quote.Date, divid.Amount)
        |> ignore

let insertDivid =
    InsertDivid.execute

//-------------------------------------------------------------------------------------------------

module private InsertSplit =

    [<Literal>]
    let private sql = @"..\..\sql\Yahoo\InsertSplit.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let execute quote split =
        let command = new CommandProvider()
        command.Execute(quote.Issue.IssueId, quote.Date, split.New, split.Old)
        |> ignore

let insertSplit =
    InsertSplit.execute
