---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Include the test work required by the specification and constitution.
Behavior changes to parsing, compilation, reference resolution, or public
workflows require test tasks in the appropriate existing test projects.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **CLI application**: `src/dotnet-exec/`
- **Reference resolution library**: `src/ReferenceResolver/`
- **Unit tests**: `tests/UnitTest/`
- **Integration tests**: `tests/IntegrationTest/`
- **Integration samples**: `tests/IntegrationTest/CodeSamples/`
- **Documentation**: `README.md`, `README.zh.md`, `docs/articles/en/`,
  `docs/articles/zh/`, `docs/ReleaseNotes.md`
- Paths shown below assume this repository layout - adjust to the concrete files
  captured in plan.md

<!--
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.

  The /speckit-tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/

  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment

  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create project structure per implementation plan
- [ ] T002 Initialize [language] project with [framework] dependencies
- [ ] T003 [P] Configure linting and formatting tools

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

Examples of foundational tasks (adjust based on your project):

- [ ] T004 Setup database schema and migrations framework
- [ ] T005 [P] Implement authentication/authorization framework
- [ ] T006 [P] Setup API routing and middleware structure
- [ ] T007 Create base models/entities that all stories depend on
- [ ] T008 Configure error handling and logging infrastructure
- [ ] T009 Setup environment configuration management

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1

> **NOTE**: Write the required tests first and ensure they fail before
> implementation when the story changes behavior.

- [ ] T010 [P] [US1] Add or update unit coverage for [behavior] in
      `tests/UnitTest/[Feature]Tests.cs`
- [ ] T011 [P] [US1] Add or update integration coverage for [workflow] in
      `tests/IntegrationTest/[Feature]Tests.cs`
- [ ] T012 [P] [US1] Add or update required sample inputs in
      `tests/IntegrationTest/CodeSamples/[sample-name].cs`

### Implementation for User Story 1

- [ ] T013 [P] [US1] Update shared abstractions or resolvers in
      `src/ReferenceResolver/[file].cs` when the behavior belongs in the library
- [ ] T014 [P] [US1] Update CLI command, option, or service logic in
      `src/dotnet-exec/[file].cs`
- [ ] T015 [US1] Wire the end-to-end workflow in `src/dotnet-exec/[file].cs`
- [ ] T016 [US1] Add validation and error handling consistent with existing
      patterns
- [ ] T017 [US1] Update public documentation in the exact files identified by
      the specification when this story changes user-facing behavior

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2

- [ ] T018 [P] [US2] Add or update unit coverage for [behavior] in
      `tests/UnitTest/[Feature]Tests.cs`
- [ ] T019 [P] [US2] Add or update integration coverage for [workflow] in
      `tests/IntegrationTest/[Feature]Tests.cs`

### Implementation for User Story 2

- [ ] T020 [P] [US2] Update shared library behavior in
      `src/ReferenceResolver/[file].cs` if needed
- [ ] T021 [US2] Implement the supporting CLI or service behavior in
      `src/dotnet-exec/[file].cs`
- [ ] T022 [US2] Integrate with User Story 1 components where required
- [ ] T023 [US2] Update public documentation if the story changes supported
      workflows

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3

- [ ] T024 [P] [US3] Add or update unit coverage for [behavior] in
      `tests/UnitTest/[Feature]Tests.cs`
- [ ] T025 [P] [US3] Add or update integration coverage for [workflow] in
      `tests/IntegrationTest/[Feature]Tests.cs`

### Implementation for User Story 3

- [ ] T026 [P] [US3] Update the relevant library or CLI implementation in
      `src/ReferenceResolver/[file].cs` or `src/dotnet-exec/[file].cs`
- [ ] T027 [US3] Complete the end-to-end behavior in `src/dotnet-exec/[file].cs`
- [ ] T028 [US3] Update public documentation if the story changes supported
      workflows

**Checkpoint**: All user stories should now be independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] TXXX [P] Documentation updates in docs/
- [ ] TXXX [P] README.md and README.zh.md updates for top-level usage changes
- [ ] TXXX Code cleanup and refactoring
- [ ] TXXX Performance optimization across all stories
- [ ] TXXX [P] Additional unit or integration coverage in
      `tests/UnitTest/` or `tests/IntegrationTest/`
- [ ] TXXX Security hardening
- [ ] TXXX Run quickstart.md validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Required tests MUST be written and FAIL before implementation
- Shared abstractions before CLI orchestration when both change
- CLI or library implementation before documentation updates
- Core implementation before integration validation
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- Required tests for a user story marked [P] can run in parallel
- Independent library and CLI tasks within a story marked [P] can run in
  parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all required tests for User Story 1 together:
Task: "Add or update unit coverage for [behavior] in tests/UnitTest/[Feature]Tests.cs"
Task: "Add or update integration coverage for [workflow] in tests/IntegrationTest/[Feature]Tests.cs"

# Launch independent implementation tasks for User Story 1 together:
Task: "Update shared abstractions in src/ReferenceResolver/[file].cs"
Task: "Update CLI logic in src/dotnet-exec/[file].cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify required tests fail before implementing
- Use Conventional Commit messages when the workflow includes commits
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
