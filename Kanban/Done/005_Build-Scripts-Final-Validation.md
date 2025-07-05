# Task 005: Build Scripts & Final Validation

## Description
Update all build scripts and perform comprehensive final validation to ensure the complete MediatR to TimeWarp.Mediator rename is successful and ready for production use, including CI/CD pipeline readiness and NuGet publishing preparation.

## Parent
000_MediatR-to-TimeWarp-Mediator-Rename-Epic

## Dependencies
1. 001_Project-Files-Assembly-Rename must be completed and validated
2. 002_Core-Library-Namespace-Changes must be completed and validated
3. 003_Test-Project-Updates must be completed and validated (all 108 tests passing)
4. 004_Sample-Project-Updates must be completed and validated (all 10 samples working)

## Requirements
- All build scripts work with new project names
- Complete solution builds successfully
- All tests pass (108 tests)
- All samples build and run correctly
- NuGet packages are created correctly with TimeWarp.Mediator branding
- No references to "MediatR" remain in code (except where intentional)
- Ready for CI/CD pipeline and NuGet publishing

## Checklist

### Design
- [x] Plan build script updates required
- [x] Design comprehensive validation approach
- [x] Plan documentation updates

### Implementation

#### Build Scripts Update
- [ ] **Update Build.ps1**:
  - Change `dotnet build src/MediatR/MediatR.csproj` → `dotnet build src/TimeWarp.Mediator/TimeWarp.Mediator.csproj`
  - Change `dotnet build src/MediatR.Contracts/MediatR.Contracts.csproj` → `dotnet build src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj`
  - Change `dotnet test test/MediatR.Tests/MediatR.Tests.csproj` → `dotnet test test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj`
  - Change `dotnet pack src/MediatR/MediatR.csproj` → `dotnet pack src/TimeWarp.Mediator/TimeWarp.Mediator.csproj`
  - Change `dotnet pack src/MediatR.Contracts/MediatR.Contracts.csproj` → `dotnet pack src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj`
  - Update any other MediatR references in build script

- [ ] **Update BuildContracts.ps1**:
  - Change `dotnet build src/MediatR.Contracts/MediatR.Contracts.csproj` → `dotnet build src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj`
  - Change `dotnet pack src/MediatR.Contracts/MediatR.Contracts.csproj` → `dotnet pack src/TimeWarp.Mediator.Contracts/TimeWarp.Mediator.Contracts.csproj`
  - Update any other MediatR references in contracts build script

- [ ] **Update Push.ps1**:
  - Update any hardcoded package names from MediatR to TimeWarp.Mediator
  - Update any hardcoded paths that reference MediatR directories
  - Verify NuGet push commands work with new package names

#### Documentation Updates
- [ ] **Update README.md**:
  - Update installation instructions to use TimeWarp.Mediator packages
  - Update usage examples to use TimeWarp.Mediator namespaces
  - Update any references to MediatR in documentation
  - Keep attribution to Jimmy Bogard for original work
  - Position as free, open source alternative to commercial MediatR

- [ ] **Update Directory.Build.props** (if needed):
  - Verify all metadata is correct for TimeWarp.Mediator
  - Ensure package descriptions reference TimeWarp.Mediator
  - Verify copyright and licensing information

#### Final Cleanup
- [ ] **Remove old MediatR.snk file** (keep backup)
- [ ] **Search for remaining MediatR references**:
  - Search all .cs files for any remaining `MediatR` text (excluding intentional comments)
  - Search all .csproj files for any remaining MediatR references
  - Search all .sln files for any remaining MediatR references
  - Search build scripts for any remaining MediatR references

- [ ] **Update git ignore patterns** (if needed):
  - Ensure .gitignore handles new project structure
  - Update any paths that reference MediatR directories

#### Comprehensive Validation
- [ ] **Build Validation**:
  - `dotnet clean` to ensure clean build
  - `dotnet build TimeWarp.Mediator.sln` - complete solution build
  - `dotnet build -c Debug TimeWarp.Mediator.sln` - debug build
  - `dotnet build -c Release TimeWarp.Mediator.sln` - release build
  - Verify no build warnings or errors

- [ ] **Test Validation**:
  - `dotnet test test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj` - **all 108 tests must pass**
  - `dotnet test test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj -c Debug` - debug tests
  - `dotnet test test/TimeWarp.Mediator.Tests/TimeWarp.Mediator.Tests.csproj -c Release` - release tests
  - Run tests multiple times to ensure stability

- [ ] **Benchmark Validation**:
  - `dotnet run --project test/TimeWarp.Mediator.Benchmarks/TimeWarp.Mediator.Benchmarks.csproj -c Release`
  - Compare benchmark results to baseline (should be similar)
  - Verify no performance regressions

- [ ] **Sample Projects Validation**:
  - Test all 10 sample projects build and run successfully
  - Verify each sample produces expected output
  - Test DI container integrations work correctly
  - Verify AspNetCore sample starts up correctly

- [ ] **Package Validation**:
  - Run `Build.ps1` to generate packages
  - Verify `TimeWarp.Mediator.nupkg` is created correctly
  - Verify `TimeWarp.Mediator.Contracts.nupkg` is created correctly
  - Extract packages and verify contents:
    - Correct assembly names
    - Correct namespaces in assemblies
    - Logo.png included
    - Correct metadata (authors, description, etc.)
    - Dependencies are correct

- [ ] **Assembly Validation**:
  - Use reflection to verify public API:
    - Main types are in `TimeWarp.Mediator` namespace
    - No public types remain in `MediatR` namespace
    - Assembly names are correct
    - Strong naming works correctly

#### Integration Testing
- [ ] **Create test consumer project**:
  - Create simple console app that consumes TimeWarp.Mediator packages
  - Test basic request/response flow
  - Test notification flow
  - Test pipeline behaviors
  - Verify it works with `using TimeWarp.Mediator;`

- [ ] **Version Compatibility**:
  - Verify MinVer generates appropriate version numbers
  - Test package version resolution
  - Verify no version conflicts

#### CI/CD Readiness
- [ ] **GitHub Actions**:
  - Update any hardcoded references to MediatR in CI/CD files
  - Verify build workflow works with new project structure
  - Test that automated builds work
  - Verify test execution in CI environment

- [ ] **Release Preparation**:
  - Verify all files are committed
  - Verify working directory is clean
  - Verify branch is ready for PR/merge
  - Verify release notes are prepared

### Documentation
- [ ] Update task completion status
- [ ] Document comprehensive validation results
- [ ] Prepare release documentation

## Notes
- This is the final task - everything must work perfectly
- **Zero tolerance for failures** - all builds, tests, and samples must work
- This task validates the entire rename operation
- After this task, the project is ready for production use

## Implementation Notes

### Validation Checklist
#### Build System
- [ ] `dotnet clean` succeeds
- [ ] `dotnet build` succeeds (Debug and Release)
- [ ] `dotnet test` succeeds (all 108 tests pass)
- [ ] `Build.ps1` succeeds completely
- [ ] `BuildContracts.ps1` succeeds completely
- [ ] `Push.ps1` is ready (don't run unless publishing)

#### Projects
- [ ] All 14 projects build successfully
- [ ] All project references are correct
- [ ] All package references are correct
- [ ] Solution loads correctly in IDE

#### Packages
- [ ] `TimeWarp.Mediator.nupkg` created correctly
- [ ] `TimeWarp.Mediator.Contracts.nupkg` created correctly
- [ ] Package metadata is correct
- [ ] Logo.png included in packages
- [ ] Assembly names are correct

#### Code Quality
- [ ] All namespaces updated correctly
- [ ] No remaining MediatR references (except intentional)
- [ ] All using statements updated
- [ ] Code compiles without warnings

#### Functionality
- [ ] All 108 tests pass
- [ ] All 10 samples work correctly
- [ ] Benchmarks run successfully
- [ ] DI container integrations work

#### Documentation
- [ ] README.md updated
- [ ] Installation instructions updated
- [ ] Usage examples updated
- [ ] Attribution to Jimmy Bogard maintained

### Files to Modify
#### Build Scripts
- `Build.ps1`
- `BuildContracts.ps1`
- `Push.ps1`

#### Documentation
- `README.md`
- Any other documentation files

#### Configuration
- `.gitignore` (if needed)
- CI/CD configuration files (if needed)

### Success Metrics
- **0 build errors or warnings**
- **108/108 tests passing**
- **10/10 sample projects working**
- **2/2 NuGet packages created correctly**
- **0 remaining unintentional MediatR references**

### Risk Mitigation
- Work on git branch created in Task 001
- Have rollback plan ready
- Test everything multiple times
- Document any issues discovered

### Success Criteria
- All build scripts work with new project names
- Complete solution builds successfully
- All tests pass (108 tests)
- All samples build and run correctly
- NuGet packages are created correctly with TimeWarp.Mediator branding
- No references to "MediatR" remain in code (except where intentional)
- Ready for CI/CD pipeline and NuGet publishing

### Next Steps (After Task 005 Completion)
1. **Create pull request** with all changes
2. **Merge to main branch**
3. **Tag release** with appropriate version
4. **Publish packages** to NuGet.org
5. **Announce** TimeWarp.Mediator as open source alternative
6. **Update** TimeWarp.Mediator analysis document as completed