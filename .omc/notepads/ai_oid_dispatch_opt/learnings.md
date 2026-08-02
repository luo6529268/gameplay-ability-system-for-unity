# Learnings

- `InputRuntime.cs` keeps the `Rand(ai.Rand5 + 1)` draw outside the OID-specific first chain. The Unity optimization must retain that draw before dispatching.
- OID 2 is the only member of both follow-up helper groups, so its specialized route must short-circuit helper 2 before helper 3.
