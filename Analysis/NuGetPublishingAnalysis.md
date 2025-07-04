# TimeWarp Mediator - NuGet Publishing Analysis

## Executive Summary

This repository is a fork of the popular MediatR library by Jimmy Bogard, created in response to the original MediatR going commercial. TimeWarp Mediator will serve as the open source alternative, released under the Unlicense (public domain), providing the community with a free mediator implementation. The goal is to make minimal code changes while updating all necessary metadata and packaging information to establish this as the go-to open source alternative.

## Current Repository State

### Repository Structure
- **Solution File**: `MediatR.sln`
- **Main Projects**:
  - `src/MediatR/MediatR.csproj` - Main mediator implementation
  - `src/MediatR.Contracts/MediatR.Contracts.csproj` - Contracts-only package
- **Test Projects**:
  - `test/MediatR.Tests/MediatR.Tests.csproj`
  - `test/MediatR.Benchmarks/MediatR.Benchmarks.csproj`
- **Sample Projects**: 9 example projects demonstrating various DI container integrations

### Current Branding & Metadata

#### Package Information (MediatR.csproj)
- **Authors**: Jimmy Bogard
- **Copyright**: Copyright Jimmy Bogard
- **Description**: Simple, unambitious mediator implementation in .NET
- **Package Tags**: mediator;request;response;queries;commands;notifications
- **Icon**: gradient_128x128.png
- **License**: Apache-2.0

#### Package Information (MediatR.Contracts.csproj)
- **Authors**: Jimmy Bogard
- **Copyright**: Copyright Jimmy Bogard
- **Description**: Contracts package for requests, responses, and notifications
- **Version**: 2.0.1 (hardcoded)
- **License**: Apache-2.0

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
- [ ] Replace existing logos in `assets/logo/` with TimeWarp branding
- [ ] Update `PackageIcon` reference in project files
- [ ] Ensure README.md reflects new branding

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

### Phase 3: Build and Test
- [ ] Run Build.ps1 to ensure everything builds
- [ ] Run all tests to verify no breaking changes
- [ ] Test package creation locally
- [ ] Verify package contents and metadata

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

## Next Steps
1. Review and approve this analysis
2. Create a detailed task list for implementation
3. Begin with Phase 1 preparation steps
4. Execute minimal changes for quick publishing
5. Announce to the community as the open source alternative