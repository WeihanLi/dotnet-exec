# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]

**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: [e.g., Python 3.11, Swift 5.9, Rust 1.75 or NEEDS CLARIFICATION]

**Primary Dependencies**: [e.g., FastAPI, UIKit, LLVM or NEEDS CLARIFICATION]

**Storage**: [if applicable, e.g., PostgreSQL, CoreData, files or N/A]

**Testing**: [e.g., pytest, XCTest, cargo test or NEEDS CLARIFICATION]

**Target Platform**: [e.g., Linux server, iOS 15+, WASM or NEEDS CLARIFICATION]

**Project Type**: [e.g., library/cli/web-service/mobile-app/compiler/desktop-app or NEEDS CLARIFICATION]

**Performance Goals**: [domain-specific, e.g., 1000 req/s, 10k lines/sec, 60 fps or NEEDS CLARIFICATION]

**Constraints**: [domain-specific, e.g., <200ms p95, <100MB memory, offline-capable or NEEDS CLARIFICATION]

**Scale/Scope**: [domain-specific, e.g., 10k users, 1M LOC, 50 screens or NEEDS CLARIFICATION]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [ ] CLI value is preserved and reusable logic is placed in the appropriate
      project boundary (`src/dotnet-exec` vs. `src/ReferenceResolver`)
- [ ] Affected execution and reference modes are identified (raw code, local
      scripts, remote or URI-backed scripts, REPL/test flows, config profiles,
      LinqPad, NetPad, and reference-resolution paths when applicable)
- [ ] Required automated coverage is identified in `tests/UnitTest` and/or
      `tests/IntegrationTest`, with `tests/IntegrationTest/CodeSamples` updates
      noted when sample files are part of the workflow
- [ ] Documentation impact is identified for `README.md`, `README.zh.md`,
      `docs/articles/en`, `docs/articles/zh`, and `docs/ReleaseNotes.md`
- [ ] CI and platform impact is evaluated against Windows, Linux, macOS, and
      .NET 10.x/11.x expectations from `.github/workflows/dotnet.yml`

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused lines and replace them with the real
  repository paths touched by the work. The delivered plan must not leave
  generic scaffolding behind.
-->

```text
src/
├── dotnet-exec/
└── ReferenceResolver/

tests/
├── UnitTest/
└── IntegrationTest/
   └── CodeSamples/

docs/
└── articles/
   ├── en/
   └── zh/
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> Fill this table for any deviation from the constitution. An empty table means
> the deviations were checked and none are required.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
