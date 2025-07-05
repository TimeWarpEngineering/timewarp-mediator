# Task 006: Strong Name Key Generation & Windsor Fix

## Description
Generate a new strong name key for TimeWarp.Mediator (rather than using the copied MediatR key) and fix the Windsor ServiceFactory reference issue. Perform final validation to ensure 100% clean build with zero known issues.

## Parent
000_MediatR-to-TimeWarp-Mediator-Rename-Epic

## Dependencies
1. All previous tasks (001-005) must be completed
2. Current state: 158/158 tests passing, 9/10 samples working, 2/2 packages created

## Requirements
- Generate new strong name key specifically for TimeWarp.Mediator
- Fix Windsor ServiceFactory reference issue
- All 10 sample projects build and run successfully  
- All tests continue to pass
- Complete solution builds with zero errors or warnings
- All packages rebuild correctly with new strong name key
- Ready for production release with no known issues

## Checklist

### Design
- [x] Identify strong name key replacement requirements
- [x] Analyze Windsor ServiceFactory reference issue
- [x] Plan comprehensive final validation

### Implementation

#### Strong Name Key Generation
- [ ] **Generate new TimeWarp.Mediator.snk key**:
  - Use `sn -k TimeWarp.Mediator.snk` to generate new key pair
  - Backup current TimeWarp.Mediator.snk (which is just renamed MediatR.snk)
  - Replace with newly generated key
  - Verify key is properly formatted and valid

- [ ] **Update strong name references**:
  - Verify `src/Directory.Build.props` references correct key file
  - Ensure all projects using strong naming pick up new key
  - Test that assemblies are properly signed with new key

- [ ] **Rebuild and test with new key**:
  - Clean and rebuild all projects
  - Verify assemblies have new strong name
  - Test that package generation works with new key
  - Run all tests to ensure no signing issues

#### Windsor ServiceFactory Fix
- [ ] **Identify ServiceFactory type issue**:
  - Find ServiceFactory delegate definition in TimeWarp.Mediator codebase
  - Determine correct namespace (likely `TimeWarp.Mediator`)
  - Check if it's missing using statement or actual type reference

- [ ] **Fix Windsor sample**:
  - Add missing `using TimeWarp.Mediator;` if needed
  - Update ServiceFactory type reference to full qualified name if needed
  - Ensure all ServiceFactory usages in Windsor sample are correct
  - Test that Windsor sample builds successfully

- [ ] **Validate Windsor sample functionality**:
  - Build Windsor sample successfully
  - Run Windsor sample and verify output
  - Ensure DI container integration works properly

#### Final Comprehensive Validation
- [ ] **Zero-issue build validation**:
  - `dotnet clean` - clean all projects
  - `dotnet build` - build complete solution with ZERO errors/warnings
  - Verify all 14 projects build successfully including Windsor
  - Check build output for any remaining issues

- [ ] **Complete test validation**:
  - `dotnet test` - all 158 tests must pass (2 Lamar skips OK)
  - Run tests multiple times to ensure stability
  - Verify no test regressions from key changes

- [ ] **All sample projects validation**:
  - Build all 10 sample projects successfully
  - Run representative samples from each DI container
  - Verify all samples produce expected output
  - Test that all DI integrations work correctly

- [ ] **Package validation with new key**:
  - Generate both NuGet packages with new strong name key
  - Verify packages contain properly signed assemblies
  - Test package metadata is correct
  - Ensure Logo.png and README.md included

- [ ] **Assembly signature validation**:
  - Use `sn -v` to verify strong name signatures
  - Check that assemblies are properly signed
  - Verify no dependency conflicts with new signing

#### Build Script Validation
- [ ] **Test all build scripts work**:
  - Run `Build.ps1` successfully
  - Run `BuildContracts.ps1` successfully  
  - Verify `Push.ps1` is ready (don't actually push)
  - Ensure all scripts work with new strong name key

#### Final Documentation
- [ ] **Update task completion status**
- [ ] **Document strong name key generation process**
- [ ] **Record Windsor fix details**
- [ ] **Prepare final validation report**

## Notes
- **Zero tolerance for known issues** - everything must work perfectly
- This is the final task before the project is 100% complete
- New strong name key establishes TimeWarp.Mediator as independent project
- Windsor fix ensures all 10 samples work without exceptions

## Implementation Notes

### Strong Name Key Requirements
- Generate cryptographically secure new key pair
- Key should be unique to TimeWarp.Mediator project
- Ensure key generation is documented for future reference
- Test that assemblies maintain binary compatibility expectations

### Windsor ServiceFactory Issue Analysis
The error `CS0246: The type or namespace name 'ServiceFactory' could not be found` suggests:
1. Missing using statement for TimeWarp.Mediator namespace
2. ServiceFactory delegate not accessible in current context
3. Possible fully qualified name needed

### Critical Validation Points
1. **Complete solution build**: All 14 projects, zero errors/warnings
2. **All tests pass**: 158/158 tests (2 Lamar skips expected)
3. **All samples work**: 10/10 samples build and run correctly
4. **Packages valid**: Both packages build with proper signatures
5. **No regressions**: Everything that worked before still works

### Files to Modify
#### Strong Name Key
- `TimeWarp.Mediator.snk` (regenerate)
- Verify `src/Directory.Build.props` (should already be correct)

#### Windsor Fix
- `samples/TimeWarp.Mediator.Examples.Windsor/Program.cs` (likely add using statement)

### Success Criteria
- **0 build errors or warnings**
- **158/158 tests passing** 
- **10/10 sample projects working**
- **2/2 NuGet packages created with new signatures**
- **0 remaining known issues**
- **Ready for production release**

### Validation Checklist
- [ ] Complete solution builds successfully
- [ ] All tests pass
- [ ] All 10 samples work
- [ ] Both packages generate correctly
- [ ] Strong name signatures valid
- [ ] No known issues or workarounds needed

### Next Steps (After Task 006 Completion)
1. **Move Epic 000 to Done** - All tasks completed
2. **Commit final changes**
3. **Create pull request**
4. **Tag release**
5. **Publish packages to NuGet.org**
6. **Announce TimeWarp.Mediator as production-ready**

## Risk Mitigation
- Backup existing key before replacement
- Test extensively after strong name key change
- Validate no binary compatibility issues
- Ensure all existing functionality preserved