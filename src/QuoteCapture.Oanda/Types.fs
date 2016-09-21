module QuoteCapture.Oanda.Types

open System

//-------------------------------------------------------------------------------------------------

type Currency =
    { CurrencyId : int
      Name       : string }

type Pair =
    { PairId     : int
      Base       : Currency
      Quot       : Currency }

type Quote =
    { Pair       : Pair
      Date       : DateTime
      MedBid     : decimal
      MedAsk     : decimal
      MinBid     : decimal option
      MinAsk     : decimal option
      MaxBid     : decimal option
      MaxAsk     : decimal option }

type InterestRate =
    { Currency   : Currency
      DateTime   : DateTime
      Bid        : decimal
      Ask        : decimal }

//-------------------------------------------------------------------------------------------------

let rec format (record : obj) =
    match record with
    | :? DateTime as date -> date.ToShortDateString()
    | :? Currency as currency -> currency.Name
    | :? Pair as pair -> format pair.Base + "/" + format pair.Quot
    | :? Quote as quote -> format quote.Date + ", " + format quote.Pair
    | :? InterestRate as interestRate -> format interestRate.DateTime
    | _ -> record.ToString()
