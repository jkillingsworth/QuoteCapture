select top 1
    quote.[Date] as [Date]
from
    [Quote] quote
where
    quote.[PairId] = @pairId
order by
    quote.[Date] desc
