module QuoteCapture.Oanda.Persistence

open FSharp.Data
open QuoteCapture.Oanda.Types

//-------------------------------------------------------------------------------------------------

[<Literal>]
let private connectionName = @"name=database"

[<Literal>]
let private configFile = @"..\..\private\Oanda\App.config"

//-------------------------------------------------------------------------------------------------

module private SelectCurrencyByName =

    [<Literal>]
    let private sql = @"..\..\sql\Oanda\SelectCurrencyByName.sql"

    type private CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let private ofRecord (record : CommandProvider.Record) : Currency =

        { CurrencyId = record.CurrencyId
          Name       = record.Name }

    let execute name =
        use command = new CommandProvider()
        let records = command.Execute(name)
        records
        |> Seq.map ofRecord
        |> Seq.exactlyOne

let selectCurrencyByName =
    SelectCurrencyByName.execute

//-------------------------------------------------------------------------------------------------

module private SelectPairByCurrencyNames =

    [<Literal>]
    let private sql = @"..\..\sql\Oanda\SelectPairByCurrencyNames.sql"

    type private CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let private ofRecord (record : CommandProvider.Record) : Pair =

        let currencyBase =
            { CurrencyId = record.BaseCurrencyId
              Name       = record.BaseName }

        let currencyQuot =
            { CurrencyId = record.QuotCurrencyId
              Name       = record.QuotName }

        { PairId = record.PairId
          Base   = currencyBase
          Quot   = currencyQuot }

    let execute baseName quotName =
        use command = new CommandProvider()
        let records = command.Execute(baseName, quotName)
        records
        |> Seq.map ofRecord
        |> Seq.exactlyOne

let selectPairByCurrencyNames =
    SelectPairByCurrencyNames.execute

//-------------------------------------------------------------------------------------------------

module private SelectPairsActive =

    [<Literal>]
    let private sql = @"..\..\sql\Oanda\SelectPairsActive.sql"

    type private CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let private ofRecord (record : CommandProvider.Record) : Pair =

        let currencyBase =
            { CurrencyId = record.BaseCurrencyId
              Name       = record.BaseName }

        let currencyQuot =
            { CurrencyId = record.QuotCurrencyId
              Name       = record.QuotName }

        { PairId = record.PairId
          Base   = currencyBase
          Quot   = currencyQuot }

    let execute () =
        use command = new CommandProvider()
        let records = command.Execute()
        records
        |> Seq.map ofRecord
        |> Seq.toArray

let selectPairsActive =
    SelectPairsActive.execute

//-------------------------------------------------------------------------------------------------

module private SelectQuoteLatestDate =

    [<Literal>]
    let private sql = @"..\..\sql\Oanda\SelectQuoteLatestDate.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let execute pair =
        use command = new CommandProvider()
        let records = command.Execute(pair.PairId)
        if (records |> Seq.isEmpty) then
            None
        else
            records
            |> Seq.exactlyOne
            |> Some

let selectQuoteLatestDate =
    SelectQuoteLatestDate.execute
