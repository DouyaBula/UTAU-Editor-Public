# UTAU Editor — Public distribution preparation

Date: 2026-11-03. Initial package version: **0.1.0**. This records source preparation, not confirmation that a GitHub Release has been published.

- Added linked English and Simplified Chinese READMEs covering custom curve editing, three slider toolboxes, Gallery, hitsounds, timing, difficulty and editor customisation.
- Retained official history as the intended parent of a single custom public snapshot, so subsequent upstream merges retain their common ancestor. Private development handoffs and an accidentally tracked Git-log dump are excluded from the public tree; historical algorithm references remain clearly marked.
- Removed upstream-only deployment, Sentry, NuGet-publishing and web-service workflows, plus official support/funding routing. Added a project-specific issue template and lightweight contribution notes.
- Added self-contained portable packaging for six desktop RIDs, dependency manifests and SHA-256 checksums. Tag builds create a draft Release only after every package job succeeds; manual workflow runs create artifacts only. Pose-companion source is a separate optional attachment.
- Desktop releases use an independent name/IPC and default `data/` location, do not take the official legacy import port/file associations, and do not query or install official updates. Existing libraries are not migrated automatically.
- Disabled upstream error-reporting hooks and Sentry build uploads. Removed Sentry's absolute project-directory metadata from assemblies, excluded local symbols, and added a package check for checkout paths/user data.
- Kept upstream copyright/licence and documented the separate resource/font/native-library terms. Packages are unsigned and macOS is not notarised.

## Validation

- Windows x64 self-contained Release publish and ZIP packaging succeeded using .NET SDK 10.0.100.
- Published-assembly checks passed: critical native libraries load, portable marker is detected, root-level stale storage redirects are ignored, configuration and a new Realm database are created in a temporary `data/`, and the updater is the non-network base manager. No application window or real user library was opened.
- Scanned prepared source and locally built assemblies for known private paths; the final checked assemblies contain no checkout path / `Sentry.ProjectDirectory` metadata. This is not a guarantee against every possible secret or server-side historical copy.
- PowerShell parsing, workflow YAML parsing, actionlint and `git diff --check` passed.
- Non-Windows packages and Windows ARM64 still require their first CI runs and target-platform manual acceptance. No full test suite was rerun for documentation/distribution-only work. Previously accepted editor regression results are recorded in the [workflow audit](UTAU_Editor_unified_workflow_audit_2026-11-02.md).
- The pre-existing MessagePack 3.1.8 NU1902 advisory remains; this release-preparation change does not claim to resolve it.

Publication requires a deliberate repository/identity decision. A rewritten branch alone does not erase old GitHub caches, PR references or Actions artifacts; see the [first-publication checklist](../releasing.md#first-publication-privacy-checklist).
