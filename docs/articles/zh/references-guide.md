# 引用管理指南

本指南全面介绍了 dotnet-exec 中的引用管理系统，包括 NuGet 包、本地文件、项目引用和框架引用。

## NuGet 包引用

### 基本包引用

```sh
# 最新版本
dotnet-exec script.cs --reference "nuget:Newtonsoft.Json"

# 指定版本
dotnet-exec script.cs --reference "nuget:Newtonsoft.Json,13.0.3"

# 多个包
dotnet-exec script.cs \
  --reference "nuget:Dapper" \
  --reference "nuget:MySql.Data"
```

### 版本约束

```sh
# 精确版本
dotnet-exec script.cs --reference "nuget:PackageName,1.2.3"

# 最小版本
dotnet-exec script.cs --reference "nuget:PackageName,1.2.3-*"

# 版本范围
dotnet-exec script.cs --reference "nuget:PackageName,[1.0.0,2.0.0)"

# 预发布版本
dotnet-exec script.cs --reference "nuget:Microsoft.EntityFrameworkCore,8.0.0-preview.1"
```

### 私有 NuGet 源

```sh
# 添加私有源
dotnet-exec script.cs \
  --reference "nuget:MyCompany.Package" \
  --nuget-source https://nuget.company.com/v3/index.json

# 多个源
dotnet-exec script.cs \
  --reference "nuget:PublicPackage" \
  --reference "nuget:PrivatePackage" \
  --nuget-source https://api.nuget.org/v3/index.json \
  --nuget-source https://nuget.company.com/v3/index.json

# 带认证的源
dotnet-exec script.cs \
  --reference "nuget:SecurePackage" \
  --nuget-source https://secure-nuget.company.com/v3/index.json \
  --nuget-api-key "your-api-key"
```

### 包依赖解析

```sh
# 显示依赖树
dotnet-exec script.cs \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --show-dependencies

# 排除传递依赖
dotnet-exec script.cs \
  --reference "nuget:MainPackage" \
  --exclude-reference "nuget:UnwantedDependency"

# 强制版本解析
dotnet-exec script.cs \
  --reference "nuget:PackageA,1.0.0" \
  --reference "nuget:PackageB,2.0.0" \
  --force-version-resolution
```

## 本地文件和文件夹引用

### DLL 文件引用

```sh
# 单个 DLL
dotnet-exec script.cs --reference "./lib/MyLibrary.dll"

# 多个 DLL
dotnet-exec script.cs \
  --reference "./lib/Core.dll" \
  --reference "./lib/Utils.dll"

# 使用通配符
dotnet-exec script.cs --reference "./lib/*.dll"

# 递归搜索
dotnet-exec script.cs --reference "./lib/**/*.dll"
```

### 文件夹引用

```sh
# 引用整个文件夹
dotnet-exec script.cs --reference folder:./lib

# 递归文件夹引用
dotnet-exec script.cs --reference folder:./lib,recursive

# 带过滤器的文件夹引用
dotnet-exec script.cs --reference folder:./lib,*.Core.dll
```

### 相对路径处理

```sh
# 相对于脚本文件的路径
dotnet-exec ./scripts/main.cs --reference "../lib/helper.dll"

# 相对于当前工作目录
dotnet-exec script.cs --reference "./dependencies/library.dll"

# 绝对路径
dotnet-exec script.cs --reference "/absolute/path/to/library.dll"
```

## 项目引用

### 项目文件引用

```sh
# 引用项目文件
dotnet-exec script.cs --reference project:../MyLibrary/MyLibrary.csproj

# 多个项目
dotnet-exec script.cs \
  --reference project:../Core/Core.csproj \
  --reference project:../Utils/Utils.csproj

# 解决方案中的项目
dotnet-exec script.cs --reference project:MyLibrary
```

### 项目依赖继承

```sh
# 继承项目的所有依赖
dotnet-exec script.cs --project ./MyProject.csproj

# 选择性继承
dotnet-exec script.cs \
  --project ./MyProject.csproj \
  --inherit-references PackageReferences \
  --exclude-reference "nuget:UnwantedPackage"
```

### 项目配置

```sh
# 指定项目配置
dotnet-exec script.cs \
  --project ./MyProject.csproj \
  --project-configuration Release

# 指定目标框架
dotnet-exec script.cs \
  --project ./MyProject.csproj \
  --target-framework net8.0

# 项目属性覆盖
dotnet-exec script.cs \
  --project ./MyProject.csproj \
  --project-property "Configuration=Debug" \
  --project-property "OutputPath=./custom-output"
```

## 框架引用

### .NET 框架引用

```sh
# ASP.NET Core
dotnet-exec script.cs --web
# 等同于：--framework Microsoft.AspNetCore.App

# Windows 桌面
dotnet-exec script.cs --framework Microsoft.WindowsDesktop.App

# 完整的 .NET 框架
dotnet-exec script.cs --framework Microsoft.NETCore.App
```

### 自定义框架

```sh
# 自定义框架包
dotnet-exec script.cs --framework MyCompany.CustomFramework

# 带版本的框架
dotnet-exec script.cs --framework MyFramework,2.1.0

# 多个框架引用
dotnet-exec script.cs \
  --framework Microsoft.AspNetCore.App \
  --framework Microsoft.Extensions.Hosting
```

### 框架隐式引用

```sh
# 显示框架自动引入的命名空间
dotnet-exec script.cs --web --show-implicit-usings

# 禁用隐式引用
dotnet-exec script.cs --web --disable-implicit-usings

# 自定义隐式引用
dotnet-exec script.cs \
  --framework MyFramework \
  --implicit-using "MyFramework.Core" \
  --implicit-using "MyFramework.Extensions"
```

## 高级引用场景

### 条件引用

```sh
# 基于操作系统的条件引用
dotnet-exec script.cs \
  --reference "nuget:System.Management" \
  --condition "RuntimeInformation.IsOSPlatform(OSPlatform.Windows)"

# 基于框架版本的条件引用
dotnet-exec script.cs \
  --reference "nuget:Microsoft.Extensions.Hosting.WindowsServices" \
  --condition "net6.0-or-greater"
```

### 引用别名

```sh
# 为引用创建别名
dotnet-exec script.cs \
  --reference "nuget:OldLibrary,alias=OldLib" \
  --reference "nuget:NewLibrary,alias=NewLib"

# 在代码中使用别名
# extern alias OldLib;
# extern alias NewLib;
```

### 全局程序集缓存 (GAC)

```sh
# Windows GAC 引用
dotnet-exec script.cs --reference "gac:System.Web"

# 强名称引用
dotnet-exec script.cs --reference "gac:MyLibrary,Version=1.0.0.0,Culture=neutral,PublicKeyToken=abc123"
```

## 最佳实践

### 引用组织

```sh
# 按类型分组引用
dotnet-exec script.cs \
  --reference "nuget:Newtonsoft.Json" \
  --reference "nuget:Dapper" \
  --reference project:../Core/Core.csproj \
  --reference "./lib/LocalHelper.dll"

# 使用配置文件管理复杂引用
dotnet-exec config set-profile web-dev \
  --web \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --reference "nuget:Serilog.AspNetCore" \
  --reference project:../Shared/Shared.csproj
```

### 版本管理

```sh
# 锁定关键依赖版本
dotnet-exec script.cs \
  --reference "nuget:CriticalPackage,1.2.3" \
  --reference "nuget:FlexiblePackage,2.*"

# 使用中央包管理
dotnet-exec script.cs \
  --project ./MyProject.csproj \
  --central-package-management
```

### 性能优化

```sh
# 并行包下载
dotnet-exec script.cs \
  --reference "nuget:Package1" \
  --reference "nuget:Package2" \
  --parallel-downloads 4

# 引用缓存
dotnet-exec script.cs \
  --reference "nuget:LargePackage" \
  --cache-references

# 预热缓存
dotnet-exec --warm-cache \
  --reference "nuget:CommonPackage1" \
  --reference "nuget:CommonPackage2"
```

## 故障排除

### 常见问题

#### 包未找到

```sh
# 清除 NuGet 缓存
dotnet nuget locals all --clear

# 指定包源
dotnet-exec script.cs \
  --reference "nuget:PackageName" \
  --nuget-source https://api.nuget.org/v3/index.json

# 检查包是否存在
dotnet package search PackageName
```

#### 版本冲突

```sh
# 显示依赖冲突
dotnet-exec script.cs \
  --reference "nuget:PackageA" \
  --reference "nuget:PackageB" \
  --show-dependency-conflicts

# 强制版本解析
dotnet-exec script.cs \
  --reference "nuget:ConflictingPackage,2.0.0" \
  --force-version "ConflictingDependency,1.5.0"
```

#### 平台兼容性

```sh
# 检查平台兼容性
dotnet-exec script.cs \
  --reference "nuget:PlatformSpecificPackage" \
  --check-platform-compatibility

# 指定运行时标识符
dotnet-exec script.cs \
  --reference "nuget:NativePackage" \
  --runtime win-x64
```

### 调试引用问题

```sh
# 详细引用解析日志
dotnet-exec script.cs \
  --reference "nuget:ProblematicPackage" \
  --verbose-reference-resolution

# 输出引用信息
dotnet-exec script.cs \
  --reference "nuget:MyPackage" \
  --output-references ./references.json

# 检查引用路径
dotnet-exec script.cs \
  --reference "./lib/library.dll" \
  --validate-references
```

### 网络问题

```sh
# 离线模式
dotnet-exec script.cs \
  --reference "nuget:CachedPackage" \
  --offline

# 代理设置
HTTP_PROXY=http://proxy.company.com:8080 \
dotnet-exec script.cs --reference "nuget:Package"

# 超时配置
dotnet-exec script.cs \
  --reference "nuget:SlowPackage" \
  --download-timeout 300
```

这个全面的引用管理系统使 dotnet-exec 能够处理从简单脚本到复杂企业应用程序的各种引用需求。