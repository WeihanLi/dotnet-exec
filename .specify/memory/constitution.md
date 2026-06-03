<!--
Sync Impact Report
- Version change: template -> 1.0.0
- Modified principles:
  - [PRINCIPLE_1_NAME] -> I. CLI Value with Reusable Boundaries
  - [PRINCIPLE_2_NAME] -> II. Execution-Mode Compatibility is Non-Negotiable
  - [PRINCIPLE_3_NAME] -> III. Tests Prove Behavioral Changes
  - [PRINCIPLE_4_NAME] -> IV. Documentation Ships with Public Changes
  - [PRINCIPLE_5_NAME] -> V. Surgical Changes, Stable Tooling, and Repository Conventions
- Added sections:
  - Engineering Constraints
  - Workflow & Quality Gates
- Removed sections:
  - None
- Templates requiring updates:
  - ✅ .specify/templates/plan-template.md
  - ✅ .specify/templates/spec-template.md
  - ✅ .specify/templates/tasks-template.md
- Follow-up TODOs:
  - None
-->

# dotnet-exec Constitution

## Core Principles

### I. CLI Value with Reusable Boundaries

All changes MUST preserve `dotnet-exec`'s core promise: executing C# code and
scripts without requiring a project file. CLI-facing orchestration belongs in
`src/dotnet-exec`, while reusable reference-resolution or execution helpers that
serve multiple entry points MUST live in shared abstractions or
`src/ReferenceResolver` instead of being duplicated in command handlers. This
keeps the CLI focused and the reusable library independently evolvable.

### II. Execution-Mode Compatibility is Non-Negotiable

Any change to command-line parsing, script transformation, compilation, or
reference resolution MUST evaluate the affected execution paths before merge. At
minimum, authors MUST account for the relevant combination of raw code
execution, local script files, remote or URI-backed scripts, REPL or test
execution, config-profile-driven execution, and the concrete transformers and
fetchers involved (`LinqpadScriptTransformer`, `NetpadScriptTransformer`,
`UriTransformer`, `ScriptContentFetcher`, and related pipeline services). This
prevents regressions in supported workflows that share infrastructure but diverge
in edge handling.

### III. Tests Prove Behavioral Changes

Behavior changes MUST add or update automated coverage in the closest existing
test projects. Use `tests/UnitTest` for isolated logic and
`tests/IntegrationTest` for end-to-end CLI and reference-resolution behavior;
when integration coverage depends on sample files, keep additions under
`tests/IntegrationTest/CodeSamples` minimal and purposeful. A change that
modifies parsing, compilation, reference resolution, or public command behavior
is incomplete until its test surface reflects the new behavior.

### IV. Documentation Ships with Public Changes

Any public-facing behavior, command surface, or supported scenario change MUST
update the user documentation in the same change. `README.md` covers top-level
usage, `README.zh.md` mirrors top-level guidance when that usage changes, deeper
guides live under `docs/articles/en` and `docs/articles/zh`, and
release-note-worthy changes belong in `docs/ReleaseNotes.md`. This keeps the CLI
promise discoverable in both languages.

### V. Surgical Changes, Stable Tooling, and Repository Conventions

Changes MUST follow `.editorconfig`, preserve existing file headers and
surrounding patterns, and stay scoped to the requested problem. Generated outputs
in `_site`, `artifacts`, and `TestResults` MUST not be hand-edited; package
versions, target frameworks, build infrastructure, and other broad upgrades MUST
remain unchanged unless the task explicitly requires them. Small,
convention-aligned edits are easier to review and safer to ship.

## Engineering Constraints

- The authoritative source layout is `src/dotnet-exec`, `src/ReferenceResolver`,
  `tests/UnitTest`, and `tests/IntegrationTest`; plans and tasks MUST reference
  real repository paths rather than generic scaffolds.
- Local verification MUST prefer existing entry points:
  `dotnet restore .\dotnet-exec.slnx`, `dotnet build .\dotnet-exec.slnx`,
  `dotnet test .\tests\UnitTest\UnitTest.csproj`,
  `dotnet test .\tests\IntegrationTest\IntegrationTest.csproj`, and
  `.\build.ps1 --target=test` as appropriate.
- Changes MUST remain compatible with the CI matrix defined in
  `.github/workflows/dotnet.yml`, which exercises Windows, Linux, and macOS with
  .NET 10.x and 11.x. Avoid platform-specific or TFM-specific behavior unless the
  task explicitly targets it.
- Commit messages created as part of the workflow SHOULD follow the repository's
  Conventional Commit format so automated history stays consistent.

## Workflow & Quality Gates

- Every implementation plan MUST record a Constitution Check that confirms
  CLI/library boundaries, affected execution modes, required tests,
  documentation impact, and CI/platform considerations.
- Every specification MUST identify the user-visible command or workflow impact,
  the affected execution and reference modes, the required test coverage shape
  (unit, integration, or both), and the documentation surfaces that need
  updates.
- Every task list MUST generate concrete tasks against real `.cs`, docs, and
  test paths in this repository. When behavior changes, test tasks are required
  rather than optional, and documentation tasks are required whenever public
  workflows change.
- Any deviation from these principles MUST be documented explicitly in the
  plan's complexity or deviation tracking section, including why the simpler
  compliant path was rejected.
- Compliance review happens in each change review: reviewers and authors are
  responsible for verifying constitution alignment before merge.

## Governance

This constitution governs Spec Kit planning and implementation guidance for
`dotnet-exec`. When this document conflicts with ad hoc workflow notes, generic
Spec Kit scaffolds, or local habits, this constitution takes precedence. It is a
bootstrap adoption for the repository; no earlier ratified constitution exists.

Amendments MUST update this file, include a Sync Impact Report, and propagate
required changes to dependent templates before the new version is considered
effective. Versioning follows semantic versioning for governance: MAJOR for
removed or redefined principles, MINOR for added principles or materially
expanded obligations, and PATCH for clarifications that do not change
expectations.

Every plan, spec, task list, and implementation review MUST include an explicit
constitution compliance check. Open follow-up work or deferred fields MUST be
tracked directly in this document's Sync Impact Report.

**Version**: 1.0.0 | **Ratified**: 2026-06-04 | **Last Amended**: 2026-06-04
