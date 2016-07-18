module QuoteCapture.Oanda.Date

open System

//-------------------------------------------------------------------------------------------------

let private minimumDate = DateTime(1970, 01, 01)
let private addDays days (date : DateTime) = date.AddDays(float days)

//-------------------------------------------------------------------------------------------------

let getMinimumDate () = minimumDate

let getMaximumDate () = DateTime.UtcNow.Date |> addDays -1

let getNextDate = function
    | Some date
        -> date |> addDays +1
    | None
        -> getMinimumDate ()
