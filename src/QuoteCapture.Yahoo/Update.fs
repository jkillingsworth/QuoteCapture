module QuoteCapture.Yahoo.Update

open System
open QuoteCapture.Logging
open QuoteCapture.Yahoo.Types

//-------------------------------------------------------------------------------------------------

let private insert (quote : Quote) =

    Persistence.insertQuote quote
    quote.Divid |> Option.iter (Persistence.insertDivid quote)
    quote.Split |> Option.iter (Persistence.insertSplit quote)

let private update (date : DateTime) (issues : Issue[]) =

    let dateFinal =
        let dateMaximum = Date.getMaximumDate ()
        if (dateMaximum < date) then
            Log.Warn("Cannot update beyond maximum date: {0:d}", date);
            dateMaximum
        else
            date

    Log.Debug("Updating quotes to date: {0:d}", dateFinal);

    use client = new QuoteClient.Client()

    for issue in issues do
        let dateStart = issue |> Persistence.selectQuoteLatestDate |> Date.getNextDate
        if (dateStart > dateFinal) then
            Log.Warn("Cannot update beyond target date: {0}", issue)
        else
            Log.Debug("Getting quotes for: {0}", issue)
            for quote in client.GetData(issue, dateStart, dateFinal) do
                Persistence.transaction (lazy insert quote)

    Log.Debug("Finished updating quotes.")

//-------------------------------------------------------------------------------------------------

let updateOne (date : DateTime) (issue : Issue) =

    let issues = [| issue |]
    update date issues

let updateAll (date : DateTime) =

    let issues = Persistence.selectIssuesActive ()
    update date issues
