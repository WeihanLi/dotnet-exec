# Release Notes

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
