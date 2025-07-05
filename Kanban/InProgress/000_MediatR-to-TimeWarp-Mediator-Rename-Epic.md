# Epic 000: MediatR → TimeWarp.Mediator Rename

## Description
Complete rebrand and rename of the MediatR library to TimeWarp.Mediator, including all namespaces, project names, file names, and assembly configurations. This epic encompasses a comprehensive refactoring that will touch nearly every file in the codebase while maintaining all functionality and establishing TimeWarp.Mediator as the open source alternative to commercial MediatR.
/res
## Dependencies
None - This is the master epic for the complete rename process

## Requirements
- Complete solution builds successfully with TimeWarp.Mediator branding
- All 108 tests pass with new namespaces
- All 10 sample projects work with new namespaces
- NuGet packages created with correct TimeWarp.Mediator metadata
- Build scripts updated and functional
- Documentation updated
- Ready for production release

## Checklist

### Design
- [x] Analyzed current codebase structure (14 projects, 141+ files)
- [x] Planned phased approach to minimize risk
- [x] Designed validation strategy for each phase
- [x] Created detailed task breakdown

### Implementation
This epic is implemented through 5 sequential child tasks:

#### Child Tasks (Sequential Dependencies)
- [x] **001_Project-Files-Assembly-Rename** - Rename project files, directories, and assembly configurations
- [x] **002_Core-Library-Namespace-Changes** - Update core library namespaces only  
- [x] **003_Test-Project-Updates** - Update test projects and ensure all 158 tests pass
- [x] **004_Sample-Project-Updates** - Update all 10 sample projects
- [x] **005_Build-Scripts-Final-Validation** - Update build scripts and final validation
- [ ] **006_Strong-Name-Key-And-Windsor-Fix** - Generate new strong name key and fix Windsor ServiceFactory issue

#### Epic Validation Criteria
- [ ] All child tasks completed successfully
- [ ] Complete solution builds without errors or warnings
- [ ] All 108 tests pass
- [ ] All 10 sample projects build and run correctly
- [ ] Both NuGet packages (TimeWarp.Mediator and TimeWarp.Mediator.Contracts) created correctly
- [ ] No unintentional MediatR references remain in codebase
- [ ] Build scripts functional with new project structure
- [ ] Documentation updated with new branding

### Documentation
- [ ] Update epic status as child tasks complete
- [ ] Document any major issues or decisions
- [ ] Prepare release notes for TimeWarp.Mediator

## Notes
This epic represents the foundational work to establish TimeWarp.Mediator as the free, open source alternative to the now-commercial MediatR library. The work is broken into 5 sequential phases to ensure we can validate and rollback at each step.

**Strategic Importance:**
- Establishes TimeWarp.Mediator as community-friendly alternative
- Maintains backward compatibility at API level (only package names change)
- Preserves all existing functionality
- Released under Unlicense (public domain)

**Risk Mitigation:**
- Sequential task approach allows early problem detection
- Each phase ends with validated, working state
- Git branch strategy enables easy rollback
- Comprehensive testing at each phase

## Implementation Notes

### Scope Summary
- **14 .csproj files** need renaming and content updates
- **1 solution file** needs complete restructuring  
- **141+ C# files** need namespace updates
- **3 build scripts** need updates
- **1 strong name key** needs renaming
- **Directory structure changes** for 14 projects

### Key Technical Challenges
- Maintaining build stability throughout rename process
- Ensuring all DI container integrations work with new assembly names
- Preserving strong name signing with new key
- Updating all internal package references correctly
- Validating no performance regressions

### Success Metrics
- **0 build errors or warnings**
- **108/108 tests passing**
- **10/10 sample projects working**
- **2/2 NuGet packages created correctly**
- **0 remaining unintentional MediatR references**

### Completion Criteria
After epic completion:
- [ ] Complete rename from MediatR to TimeWarp.Mediator is successful
- [ ] All functionality is preserved and validated
- [ ] Packages are ready for NuGet publishing
- [ ] Code is ready for production use
- [ ] Documentation reflects new branding
- [ ] CI/CD pipeline works with new structure

### Next Steps (Post-Epic)
1. Create pull request with all changes
2. Merge to main branch
3. Tag release with appropriate version
4. Publish packages to NuGet.org
5. Announce TimeWarp.Mediator as open source alternative
6. Update project analysis documentation