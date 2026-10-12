# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** commits `eb11b24`..`ff3d017` vs parent `d681d99` (717 lines). Product diff only; kitchen `task.md` checked for Results accuracy.

## Summary

The change makes `dev verify-samples` build and run every Exe sample (stdout checks for aot and named-pipelines), runs that step from `dev workflow` on the pr/merge path, and teaches `dev pack` to require a single `lib/net10.0` folder plus the Generators buildTransitive props before it restores and runs `tests/timewarp-mediator-package-smoke` against `artifacts/packages`. Container samples that crashed at startup now exit 0, samples are labelled legacy in `samples/readme.md`, RS0030 is silenced, test Nullable no longer fights the csproj, and `dev benchmark` can record a SHA-stamped baseline. Re-checked the four previously crashing samples, all 11 executables, the aot and named-pipelines stdout strings, the packed nupkg layout, a clean smoke restore and run (`package-pong`), command registration, the baseline SHA `d4784157057bced0a043b01422321a4583575f6a`, and `ganda repo audit` (30 passed, 0 failed). No defects found.
