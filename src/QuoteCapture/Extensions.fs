module QuoteCapture.Extensions

open System.Text.RegularExpressions

//-------------------------------------------------------------------------------------------------

type Match with

    member this.GetValue(index : int, group : string) =

        this.Groups.[group].Captures.[index].Value
