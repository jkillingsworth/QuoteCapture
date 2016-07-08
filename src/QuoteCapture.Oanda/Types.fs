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
      MinBid     : decimal
      MinAsk     : decimal
      MaxBid     : decimal
      MaxAsk     : decimal }

type InterestRate =
    { Currency   : Currency
      DateTime   : DateTime
      Bid        : decimal
      Ask        : decimal }
