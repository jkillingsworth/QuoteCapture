module QuoteCapture.Oanda.Update

open System
open QuoteCapture.Logging
open QuoteCapture.Oanda.Types

//-------------------------------------------------------------------------------------------------

let private updateQuotes (date : DateTime) (pairs : Pair[]) =

    let dateFinal =
        let dateMaximum = Date.getMaximumDate ()
        if (dateMaximum < date) then
            Log.Warn("Cannot update beyond maximum date: {0:d}", date);
            dateMaximum
        else
            date

    Log.Debug("Updating quotes to date: {0:d}", dateFinal);

    use client = new QuoteClient.Client()

    for pair in pairs do
        let dateStart = pair |> Persistence.selectQuoteLatestDate |> Date.getNextDate
        if (dateStart > dateFinal) then
            Log.Warn("Cannot update beyond target date: {0}", pair)
        else
            Log.Debug("Getting quotes for: {0}", pair)
            for quote in client.GetData(pair, dateStart, dateFinal) do
                Persistence.insertQuote quote

    Log.Debug("Finished updating quotes.")

//-------------------------------------------------------------------------------------------------

let updateQuotesOne (date : DateTime) (pair : Pair) =

    let pairs = [| pair |]
    updateQuotes date pairs

let updateQuotesAll (date : DateTime) =

    let pairs = Persistence.selectPairsActive ()
    updateQuotes date pairs

let updateInterestRates () =

    Log.Debug("Updating interest rates.")

    use client = new InterestRateClient.Client()

    let interestRatesIncoming = client.GetData()
    let interestRatesExisting = Persistence.selectInterestRates ()
    let interestRates = interestRatesIncoming |> Array.except interestRatesExisting

    for interestRate in interestRates do
        Persistence.insertInterestRate interestRate
        Log.Info("Got interest rate: {0}", interestRate)

    Log.Debug("Finished updating interest rates.")
