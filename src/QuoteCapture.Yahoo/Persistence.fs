module QuoteCapture.Yahoo.Persistence

open FSharp.Data
open QuoteCapture.Yahoo.Types

//-------------------------------------------------------------------------------------------------

[<Literal>]
let private connectionName = @"name=database"

[<Literal>]
let private configFile = @"..\..\private\Yahoo\App.config"

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
