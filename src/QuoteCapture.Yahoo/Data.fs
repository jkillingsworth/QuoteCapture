module QuoteCapture.Yahoo.Data

open FSharp.Data
open QuoteCapture.Yahoo.Types

//-------------------------------------------------------------------------------------------------

[<Literal>]
let private connectionName = @"name=database"

[<Literal>]
let private configFile = @"..\..\private\Yahoo\App.config"

//-------------------------------------------------------------------------------------------------

module Issue =

    module SelectByTicker =

        [<Literal>]
        let private sql = @"..\..\sql\Yahoo\Issue.SelectByTicker.sql"

        type private CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

        let private ofRecord (record : CommandProvider.Record) : Issue =

          { IssueId = record.IssueId
            Ticker  = record.Ticker }

        let execute ticker =
            use command = new CommandProvider()
            let records = command.Execute(ticker)
            records
            |> Seq.map ofRecord
            |> Seq.exactlyOne
