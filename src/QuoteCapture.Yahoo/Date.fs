module QuoteCapture.Yahoo.Date

open System

//-------------------------------------------------------------------------------------------------

let private minimumDate = DateTime(1960, 01, 01)
let private addDays days (date : DateTime) = date.AddDays(float days)

//-------------------------------------------------------------------------------------------------

let private holidays = Persistence.selectHolidays ()

let private isWeekendSat (date : DateTime) = date.DayOfWeek = DayOfWeek.Saturday
let private isWeekendSun (date : DateTime) = date.DayOfWeek = DayOfWeek.Sunday

let private isWeekendOrHoliday = function
    | date when isWeekendSat date -> true
    | date when isWeekendSun date -> true
    | date -> Array.contains date holidays

let rec private findNearest increment date =
    if (date |> isWeekendOrHoliday) then
        date |> addDays increment |> findNearest increment
    else
        date

//-------------------------------------------------------------------------------------------------

let getMinimumDate () = minimumDate

let getMaximumDate () =

    let timeZoneEst = "Eastern Standard Time"
    let hour08PmEst = 20

    let dateTime = DateTime.Now
    let dateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, timeZoneEst)
    let dateTime =
        if (dateTime.Hour < hour08PmEst) then
            dateTime |> addDays -1
        else
            dateTime

    dateTime.Date |> findNearest -1

let getNextDate = function
    | Some date
        -> date |> addDays +1 |> findNearest +1
    | None
        -> getMinimumDate ()
