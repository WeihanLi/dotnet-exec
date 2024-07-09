# Release Notes

## 0.22.0

- [x] feature: implement repl, start repl when there's no arguments provided
- [x] feature: support linqpad script execution
- [ ] upgrade for .NET 9 preview 6 release

## 0.21.0

- upgrade for .NET 9 preview 5 release
- upgrade roslyn packages to support new C# features

## 0.20.0

- feature: respect `Environment.ExitCode`
- feature: container image add back arm32 support, arm32 container issue fixed in .NET 9 preview 4
- feature: support `GetSourceRepositories` on `INuGetHelper`, public `NuGetLoggerLoggingAdapter` 

## 0.19.0

- feature: multi file as arguments enhancement
- feature: script reference support
- feature: support web reference environment variable, and enabled by default for `dotnet-exec:web` container image

## 0.18.0

- Add `net9.0` target support
- Multi-platform container image support(`linux/amd64`/`linux/arm64`/`linux/arm`)
- Enhancements for `ProjectReferenceResolver`, add retry for building project
- Include `System.Net.Http.Json` namespace by default
- Support `--compile-out` option to export compile result

## 0.17.0

- Refactor `NuGetHelper`, support `sources` options for nuget sources filters
- docker image enhancements, add `entrypoint`, default `command` and configure `dotnet-exec` as executable
- add env `DOTNET_EXEC_DEBUG_ENABLED` to enable debug model
- add `--info` for output the tool and runtime info

## 0.16.0

- Update config profile directory path to fix config profile error in Linux Platform
- Add private nuget source support via nuget config
- Add `--nuget-config` option to specify a nuget config file path
- Add `--dry-run` option to compile code without execution
- Add reference normalize to avoid duplicate references
- Allow more style of references
- Support `--env` option to setup environment
- Support `--` to pass raw command arguments
- Replace cake build with dotnet-exec + C# script

## 0.15.0

- .NET 8 Release, C# 12 features support
- Add `--compile-symbol` for compile preprocessor symbol names
- Add `--compile-feature` for compile features, for interceptor support
- Rename `--compiler-type` value `default` to `simple`

## 0.14.0

- Add source generator support for default compiler
- Add `--compiler` alias for `--compiler-type` and `--executor` for `--executor-type`
- Add `--generator` option to enable generator support, disabled by default
- Add variable replacement for project file resolve
- Use exit code from script if exists

## 0.13.0

- Support for .NET 8
- Fixes file local types with the default compiler
- Add `-e` alias for `--entry`
- Use `global` keyword for global usings
- Update support for source generator

## 0.12.0

- ConfigProfile support
- Update support for removing references and usings
- Add support for gitee uri transform
- Support project reference when exacting from project file
- Import implicit using for framework automatically
- Add additional script support for script

## 0.11.0

- ProjectReference support
- Disable web framework references by default to improve perf
- Include `Microsoft.Extensions.*`(`Configuration`/`DependencyInjection`/`Logging`) for wide reference
- Fallback to NuGet package reference when runtime not found

## 0.10.0

- Support using static and using alias for script
- Cleanup unnecessary references

## 0.9.0

- Release for .NET 7.0
- Implicit code/script execution
- Removed advanced compiler
- Reference handling enhancements

## 0.8.0

- Add additional scripts option to include dependencies
- Support C# 11 features
- Add `ReferenceResolver`

## 0.7.0

- Support script embedded reference and using

## 0.6.0

- Support script execute

## 0.5.0

- Implement references handling
- Support execute without sdk

## 0.4.0

- Refactor references handling
- Add startup type support
- Refactor `ScriptContentFetcher`
- Enhancements for usings and references
- Fix Linux support

## 0.3.0

- Refactor `CodeExecutor`
- Remote script execution support

## 0.2.0

- `CodeCompiler` enhancements
- Update references handling

## 0.1.0

- Init
