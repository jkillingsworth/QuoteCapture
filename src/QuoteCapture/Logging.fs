module QuoteCapture.Logging

open System

//-------------------------------------------------------------------------------------------------

type Log() =

    static member Extra(message : string) =
        ()

    static member Trace(message : string, [<ParamArray>] args : obj[]) =
        ()

    static member Debug(message : string, [<ParamArray>] args : obj[]) =
        ()

    static member Info(message : string, [<ParamArray>] args : obj[]) =
        ()

    static member Warn(message : string, [<ParamArray>] args : obj[]) =
        ()

    static member Error(ex : Exception) =
        ()

    static member Fatal(ex : Exception) =
        ()
