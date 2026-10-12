; Shipped analyzer releases for TimeWarp.Mediator.Analyzers.
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md
; Release headers must parse as System.Version (RS2007), so a prerelease is filed under its target
; version: the 14.0.0 betas share "Release 14.0.0" and each row notes the beta it shipped in.

## Release 14.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
TWM001  | TimeWarp.Mediator | Error | RequestHasNoHandler, shipped in 14.0.0-beta.1
TWM002  | TimeWarp.Mediator | Error | DuplicateHandler, shipped in 14.0.0-beta.1
TWM003  | TimeWarp.Mediator | Error | ScopeMismatch, shipped in 14.0.0-beta.1
TWM004  | TimeWarp.Mediator | Error | WrongScopeSend, shipped in 14.0.0-beta.1
