﻿select
    issue.[IssueId] as [IssueId],
    issue.[Ticker]  as [Ticker]
from
    [Issue] issue
where
    issue.[Active] = 1

