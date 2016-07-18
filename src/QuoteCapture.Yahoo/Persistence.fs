module QuoteCapture.Yahoo.Persistence

open System.Transactions
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

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile, AllParametersOptional = false>

    let execute quote =
        let command = new CommandProvider()
        let argIssueId = quote.Issue.IssueId
        let argDate    = quote.Date
        let argOpen    = quote.Open
        let argHi      = quote.Hi
        let argLo      = quote.Lo
        let argClose   = quote.Close
        let argVolume  = quote.Volume
        ignore <| command.Execute(argIssueId, argDate, argOpen, argHi, argLo, argClose, argVolume)

let insertQuote =
    InsertQuote.execute

//-------------------------------------------------------------------------------------------------

module private InsertDivid =

    [<Literal>]
    let private sql = @"..\..\sql\Yahoo\InsertDivid.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile, AllParametersOptional = false>

    let execute quote divid =
        let command = new CommandProvider()
        let argIssueId = quote.Issue.IssueId
        let argDate    = quote.Date
        let argAmount  = divid.Amount
        ignore <| command.Execute(argIssueId, argDate, argAmount)

let insertDivid =
    InsertDivid.execute

//-------------------------------------------------------------------------------------------------

module private InsertSplit =

    [<Literal>]
    let private sql = @"..\..\sql\Yahoo\InsertSplit.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile, AllParametersOptional = false>

    let execute quote split =
        let command = new CommandProvider()
        let argIssueId = quote.Issue.IssueId
        let argDate    = quote.Date
        let argNew     = split.New
        let argOld     = split.Old
        ignore <| command.Execute(argIssueId, argDate, argNew, argOld)

let insertSplit =
    InsertSplit.execute

//-------------------------------------------------------------------------------------------------

let transaction (action : Lazy<'T>) =

    use scope = new TransactionScope()
    let value = action.Force()
    scope.Complete()
    value
