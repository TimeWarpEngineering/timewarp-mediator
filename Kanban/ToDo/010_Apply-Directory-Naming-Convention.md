# Task 010: Apply Directory Naming Convention

## Description

Review and update this repository to follow the directory naming convention ADR from TimeWarp.Architecture. The ADR will be pulled over as part of task 008, and this task will ensure the repository structure complies with TimeWarp.Architecture standards.

## Parent (optional)
008_Update-Sync-Config-For-Documentation

## Requirements

- Directory naming convention ADR from TimeWarp.Architecture
- Understanding of current repository structure
- Plan for any necessary directory renames or restructuring

## Checklist

### Design
- [ ] Review directory naming convention ADR (from task 008)
- [ ] Audit current directory structure
- [ ] Identify directories that don't comply with convention
- [ ] Plan migration strategy for non-compliant directories
- [ ] Consider impact on build scripts and CI/CD

### Implementation
- [ ] Rename directories to match convention
- [ ] Update file paths in:
  - [ ] Build scripts (Build.ps1)
  - [ ] CI/CD workflows (.github/workflows/ci-cd.yml)
  - [ ] Solution file (TimeWarp.Mediator.sln)
  - [ ] Project references
  - [ ] Documentation links
- [ ] Update any hardcoded paths in code
- [ ] Test build and CI after changes

### Documentation
- [ ] Update README.md with new directory structure
- [ ] Update CLAUDE.md with corrected paths
- [ ] Document the applied convention for future reference
- [ ] Update any architecture documentation

## Notes

This task depends on task 008 bringing over the directory naming convention ADR. The goal is to ensure this repository follows TimeWarp.Architecture organizational standards.

Common areas that may need updates:
- Source directories (src/, test/, samples/)
- Documentation directories
- Tool and script directories
- Asset directories

## Implementation Notes

[To be added during implementation]