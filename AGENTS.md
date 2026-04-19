# AGENTS.md

## Purpose

This repository contains `dotnet-exec`, a .NET CLI tool for executing C# code and script files without requiring a project file, plus the `ReferenceResolver` library used for package, file, folder, framework, and project references.

Treat this file as the repository-level operating guide for coding agents. Keep changes targeted, preserve existing conventions, and prefer updating tests and docs together with behavior changes.

## Repository layout

- `src/dotnet-exec`: main CLI application
- `src/ReferenceResolver`: reusable reference resolution library
- `tests/UnitTest`: fast unit tests
- `tests/IntegrationTest`: integration tests and sample scripts
- `tests/PerformanceTest`: benchmarks and performance experiments
- `docs`: end-user documentation and release notes
- `build`: build orchestration used by CI and local packaging
- `_site`, `artifacts`, `TestResults`: generated output; do not hand-edit unless the task is specifically about generated artifacts

## Setup and key commands

Prefer `rg` for file discovery and text search.

Primary commands from the repository root:

```powershell
dotnet restore .\dotnet-exec.slnx
dotnet build .\dotnet-exec.slnx
dotnet test .\tests\UnitTest\UnitTest.csproj
dotnet test .\tests\IntegrationTest\IntegrationTest.csproj
.\build.ps1 --target=test
```

Cross-platform build entry points:

```powershell
.\build.ps1 --target=test
```

```bash
bash build.sh --target=build
```

Use focused test runs while iterating:

```powershell
dotnet test .\tests\UnitTest\UnitTest.csproj --filter "FullyQualifiedName~CodeExecutor"
dotnet test .\tests\IntegrationTest\IntegrationTest.csproj --filter "FullyQualifiedName~NuGetReferenceResolverTest"
```

When a change affects packaging or CLI behavior, prefer running the relevant build script target in addition to direct `dotnet test` commands.

## CI expectations

The main GitHub Actions workflow is `.github/workflows/dotnet.yml`.

- Windows CI runs `.\build.ps1 --target=test`
- Linux and macOS CI run `bash build.sh --target=build`
- The workflow installs .NET `10.x` and `11.x`

Before finishing substantial code changes, run the closest equivalent local verification for the area you touched.

## Coding conventions

Follow `.editorconfig`.

- Use spaces for indentation
- Use file-scoped namespaces in C#
- Prefer `var` where the type is apparent
- Keep line length within 120 where practical
- Preserve the file header in C# files
- Do not reorder using directives to force `System.*` first; this repo disables that preference

Match the surrounding style before introducing a new pattern. Do not perform opportunistic style churn in unrelated files.

## Testing expectations

- Add or update tests for behavior changes
- Prefer unit tests for isolated logic and integration tests for end-to-end CLI/reference-resolution behavior
- Many integration tests rely on files under `tests/IntegrationTest/CodeSamples`; keep samples minimal and purposeful
- If you change command-line parsing, script transformation, compilation, or reference resolution, verify with the relevant unit and integration tests

## Documentation expectations

When public behavior, commands, or supported scenarios change, update the relevant documentation:

- `README.md` for main usage and quick-start guidance
- `README.zh.md` when the change affects mirrored top-level documentation
- `docs/articles/en/*` and `docs/articles/zh/*` for deeper guides
- `docs/ReleaseNotes.md` for release-note-worthy behavior changes

## Change boundaries

- Do not hand-edit generated outputs in `_site`, `artifacts`, or `TestResults` unless explicitly required
- Do not upgrade package versions, target frameworks, or build infrastructure unless the task requires it
- Keep changes scoped; avoid broad renames or cleanup-only edits mixed into feature or bug-fix work
- Never revert user changes you did not make

## Agent workflow

1. Read the relevant source files and tests before editing.
2. Make the smallest coherent change that solves the task.
3. Run targeted verification locally.
4. Update tests and docs when behavior changes.
5. Summarize what changed, what you verified, and any remaining gaps.

## Commit Message Convention

Follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification:

```text
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

Common types:

| Type | When to use |
| ------ | ------------- |
| `feat` | A new feature |
| `fix` | A bug fix |
| `docs` | Documentation changes only |
| `style` | Formatting changes (no logic change) |
| `refactor` | Code restructuring (no feature or fix) |
| `test` | Adding or updating tests |
| `chore` | Build process, dependency updates, tooling |
| `perf` | Performance improvements |
| `ci` | CI/CD workflow changes |

Examples:

```text
feat(exec): support .rest file extension in exec command
fix(middleware): handle null response body in logging middleware
docs: update installation instructions in README
chore: bump WeihanLi.Common dependency
```

- Use the **imperative mood** in the description ("add" not "added")
- Keep the first line at 72 characters or fewer
- Reference issues in the footer: `Fixes #123` or `Closes #123`
