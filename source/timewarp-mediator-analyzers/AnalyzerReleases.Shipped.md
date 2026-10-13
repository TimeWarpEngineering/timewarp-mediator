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
TWM005  | TimeWarp.Mediator | Warning | AotBehaviorNotWoven, shipped in 14.0.0-beta.5
TWM006  | TimeWarp.Mediator | Error | AotHandlerNotConstructible, shipped in 14.0.0-beta.5
TWM007  | TimeWarp.Mediator | Error | InaccessibleMemberType, shipped in 14.0.0-beta.5
TWM008  | TimeWarp.Mediator | Error | MediatorRuntimeMissing, shipped in 14.0.0-beta.5
TWM009  | TimeWarp.Mediator | Warning | UnsupportedHandlerKind, shipped in 14.0.0-beta.5
TWM010  | TimeWarp.Mediator | Warning | InvalidBehaviorRegistration, shipped in 14.0.0-beta.5
TWM011  | TimeWarp.Mediator | Warning | BehaviorAppliesToNoRequest, shipped in 14.0.0-beta.5
