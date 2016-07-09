select
    currency.[CurrencyId] as [CurrencyId],
    currency.[Name]       as [Name]
from
    [Currency] currency
where
    currency.[Name] = @name
