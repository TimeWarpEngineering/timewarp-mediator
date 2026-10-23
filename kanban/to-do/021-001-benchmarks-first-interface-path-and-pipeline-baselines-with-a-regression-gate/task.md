# Benchmarks first: interface-path and pipeline baselines with a regression gate

## Description

Every consumer dispatches through `ISender`, `ISender<TScope>` or `IMediator`, but the recorded
comparison baseline measures only the concrete `Mediator.Send(Ping)` overload. Through the
interface the 14.0 generated mediator costs about 65 ns / 240 B, the same as the reflection
dispatcher, because `Send<TResponse>` is emitted as `SendCore` → `Send(object)` → `Box<T>`.
Parent review task 021, findings F2 and F17, gates in section 9.

This task lands before any redesign work so that 021-003 has numbers to beat and later tasks
cannot regress silently.

## Requirements

- Keep the interface-path benchmarks already added to
  `tests/timewarp-mediator-benchmarks-comparison` (`TimeWarpGeneratedInterfaceSend`,
  `TimeWarpGeneratedObjectSend`, `TimeWarpGeneratedInterfaceSendUnit`,
  `TimeWarpGeneratedMonomorphicSendUnit`) and add `ISender<TScope>` and `IPublisher` variants.
- Add a State-shaped pipeline benchmark: one `IAction` request, three open-generic behaviors
  woven through `[assembly: MediatorBehavior]`, dispatched through `ISender<ClientPipeline>`.
- Add an Architecture-shaped benchmark: `IRequest<OneOf<...>>`-style response through `ISender`
  with one behavior.
- Record the 14.0 baseline with `./bin/dev benchmark --project comparison --record` so
  `documentation/benchmarks/comparison-baseline.md` carries interface, scoped, publish and
  pipeline rows with commit SHA and date.
- Add a `--gate` mode (or a separate `dev benchmark-gate` command) that runs the comparison
  project and fails when mean time or allocated bytes for a gated row exceed the recorded
  baseline by more than a documented tolerance. Document the tolerance and how to re-record.
- Targets for 15.0 are recorded in the baseline file as a table, not enforced yet: interface
  Ping/Pong ≤ 1.5× martinothamar and ≤ 24 B; void send 0 B; pipeline with three behaviors
  allocation-free apart from the handler.

## Checklist

- [ ] Scoped-sender and publisher interface benchmarks added
- [ ] State-shaped pipeline benchmark added
- [ ] Architecture-shaped response benchmark added
- [ ] 14.0 baseline recorded with `--record`
- [ ] Regression gate implemented and documented
- [ ] `./bin/dev workflow` green
- [ ] PR with merge commit

## Notes

Parent: 021. First child; nothing depends on it except the gate in 021-003. Review:
`documentation/design/2026-10-13-15.0-breaking-redesign-review.md` sections 3 (F2, F17), 9.
Measured on 2026-10-13: interface 64.8 ns / 240 B, reflection 65.2 ns / 224 B, concrete
29.9 ns / 96 B, martinothamar 8.6 ns / 24 B; void concrete 24.1 ns / 0 B, void interface
43.9 ns / 96 B (`research/interface-path-benchmark.log` in the parent kitchen).

## Session

- Created: 04be23b7-afaf-4196-b07d-39320558a224 (2026-10-23)
