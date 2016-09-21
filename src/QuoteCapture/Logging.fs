module QuoteCapture.Logging

open System
open System.Globalization
open System.IO
open System.Reflection
open log4net.Appender
open log4net.Config
open log4net.Core
open log4net.Layout
open log4net.Layout.Pattern
open log4net.Util

//-------------------------------------------------------------------------------------------------

[<assembly: XmlConfigurator(Watch = true)>] do ()

//-------------------------------------------------------------------------------------------------

[<AllowNullLiteral>]
type SeparatorPatternConverter() =

    inherit PatternLayoutConverter()

    let mutable separator : string = null

    member this.Separator
        with get () = separator and set value = separator <- value

    override this.Convert(writer : TextWriter, loggingEvent : LoggingEvent) =
        writer.Write(separator)

//-------------------------------------------------------------------------------------------------

type ExtraPatternLayout() =

    inherit PatternLayout()

    let separatorKey = "separator"

    let mutable converterException = null
    let mutable converterExtension = null

    let mutable exceptionPattern = PatternLayout.DefaultConversionPattern
    let mutable extensionPattern = PatternLayout.DefaultConversionPattern
    let mutable separator = String.Empty
    let mutable canActivate = true

    member this.ExceptionPattern
        with get () = exceptionPattern and set value = exceptionPattern <- value

    member this.ExtensionPattern
        with get () = extensionPattern and set value = extensionPattern <- value

    member this.Separator
        with get () = separator and set value = separator <- value

    override this.Format(writer : TextWriter, loggingEvent : LoggingEvent) =

        let action (converter : PatternConverter) = converter.Format(writer, loggingEvent)

        match loggingEvent with
        | event when event.ExceptionObject <> null
            ->
            base.Format(writer, loggingEvent)
            this.ForEach(converterException, action)

        | event when event.Level = Level.All
            ->
            this.ForEach(converterExtension, action)

        | _ ->
            base.Format(writer, loggingEvent)

    override this.CreatePatternParser(pattern) =

        let converterInfo = new ConverterInfo()
        converterInfo.Name <- separatorKey
        converterInfo.Type <- typeof<SeparatorPatternConverter>

        let patternParser = base.CreatePatternParser(pattern)
        patternParser.PatternConverters.[separatorKey] <- converterInfo
        patternParser

    override this.ActivateOptions() =
        if canActivate then
            base.ActivateOptions()
            this.ActivateExtraOptions()

    member private this.ActivateExtraOptions() =

        converterException <- this.CreatePatternParser(exceptionPattern).Parse()
        converterExtension <- this.CreatePatternParser(extensionPattern).Parse()

        this.ForEach(converterException, this.ActivateExceptionOption)
        this.ForEach(converterException, this.ActivateSeparatorOption)
        this.ForEach(converterExtension, this.ActivateSeparatorOption)

    member private this.ActivateExceptionOption(converter : PatternConverter) =

        if (converter :? PatternLayoutConverter) then
            let layoutConverter = converter :?> PatternLayoutConverter
            if (layoutConverter.IgnoresException |> not) then
                this.IgnoresException <- false

    member private this.ActivateSeparatorOption(converter : PatternConverter) =

        if (converter :? SeparatorPatternConverter) then
            let separatorConverter = converter :?> SeparatorPatternConverter
            separatorConverter.Separator <- separator

    member private this.ForEach(converter, action) =

        let mutable converter = converter : PatternConverter
        while (converter <> null) do
            action converter
            converter <- converter.Next

//-------------------------------------------------------------------------------------------------

type ConsoleBeepAppender() =

    inherit AppenderSkeleton()

    let mutable frequency = 800
    let mutable duration = 200

    member this.Frequency
        with get () = frequency and set value = frequency <- value

    member this.Duration
        with get () = duration and set value = duration <- value

    override this.Append(loggingEvent : LoggingEvent) =
        Console.Beep(frequency, duration)

//-------------------------------------------------------------------------------------------------

type Log() =

    static member private Logger = LoggerManager.GetLogger(Assembly.GetCallingAssembly(), "*")

    static member private Message(level, format, args) =

        if (Log.Logger.IsEnabledFor(level)) then
            let message = SystemStringFormat(CultureInfo.CurrentCulture, format, args)
            Log.Logger.Log(null, level, message, null)

    static member private Exception(level, ex, message) =

        if (Log.Logger.IsEnabledFor(level)) then
            Log.Logger.Log(null, level, message, ex)

    static member Extra(message) =
        Log.Message(Level.All, message, null)

    static member Trace(message, [<ParamArray>] args) =
        Log.Message(Level.Trace, message, args)

    static member Debug(message, [<ParamArray>] args) =
        Log.Message(Level.Debug, message, args)

    static member Info(message, [<ParamArray>] args) =
        Log.Message(Level.Info, message, args)

    static member Warn(message, [<ParamArray>] args) =
        Log.Message(Level.Warn, message, args)

    static member Error(ex) =
        Log.Exception(Level.Error, ex, "Error.")

    static member Fatal(ex) =
        Log.Exception(Level.Fatal, ex, "Fatal error.")
