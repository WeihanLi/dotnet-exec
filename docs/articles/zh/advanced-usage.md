# 高级使用指南

本指南涵盖了有效使用 dotnet-exec 的高级功能和场景。

## 脚本类型和执行模式

### 本地文件执行

使用自定义入口点执行 C# 文件：

```sh
# 默认入口点（Main 方法）
dotnet-exec MyScript.cs

# 自定义入口点
dotnet-exec MyScript.cs --entry MainTest

# 多个入口方法（回退机制）
dotnet-exec MyScript.cs --default-entry MainTest Execute Run
```

### 远程文件执行

直接从 URL 执行脚本：

```sh
# GitHub 原始文件
dotnet-exec https://raw.githubusercontent.com/user/repo/main/script.cs

# 任何可访问的 URL
dotnet-exec https://example.com/scripts/utility.cs

# 使用别名的短链接
dotnet-exec gh:WeihanLi/dotnet-exec/samples/basic.cs
```

### 多文件执行

执行多个相关脚本：

```sh
# 主脚本加附加脚本
dotnet-exec main.cs --additional-script helper.cs --additional-script utils.cs

# 使用通配符模式
dotnet-exec main.cs --additional-script "*.cs"

# 来自不同来源
dotnet-exec main.cs \
  --additional-script ./local/helper.cs \
  --additional-script https://example.com/utils.cs
```

## 高级引用管理

### 复杂 NuGet 场景

```sh
# 版本约束
dotnet-exec script.cs --reference "nuget:Newtonsoft.Json,13.0.1"

# 预发布版本
dotnet-exec script.cs --reference "nuget:Microsoft.EntityFrameworkCore,8.0.0-preview.1"

# 私有 NuGet 源
dotnet-exec script.cs \
  --reference "nuget:MyCompany.Utils" \
  --nuget-source https://nuget.company.com/v3/index.json

# 多个源的包
dotnet-exec script.cs \
  --reference "nuget:PublicPackage" \
  --reference "nuget:PrivatePackage" \
  --nuget-source https://api.nuget.org/v3/index.json \
  --nuget-source https://nuget.company.com/v3/index.json
```

### 框架引用

```sh
# Web 框架
dotnet-exec script.cs --web

# 桌面框架
dotnet-exec script.cs --framework Microsoft.WindowsDesktop.App

# 自定义框架
dotnet-exec script.cs --framework MyCustom.Framework
```

### 项目引用

```sh
# 引用本地项目
dotnet-exec script.cs --reference project:../MyLibrary/MyLibrary.csproj

# 继承项目依赖
dotnet-exec script.cs --project ./MyProject.csproj

# 多项目引用
dotnet-exec script.cs \
  --reference project:../Core/Core.csproj \
  --reference project:../Utils/Utils.csproj
```

## 编译选项和环境控制

### 编译配置

```sh
# 发布模式编译
dotnet-exec script.cs --configuration Release

# 调试符号
dotnet-exec script.cs --configuration Debug

# 优化设置
dotnet-exec script.cs --configuration Release --disable-optimization false
```

### 语言版本

```sh
# 指定 C# 版本
dotnet-exec script.cs --langversion 11

# 最新版本
dotnet-exec script.cs --langversion latest

# 预览功能
dotnet-exec script.cs --langversion preview
```

### 运行时配置

```sh
# 指定目标框架
dotnet-exec script.cs --framework net8.0

# 运行时标识符
dotnet-exec script.cs --runtime win-x64

# 自包含部署
dotnet-exec script.cs --self-contained
```

## 高级 REPL 用法

### 带自定义配置的 REPL

```sh
# 启动带有丰富引用的 REPL
dotnet-exec --reference "nuget:Dapper" --reference "nuget:Newtonsoft.Json"

# 带有 Web 框架的 REPL
dotnet-exec --web

# 使用自定义配置文件的 REPL
dotnet-exec --profile myprofile
```

### REPL 命令

在 REPL 会话中：

```csharp
// 添加引用
#r nuget:CsvHelper
#r nuget:Microsoft.EntityFrameworkCore,7.0.0
#r /path/to/local.dll

// 获取帮助
#help

// 清除屏幕
#cls

// 退出
#exit
```

### 多行输入和自动完成

```csharp
// 多行输入（使用 \ 续行）
> var numbers = Enumerable.Range(1, 10) \
* .Where(x => x % 2 == 0) \
* .ToList();

// 自动完成（以 ? 结尾）
> Console.?
WriteLine
Write
ReadLine
...

// 成员列表（以 . 结尾）
> DateTime.
Now
Today
UtcNow
...
```

## 性能优化

### 编译缓存

```sh
# 启用编译缓存（默认）
dotnet-exec script.cs

# 禁用缓存强制重新编译
dotnet-exec script.cs --no-cache

# 清除缓存
dotnet-exec script.cs --clear-cache
```

### 并行执行

```sh
# 并行处理多个脚本
dotnet-exec script1.cs &
dotnet-exec script2.cs &
wait

# 批处理模式
for script in *.cs; do
  dotnet-exec "$script" &
done
wait
```

### 内存管理

```sh
# 限制内存使用
dotnet-exec script.cs --max-memory 512MB

# 垃圾回收配置
DOTNET_GCServer=1 dotnet-exec script.cs

# 针对大型脚本优化
dotnet-exec large-script.cs --configuration Release --optimization-level Speed
```

## 集成模式

### CI/CD 集成

```yaml
# GitHub Actions
- name: Run C# Script
  run: dotnet-exec scripts/deploy.cs --env Production

# Azure DevOps
- script: dotnet-exec scripts/build-helper.cs
  displayName: 'Run Build Helper'
```

### Docker 集成

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0
RUN dotnet tool install -g dotnet-execute
COPY scripts/ /app/scripts/
WORKDIR /app
ENTRYPOINT ["dotnet-exec"]
```

```sh
# 运行容器化脚本
docker run --rm -v $(pwd)/scripts:/app/scripts my-dotnet-exec script.cs
```

### MSBuild 集成

```xml
<!-- 项目文件中 -->
<Target Name="RunScript" BeforeTargets="Build">
  <Exec Command="dotnet-exec scripts/pre-build.cs" />
</Target>
```

## 错误处理和调试

### 详细日志记录

```sh
# 启用详细日志
dotnet-exec script.cs --verbose

# 调试级别日志
dotnet-exec script.cs --verbose --log-level Debug

# 输出到文件
dotnet-exec script.cs --verbose > execution.log 2>&1
```

### 编译错误诊断

```sh
# 显示详细编译错误
dotnet-exec script.cs --show-compilation-errors

# 输出编译的程序集
dotnet-exec script.cs --compile-output ./output/compiled.dll

# 保留临时文件进行调试
dotnet-exec script.cs --keep-temp-files
```

### 性能分析

```sh
# 显示执行时间
dotnet-exec script.cs --timing

# 内存使用分析
dotnet-exec script.cs --memory-profiling

# 编译性能分析
dotnet-exec script.cs --compilation-timing
```

## 高级配置场景

### 环境特定配置

```sh
# 开发环境配置文件
dotnet-exec config set-profile dev \
  --reference "nuget:Microsoft.Extensions.Logging.Debug" \
  --using "Microsoft.Extensions.Logging" \
  --property "Environment=Development"

# 生产环境配置文件
dotnet-exec config set-profile prod \
  --reference "nuget:Microsoft.Extensions.Logging.EventLog" \
  --property "Environment=Production" \
  --optimization Release
```

### 团队共享配置

```sh
# 导出配置文件
dotnet-exec config export --profile team-config --output team-config.json

# 导入配置文件
dotnet-exec config import --file team-config.json

# 版本控制中的配置
echo "team-config.json" >> .gitignore
dotnet-exec config export --profile team --output .dotnet-exec/team.json
```

### 复杂别名设置

```sh
# 创建复杂别名
dotnet-exec alias set web-test \
  --reference "nuget:Microsoft.AspNetCore.Mvc.Testing" \
  --reference "nuget:xunit" \
  --web \
  --using "Microsoft.AspNetCore.Mvc.Testing" \
  --using "Xunit"

# 使用别名
dotnet-exec web-test my-web-test.cs
```

## 自定义扩展

### 自定义中间件

```csharp
// 自定义选项处理中间件
public class CustomMiddleware : IOptionsConfigureMiddleware
{
    public Task Execute(ExecOptions options)
    {
        // 自定义逻辑
        if (options.References.Any(r => r.Contains("EntityFramework")))
        {
            options.Usings.Add("Microsoft.EntityFrameworkCore");
        }
        return Task.CompletedTask;
    }
}
```

### 插件系统(TBD)

```sh
# 加载自定义插件
dotnet-exec script.cs --plugin ./MyCustomPlugin.dll

# 多个插件
dotnet-exec script.cs \
  --plugin ./Plugin1.dll \
  --plugin ./Plugin2.dll
```

这些高级功能让 dotnet-exec 成为一个强大而灵活的 C# 脚本执行工具，适用于从简单自动化到复杂企业级场景的各种用例。
