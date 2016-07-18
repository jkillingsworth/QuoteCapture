select
    interestRate.[CurrencyId] as [CurrencyId],
    interestRate.[DateTime]   as [DateTime],
    interestRate.[Bid]        as [Bid],
    interestRate.[Ask]        as [Ask],
    currency.[Name]           as [Name]
from
    [InterestRate] interestRate
    inner join [Currency] currency on interestRate.CurrencyId = currency.CurrencyId
