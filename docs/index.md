---
_layout: landing
---

# dotnet-exec

## Intro

`dotnet-exec` is a command-line tool for executing C# program without a project file, and you can have your custom entry point other than the `Main` method

Slides:

- [Makes C# more simple -- .NET Conf China 2022](https://github.com/WeihanLi/dotnet-exec/blob/main/docs/slides/dotnet-conf-china-2022-dotnet-exec_makes_csharp_more_simple.pdf)
- [dotnet-exec simpler C# -- .NET Conf China 2023 Watch Party Shanghai](https://github.com/WeihanLi/dotnet-exec/blob/main/docs/slides/dotnet-exec-simpler-csharp.pdf)

Github Action for executing without dotnet environment

- <https://github.com/WeihanLi/dotnet-exec-action>
- <https://github.com/marketplace/actions/dotnet-exec>

## Install/Update

### dotnet tool

Latest stable version:

```sh
dotnet tool update -g dotnet-execute
```

Latest preview version:

```sh
dotnet tool update -g dotnet-execute --prerelease
```

Install failed? try the command below:

```sh
dotnet tool update -g dotnet-execute --prerelease --add-source https://api.nuget.org/v3/index.json --ignore-failed-sources
```

### Container support

Execute with docker

``` sh
docker run --rm weihanli/dotnet-exec:latest "1+1"
```

``` sh
docker run --rm weihanli/dotnet-exec:latest "Guid.NewGuid()"
```

``` sh
docker run --rm --pull=always weihanli/dotnet-exec:latest "ApplicationHelper.RuntimeInfo"
```

Execute with podman

``` sh
podman run --rm weihanli/dotnet-exec:latest "1+1"
```

``` sh
podman run --rm weihanli/dotnet-exec:latest "Guid.NewGuid()"
```

``` sh
podman run --rm --pull=always weihanli/dotnet-exec:latest "ApplicationHelper.RuntimeInfo"
```

for the full image tag list, see <https://hub.docker.com/r/weihanli/dotnet-exec/tags>

## Examples

### Get started

Execute local file:

``` sh
dotnet-exec HttpPathJsonSample.cs
```

Execute a local file with custom entry point:

``` sh
dotnet-exec 'HttpPathJsonSample.cs' --entry MainTest
```

Execute remote file:

``` sh
dotnet-exec https://github.com/WeihanLi/SamplesInPractice/blob/master/net7Sample/Net7Sample/ArgumentExceptionSample.cs
```

Execute raw code:

``` sh
dotnet-exec 'Console.WriteLine(1+1);'
```

Execute the raw script:

```sh
dotnet-exec '1 + 1'
```

``` sh
dotnet-exec 'Guid.NewGuid()'
```

### References

Execute raw code with custom references:

NuGet package reference:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "nuget: WeihanLi.Npoi,2.5.0" -u "WeihanLi.Npoi"
```

Local dll reference:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "./out/WeihanLi.Npoi.dll" -u "WeihanLi.Npoi"
```

Local dll in a folder references:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "folder: ./out" -u "WeihanLi.Npoi"
```

Local project reference:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "project: ./WeihanLi.Npoi.csproj" -u "WeihanLi.Npoi"
```

Framework reference:

``` sh
dotnet-exec 'WebApplication.Create().Run();' --reference 'framework:web'
```

Web framework reference in one option:

``` sh
dotnet-exec 'WebApplication.Create().Run();' --web
```

### Usings

Execute raw code with custom usings:

``` sh
dotnet-exec 'WriteLine(1+1);' --using "static System.Console"
```

Execute script with custom reference:

``` sh
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump()' -r "nuget:WeihanLi.Npoi,2.5.0" -u WeihanLi.Npoi
```

### More

Execute with additional dependencies

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' --ad FileLocalType2.cs
```

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' --addition FileLocalType2.cs
```

Execute with exacting references and usings from the project file

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' --project ./Sample.csproj
```

Execute file with preview features:

``` sh
dotnet-exec RawStringLiteral.cs --preview
```
