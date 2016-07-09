select
    pair.[PairId]         as [PairId],
    pair.[BaseCurrencyId] as [BaseCurrencyId],
    pair.[QuotCurrencyId] as [QuotCurrencyId],
    base.[Name]           as [BaseName],
    quot.[Name]           as [QuotName]
from
    [Pair] pair
    inner join [Currency] base on pair.BaseCurrencyId = base.CurrencyId
    inner join [Currency] quot on pair.QuotCurrencyId = quot.CurrencyId
where
    base.[Name] = @baseName and quot.[Name] = @quoteName
