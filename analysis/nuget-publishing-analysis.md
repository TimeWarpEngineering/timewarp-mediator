# TimeWarp Mediator - NuGet Publishing Analysis

## Executive Summary

This repository is a fork of the popular MediatR library by Jimmy Bogard, created in response to the original MediatR going commercial. TimeWarp Mediator will serve as the open source alternative, released under the Unlicense (public domain), providing the community with a free mediator implementation. The goal is to make minimal code changes while updating all necessary metadata and packaging information to establish this as the go-to open source alternative.

## Current Repository State ✅ UPDATED

### Repository Structure
- **Solution File**: `MediatR.sln` *(unchanged for now)*
- **Main Projects**: *(package names TBD)*
  - `src/MediatR/MediatR.csproj` - Main mediator implementation
  - `src/MediatR.Contracts/MediatR.Contracts.csproj` - Contracts-only package
- **Test Projects**:
  - `test/MediatR.Tests/MediatR.Tests.csproj` - ✅ All 108 tests passing
  - `test/MediatR.Benchmarks/MediatR.Benchmarks.csproj` - ✅ Builds successfully
- **Sample Projects**: 9 example projects - ✅ All build successfully
- **Build Infrastructure**: ✅ Complete solution builds without errors

### Current Branding & Metadata

#### Package Information ✅ UPDATED (via Directory.Build.props)
- **Authors**: Steven T. Cramer *(updated)*
- **Copyright**: Copyright Jimmy Bogard (original MediatR); TimeWarp Engineering (Unlicense) *(updated)*
- **Product**: TimeWarp Mediator *(updated)*
- **Description**: Simple, unambitious mediator implementation in .NET *(kept for compatibility)*
- **Package Tags**: mediator;MediatR;request;response;queries;commands;notifications;opensource;free;unlicense *(enhanced)*
- **Icon**: Logo.png *(updated - TimeWarp logo included in packages)*
- **License**: Unlicense *(updated)*
- **URL**: https://github.com/TimeWarpEngineering/timewarp-mediator *(updated)*

#### Package Information (MediatR.Contracts.csproj)
- **Description**: Contracts package for requests, responses, and notifications *(kept)*
- **Version**: 2.0.1 (explicit) + LangVersion 10.0 *(build fixes)*
- **Inherits**: All other metadata from Directory.Build.props *(consistent branding)*

### Licensing Situation
- **Current Status**:
  - Original MediatR by Jimmy Bogard: Moving to commercial license
  - This fork (TimeWarp Mediator): Will be released under The Unlicense (public domain)
  - Original Apache 2.0 license allows for this relicensing
- Two license files present: `LICENSE` (Apache 2.0) and `UNLICENSE`
- Project files currently reference Apache-2.0 in `PackageLicenseExpression`
- **Strategic Positioning**: TimeWarp Mediator as the free, open source alternative to the commercial MediatR

### Build & Deployment Infrastructure
- **Build Script**: `Build.ps1` - Builds, tests, and packages
- **Push Script**: `Push.ps1` - Pushes to NuGet using environment variables
- **Contracts Build**: `BuildContracts.ps1` - Specific build for contracts package
- **CI/CD**: Basic GitHub Actions workflow for syncing files
- **Signing**: Strong name signing enabled with `MediatR.snk`
- **Versioning**: Uses MinVer for automatic versioning

## Required Changes for Publishing

### 1. Solution and Project Renaming
- [ ] Rename `MediatR.sln` to `TimeWarp.Mediator.sln`
- [ ] Update all project references in the solution file
- [ ] Rename project files:
  - `MediatR.csproj` → `TimeWarp.Mediator.csproj`
  - `MediatR.Contracts.csproj` → `TimeWarp.Mediator.Contracts.csproj`
  - Update all test and sample project names accordingly

### 2. Namespace Changes
- [ ] Update root namespace from `MediatR` to `TimeWarp.Mediator`
- [ ] Update all `using` statements throughout the codebase
- [ ] Update `RootNamespace` in project files

### 3. Copyright Attribution Updates
- [x] Update copyright in `src/MediatR/MediatR.csproj` 
- [x] Update copyright in `src/MediatR.Contracts/MediatR.Contracts.csproj`
- [x] Implemented via Directory.Build.props for consistent metadata across packages

### 4. Package Metadata Updates

#### For TimeWarp.Mediator.csproj:
```xml
<Authors>Steven T. Cramer</Authors>
<Copyright>Copyright Jimmy Bogard (original MediatR); TimeWarp Engineering (Unlicense)</Copyright>
<PackageId>TimeWarp.Mediator</PackageId>
<Product>TimeWarp Mediator</Product>
<Description>Free, open source mediator implementation in .NET. A community-maintained alternative to the commercial MediatR, released under the Unlicense.</Description>
<PackageLicenseExpression>Unlicense</PackageLicenseExpression>
<PackageProjectUrl>https://github.com/TimeWarpEngineering/timewarp-mediator</PackageProjectUrl>
<RepositoryUrl>https://github.com/TimeWarpEngineering/timewarp-mediator</RepositoryUrl>
<PackageTags>mediator;MediatR;request;response;queries;commands;notifications;opensource;free;unlicense</PackageTags>
```

#### For TimeWarp.Mediator.Contracts.csproj:
```xml
<Authors>TimeWarp Engineering</Authors>
<Copyright>Copyright Jimmy Bogard (original MediatR); TimeWarp Engineering (Unlicense)</Copyright>
<PackageId>TimeWarp.Mediator.Contracts</PackageId>
<Product>TimeWarp Mediator Contracts</Product>
<Description>Free, open source mediator contracts package. A community-maintained alternative to the commercial MediatR.Contracts, released under the Unlicense.</Description>
<PackageLicenseExpression>Unlicense</PackageLicenseExpression>
<PackageTags>mediator;MediatR;contracts;request;response;queries;commands;notifications;opensource;free;unlicense</PackageTags>
```

### 4. Assembly and File Updates
- [ ] Generate new strong name key: `TimeWarp.Mediator.snk`
- [ ] Update `AssemblyOriginatorKeyFile` references
- [ ] Update assembly names in all project files
- [ ] Update `AssemblyName` and `Product` properties

### 5. Logo and Branding Assets
- [x] **CHANGED APPROACH**: Synced TimeWarp logo from parent repository via sync workflow
- [x] Logo.png properly included in both NuGet packages (MediatR and MediatR.Contracts)
- [x] Renamed old assets to `old-assets/` to avoid conflicts
- [x] Updated `PackageIcon` reference to Logo.png in Directory.Build.props
- [ ] **DEFERRED**: README.md branding updates (keeping minimal changes for now)

### 6. Documentation Updates
- [ ] Update README.md with TimeWarp branding
- [ ] Clearly position as the open source alternative to commercial MediatR
- [ ] Maintain attribution to Jimmy Bogard for original work
- [ ] Update installation instructions with new package names
- [ ] Update all wiki/documentation links
- [ ] Add migration guide for users switching from MediatR to TimeWarp.Mediator

### 7. CI/CD Pipeline Setup
- [ ] Set up GitHub Actions for automated builds
- [ ] Configure NuGet API key as repository secret
- [ ] Set up automated versioning with MinVer
- [ ] Configure release workflow for pushing to NuGet.org

### 8. Legal Considerations
- [ ] Ensure proper attribution to Jimmy Bogard for original work
- [ ] Replace Apache 2.0 with Unlicense for entire codebase (allowed by Apache 2.0)
- [ ] Update LICENSE file to Unlicense only
- [ ] Add attribution section in README for original author
- [ ] Remove Apache 2.0 LICENSE file, keep only UNLICENSE

## Recommended Action Plan

### Phase 1: Preparation (No Code Changes)
- [ ] Review and approve this analysis
- [ ] Decide on final naming convention (TimeWarp.Mediator vs other options)
- [ ] Prepare new logo assets
- [ ] Set up NuGet.org account and obtain API keys
- [ ] Prepare messaging about being the open source alternative

### Phase 2: Minimal Changes for Quick Publishing
- [ ] Update package metadata in .csproj files with Unlicense
- [ ] Change package IDs and authors
- [ ] Update copyright to "Released to Public Domain"
- [ ] Update descriptions to position as free alternative
- [ ] Keep existing namespaces to avoid breaking changes
- [ ] Update README with attribution and positioning
- [ ] Generate new strong name key
- [ ] Replace LICENSE file with Unlicense only

### Phase 3: Build and Test ✅ COMPLETED
- [x] Fixed all build issues - complete solution now builds successfully
- [x] Resolved C# language version conflicts (explicit LangVersion 10.0 for netstandard2.0)
- [x] Separated compiler settings - strict for core libraries, permissive for samples/tests
- [x] All 108 tests pass with no failures
- [x] Package creation works - both MediatR and MediatR.Contracts packages build successfully
- [x] Verified package contents include Logo.png and proper metadata
- [x] Implemented proper strong naming for core libraries only (avoiding test conflicts)

### Phase 4: Initial Release
- [ ] Tag release with appropriate version
- [ ] Push packages to NuGet.org
- [ ] Monitor for any issues
- [ ] Update documentation with new package information

### Phase 5: Future Enhancements (Post-Release)
- [ ] Set up comprehensive CI/CD pipeline
- [ ] Consider namespace changes in major version
- [ ] Add TimeWarp-specific features
- [ ] Establish contribution guidelines
- [ ] Build community around the open source alternative
- [ ] Maintain feature parity with commercial MediatR where possible

## Risk Assessment

### Low Risk
- Changing package metadata
- Updating authors and copyright
- Generating new strong name key
- Updating documentation

### Medium Risk
- Maintaining compatibility with existing MediatR users
- SEO and discoverability of new package
- Ensuring other open source projects can continue to use a free mediator

### High Risk
- Namespace changes (recommend deferring to major version)
- Breaking changes to public API
- Potential confusion in the community about which package to use

## Conclusion

TimeWarp Mediator represents an important open source alternative to the now-commercial MediatR library. By releasing under the Unlicense (public domain), TimeWarp Engineering ensures that the .NET community will always have access to a free, unrestricted mediator implementation. The recommended approach maintains all existing functionality while clearly positioning this as the community-friendly alternative.

## Key Advantages of TimeWarp Mediator
- **Completely Free**: Released under Unlicense (public domain)
- **No Restrictions**: Use for any purpose, commercial or otherwise
- **Community Driven**: Open to contributions and improvements
- **Drop-in Replacement**: Compatible with existing MediatR implementations
- **Future Proof**: Will remain free and open source forever

## ✅ PROGRESS UPDATE - BUILD INFRASTRUCTURE COMPLETE

### Completed Tasks (January 2025)
- **✅ Full Build Success**: Complete solution builds without errors
- **✅ All Tests Passing**: 108 tests pass with 0 failures  
- **✅ Package Metadata**: Updated copyright, authors, licensing, URLs via Directory.Build.props
- **✅ Logo Integration**: TimeWarp Logo.png included in both packages
- **✅ Build System Fixes**: 
  - Resolved C# language version conflicts (netstandard2.0 + nullable references)
  - Separated strict compiler settings (core libraries only)
  - Fixed strong naming conflicts (tests vs signed assemblies)
- **✅ Asset Management**: Synced branding assets from parent repository
- **✅ Package Verification**: Both MediatR and MediatR.Contracts packages build correctly

### Ready for Next Phase
The repository is now in a **buildable, testable state** with proper TimeWarp branding and can create functional NuGet packages. The major outstanding decisions are:

1. **Package Naming Strategy**: Keep "MediatR" names vs rename to "TimeWarp.Mediator"
2. **Namespace Strategy**: Keep `MediatR` namespace vs change to `TimeWarp.Mediator`
3. **Release Timing**: When to publish the first packages to NuGet.org

## Next Steps
1. **Decide on final package naming convention** (MediatR vs TimeWarp.Mediator)
2. **Decide on namespace strategy** (breaking change vs compatibility)
3. **Prepare for first NuGet release** (versioning, release notes)
4. **Community announcement strategy** (positioning as open source alternative)