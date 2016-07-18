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

//-------------------------------------------------------------------------------------------------

module private SelectInterestRates =

    [<Literal>]
    let private sql = @"..\..\sql\Oanda\SelectInterestRates.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile>

    let private ofRecord (record : CommandProvider.Record) : InterestRate =

        let currency =
            { CurrencyId = record.CurrencyId
              Name       = record.Name }

        { Currency = currency
          DateTime = record.DateTime
          Bid      = record.Bid
          Ask      = record.Ask }

    let execute () =
        use command = new CommandProvider()
        let records = command.Execute()
        records
        |> Seq.map ofRecord
        |> Seq.toArray

let selectInterestRates =
    SelectInterestRates.execute

//-------------------------------------------------------------------------------------------------

module private InsertQuote =

    [<Literal>]
    let private sql = @"..\..\sql\Oanda\InsertQuote.sql"

    type CommandProvider = SqlCommandProvider<sql, connectionName, ConfigFile = configFile, AllParametersOptional = true>

    let execute quote =
        let command = new CommandProvider()
        command.Execute(Some quote.Pair.PairId, Some quote.Date, Some quote.MedBid, Some quote.MedAsk, quote.MinBid, quote.MinAsk, quote.MaxBid, quote.MaxAsk)
        |> ignore

let insertQuote =
    InsertQuote.execute
