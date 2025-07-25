# dotnet-exec

Package | Latest | Latest Preview
---- | ---- | ----
dotnet-execute | [![dotnet-execute](https://img.shields.io/nuget/v/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/) | [![dotnet-execute Latest](https://img.shields.io/nuget/vpre/dotnet-execute)](https://www.nuget.org/packages/dotnet-execute/absoluteLatest)
ReferenceResolver | [![ReferenceResolver](https://img.shields.io/nuget/v/ReferenceResolver)](https://www.nuget.org/packages/ReferenceResolver/) | [![ReferenceResolver Latest](https://img.shields.io/nuget/vpre/ReferenceResolver)](https://www.nuget.org/packages/ReferenceResolver/absoluteLatest)

[![default](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnet.yml/badge.svg)](https://github.com/WeihanLi/dotnet-exec/actions/workflows/dotnet.yml)

[![Docker Pulls](https://img.shields.io/docker/pulls/weihanli/dotnet-exec)](https://hub.docker.com/r/weihanli/dotnet-exec)

[![GitHub Commit Activity](https://img.shields.io/github/commit-activity/m/WeihanLi/dotnet-exec)](https://github.com/WeihanLi/dotnet-exec/commits/main)

[![GitHub Release](https://img.shields.io/github/v/release/WeihanLi/dotnet-exec)](https://github.com/WeihanLi/dotnet-exec/releases)

[![BuiltWithDot.Net shield](https://builtwithdot.net/project/5741/dotnet-exec/badge)](https://builtwithdot.net/project/5741/dotnet-exec)

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/WeihanLi/dotnet-exec)

[中文介绍](./README.zh-CN.md)

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
dotnet tool update -g dotnet-execute --source https://api.nuget.org/v3/index.json
```

or

```sh
dotnet tool update -g dotnet-execute --prerelease --add-source https://api.nuget.org/v3/index.json --ignore-failed-sources
```

Uninstall or failed to update? Try uninstall and install again

```sh
dotnet tool uninstall -g dotnet-execute
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
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump();' -r "nuget: WeihanLi.Npoi,3.0.0" -u "WeihanLi.Npoi"
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
dotnet-exec 'CsvHelper.GetCsvText(new[]{1,2,3}).Dump()' -r "nuget:WeihanLi.Npoi,3.0.0" -u WeihanLi.Npoi
```

### More

Execute with additional dependencies

``` sh
dotnet-exec 'typeof(LocalType).FullName.Dump();' FileLocalType2.cs
```

or with explicit addition references

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

Execute file with preview features(features requires enable preview feature flag):

``` sh
dotnet-exec RawStringLiteral.cs --preview
```

### Config Profile

You can customize the config you used often into a config profile to reuse it for convenience.

List the profiles had configured:

``` sh
dotnet-exec profile ls
```

Configure a profile:

``` sh
dotnet-exec profile set web -r "nuget:WeihanLi.Web.Extensions" -u 'WeihanLi.Web.Extensions' --web --wide false
```

Get the profile details:

``` sh
dotnet-exec profile get web
```

Remove the profile not needed:

``` sh
dotnet-exec profile rm web
```

Executing with specific profile config:

``` sh
dotnet-exec 'WebApplication.Create().Chain(_=>_.MapRuntimeInfo()).Run();' --profile web --using 'WeihanLi.Extensions'
```

![image](https://user-images.githubusercontent.com/7604648/205428791-48f0863b-ca9a-4a55-93cd-bb5514845c5d.png)

Executing with specific profile config and remove preset specific using:

``` sh
dotnet-exec 'WebApplication.Create().Run();' --profile web --using '-WeihanLi.Extensions'
```

### Alias command

The `alias` command allows you to manage aliases for frequently used commands.

#### List aliases

To list all configured aliases, use the `list` subcommand:

```sh
dotnet-exec alias list
```

You can also use `dotnet-exec alias ls` to list aliases.

#### Set alias

To set a new alias, use the `set` subcommand followed by the alias name and value:

```sh
dotnet-exec alias set <aliasName> <aliasValue>
```

For example, to set an alias for generating a new GUID:

```sh
dotnet-exec alias set guid "Guid.NewGuid()"
```

use example:

```sh
dotnet-exec guid
```

#### Unset alias

To remove an existing alias, use the `unset` subcommand followed by the alias name:

```sh
dotnet-exec alias unset <aliasName>
```

For example, to remove the `guid` alias:

```sh
dotnet-exec alias unset guid
```

## Acknowledgements

- [Roslyn](https://github.com/dotnet/roslyn)
- [NuGet.Clients](https://github.com/NuGet/NuGet.Client)
- [System.CommandLine](https://github.com/dotnet/command-line-api)
- [Thanks JetBrains for the open source Rider license](https://jb.gg/OpenSource?from=dotnet-exec)
- Many thanks to the contributors and users for this project
