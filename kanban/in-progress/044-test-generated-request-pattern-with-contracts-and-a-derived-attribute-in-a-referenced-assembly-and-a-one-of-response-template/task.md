# Test GeneratedRequestPattern with contracts and a derived attribute in a referenced assembly and a OneOf response template

## Description

Task 043 shipped `GeneratedRequestPatternAttribute` (15.0.0-beta.10, released). A review of the
merged code against its first consumer (timewarp-architecture task 483, via timewarp-state task
160) found the code paths should handle the consumer's shape, but no test or sample proves it:

- Every test in `generated-request-pattern-tests.cs` / `.sources.cs` and the sample keep the
  contract types and the derived attribute **in the host compilation**. The architecture
  consumer has them in a **referenced project** (Web.Contracts), and the derived attribute
  (`[ContractState]`) in a referenced analyzer/attributes package.
- Only the simple `IRequest<{ContractType}.Response>` interface template is tested. The
  consumer's contracts respond with `OneOf<{ContractType}.Response, SharedProblemDetails>`,
  a nested generic argument.
- `DerivedAttribute_FromMetadataOnlyReference_IsReadFromConstFields` covers a metadata-only
  derived attribute, but with same-compilation contracts.

Resolution of `{ContractType}` is by dotted name through `request-pattern.type-names.cs:17-57`,
which should find referenced types; duplicate metadata names across assemblies and nested
generic contracts are the risks. This task adds the proof and fixes whatever it finds.

## Checklist

- [ ] Test: contracts (with a nested `Response` record) compiled into a separate assembly referenced by the host; a derived attribute with const templates in another referenced assembly; host declares `[Derived(typeof(Ns.CreateThing))]`; a test sibling generator emits the nested request and handler; `Send` dispatches and the response round-trips
- [ ] Test: interface template `TimeWarp.Mediator.IRequest<OneOf.OneOf<{ContractType}.Response, Ns.SharedProblemDetails>>` (or an equivalent two-arity generic defined in the test, if adding the OneOf package to tests is undesirable) resolves and dispatches
- [ ] Test: a void interface template naming an interface from a referenced assembly (stand-in for `TimeWarp.State.IAction`) and a behavior constrained on that interface runs on the declared request
- [ ] Test: TWM013 reports when the referenced contract type name does not resolve
- [ ] Any generator or analyzer fix the tests expose, with a Design-region note
- [ ] Readme/docs: one paragraph stating that contracts and derived attributes may live in referenced assemblies, and the "declaration must name the interface a behavior is constrained on" limitation
- [ ] `dev build` 0/0, `dev test`, `ganda repo audit` clean; `dev check-version` (bump to beta.11 only if generator or analyzer code changes; tests-only needs no bump)
- [ ] No cross-repo work; stop and report if the consumer shape needs a Mediator surface change

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-17)

## Notes

- Filed per Steve 2026-11-17 ("file the follow up") after the surface review of PR #137.
- Consumers: timewarp-state 160 (declares `TimeWarp.State.IAction` as the interface template),
  then timewarp-architecture 483. Neither is blocked on this task unless a test here fails.
