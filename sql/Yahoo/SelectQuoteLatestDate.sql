select top 1
    quote.[Date] as [Date]
from
    [Quote] quote
where
    quote.[IssueId] = @issueId
order by
    quote.[Date] desc
