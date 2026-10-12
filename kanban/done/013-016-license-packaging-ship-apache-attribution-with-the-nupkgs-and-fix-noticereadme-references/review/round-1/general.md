# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** `1a2caf5` vs `ae5151d` (product files). `notice`, `readme.md`, `source/Directory.Build.props`, Mediator and Contracts csproj license expressions, `tools/dev-cli` pack layout.

## Summary

Mediator and Contracts override `PackageLicenseExpression` to `Unlicense AND Apache-2.0`. Generators and Analyzers keep the shared `Unlicense`. `source/Directory.Build.props` packs repo-root `notice` and `unlicense` beside the existing logo and readme, and the shared `Copyright` string now has years. The readme License section names the lowercase files. `dev pack` requires `notice`, `unlicense`, and the net10.0 runtime assemblies.

The Apache body in `notice` matches upstream `8a64581:LICENSE` except the filled Appendix line `Copyright 2014-2025 Jimmy Bogard`. Upstream has no NOTICE file. Packed `TimeWarp.Mediator` and `TimeWarp.Mediator.Contracts` 14.0.0-beta.4 nuspecs use `Unlicense AND Apache-2.0`; Analyzers and Generators use `Unlicense`. Each nupkg's `notice` and `unlicense` are byte-identical to the repo files, and the layout paths the gate requires are present. No findings.

## Issues
