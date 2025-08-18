# 配置文件和别名指南

本指南介绍如何使用配置文件和命令别名来简化和自动化 dotnet-exec 工作流程。

## 配置文件管理

### 创建配置文件

配置文件让您可以保存和重用常用的配置组合：

```sh
# 创建基本配置文件
dotnet-exec config set-profile myprofile \
  --reference "nuget:Newtonsoft.Json" \
  --using "Newtonsoft.Json"

# 创建 Web 开发配置文件
dotnet-exec config set-profile web-dev \
  --web \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --reference "nuget:Serilog.AspNetCore" \
  --using "Microsoft.EntityFrameworkCore" \
  --using "Serilog"

# 创建测试配置文件
dotnet-exec config set-profile testing \
  --reference "nuget:xunit" \
  --reference "nuget:Moq" \
  --reference "nuget:FluentAssertions" \
  --using "Moq" \
  --using "FluentAssertions"
```

### 使用配置文件

```sh
# 使用配置文件执行脚本
dotnet-exec script.cs --profile myprofile

# 在 REPL 中使用配置文件
dotnet-exec --profile web-dev

# 组合配置文件和额外选项
dotnet-exec script.cs --profile myprofile --reference "nuget:ExtraPackage"
```

### 配置文件管理命令

```sh
# 列出所有配置文件
dotnet-exec config list-profiles

# 显示配置文件详情
dotnet-exec config show-profile myprofile

# 更新配置文件
dotnet-exec config update-profile myprofile \
  --reference "nuget:AdditionalPackage"

# 删除配置文件
dotnet-exec config remove-profile myprofile

# 重命名配置文件
dotnet-exec config rename-profile old-name new-name
```

## 高级配置文件

### 环境特定配置

```sh
# 开发环境配置
dotnet-exec config set-profile dev \
  --reference "nuget:Microsoft.Extensions.Logging.Debug" \
  --reference "nuget:Microsoft.Extensions.DependencyInjection" \
  --using "Microsoft.Extensions.Logging" \
  --using "Microsoft.Extensions.DependencyInjection" \
  --property "Environment=Development" \
  --property "LogLevel=Debug"

# 生产环境配置
dotnet-exec config set-profile prod \
  --reference "nuget:Microsoft.Extensions.Logging.EventLog" \
  --using "Microsoft.Extensions.Logging" \
  --property "Environment=Production" \
  --property "LogLevel=Warning" \
  --configuration Release

# 测试环境配置
dotnet-exec config set-profile test \
  --reference "nuget:Microsoft.AspNetCore.Mvc.Testing" \
  --reference "nuget:Testcontainers" \
  --using "Microsoft.AspNetCore.Mvc.Testing" \
  --property "Environment=Testing"
```

### 项目特定配置

```sh
# 微服务项目配置
dotnet-exec config set-profile microservice \
  --web \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --reference "nuget:Microsoft.AspNetCore.Authentication.JwtBearer" \
  --reference "nuget:Swashbuckle.AspNetCore" \
  --reference "nuget:Serilog.AspNetCore" \
  --using "Microsoft.EntityFrameworkCore" \
  --using "Microsoft.AspNetCore.Authentication.JwtBearer" \
  --using "Serilog"

# 数据处理配置
dotnet-exec config set-profile data-processing \
  --reference "nuget:CsvHelper" \
  --reference "nuget:Microsoft.Data.SqlClient" \
  --reference "nuget:Dapper" \
  --reference "nuget:System.Text.Json" \
  --using "CsvHelper" \
  --using "Microsoft.Data.SqlClient" \
  --using "Dapper" \
  --using "System.Text.Json"

# DevOps 自动化配置
dotnet-exec config set-profile devops \
  --reference "nuget:System.Management" \
  --reference "nuget:Microsoft.Extensions.Configuration" \
  --reference "nuget:Microsoft.Extensions.Configuration.Json" \
  --using "System.Management" \
  --using "Microsoft.Extensions.Configuration"
```

### 配置文件继承

```sh
# 基础配置文件
dotnet-exec config set-profile base \
  --reference "nuget:Microsoft.Extensions.Logging" \
  --reference "nuget:Microsoft.Extensions.Configuration" \
  --using "Microsoft.Extensions.Logging" \
  --using "Microsoft.Extensions.Configuration"

# 继承基础配置的 Web 配置
dotnet-exec config set-profile web-extended \
  --inherit-profile base \
  --web \
  --reference "nuget:Microsoft.AspNetCore.Mvc" \
  --using "Microsoft.AspNetCore.Mvc"

# 继承并覆盖配置
dotnet-exec config set-profile web-custom \
  --inherit-profile web-extended \
  --override-reference "nuget:Microsoft.Extensions.Logging.Debug" \
  --using "Microsoft.AspNetCore.Authorization"
```

## 命令别名

### 创建别名

别名提供了一种创建自定义命令快捷方式的方法：

```sh
# 简单别名
dotnet-exec alias set json \
  --reference "nuget:Newtonsoft.Json" \
  --using "Newtonsoft.Json"

# Web 开发别名
dotnet-exec alias set web-script \
  --web \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --using "Microsoft.EntityFrameworkCore"

# 测试别名
dotnet-exec alias set test-script \
  --reference "nuget:FluentAssertions" \
  --using "FluentAssertions"
```

### 使用别名

```sh
# 使用别名执行脚本
dotnet-exec json my-json-script.cs

# 使用别名启动 REPL
dotnet-exec web-script

# 组合别名和额外参数
dotnet-exec json my-script.cs --reference "nuget:CsvHelper"
```

### 别名管理

```sh
# 列出所有别名
dotnet-exec alias list

# 显示别名详情
dotnet-exec alias show json

# 更新别名
dotnet-exec alias update json \
  --reference "nuget:System.Text.Json"

# 删除别名
dotnet-exec alias remove json

# 重命名别名
dotnet-exec alias rename old-alias new-alias
```

### 复杂别名示例

```sh
# API 测试别名
dotnet-exec alias set api-test \
  --reference "nuget:RestSharp" \
  --reference "nuget:xunit" \
  --reference "nuget:Microsoft.AspNetCore.Mvc.Testing" \
  --using "RestSharp" \
  --using "Xunit" \
  --using "Microsoft.AspNetCore.Mvc.Testing"

# 数据库脚本别名
dotnet-exec alias set db-script \
  --reference "nuget:Microsoft.Data.SqlClient" \
  --reference "nuget:Dapper" \
  --reference "nuget:Microsoft.Extensions.Configuration" \
  --using "Microsoft.Data.SqlClient" \
  --using "Dapper" \
  --using "Microsoft.Extensions.Configuration"

# Docker 管理别名
dotnet-exec alias set docker-mgmt \
  --reference "nuget:Docker.DotNet" \
  --reference "nuget:Microsoft.Extensions.Logging" \
  --using "Docker.DotNet" \
  --using "Docker.DotNet.Models" \
  --using "Microsoft.Extensions.Logging"
```

## 团队协作配置

### 导出和导入配置

```sh
# 导出单个配置文件
dotnet-exec config export --profile web-dev --output web-dev-profile.json

# 导出所有配置文件
dotnet-exec config export-all --output team-configs.json

# 导出别名
dotnet-exec alias export --output team-aliases.json

# 导入配置文件
dotnet-exec config import --file web-dev-profile.json

# 导入并合并
dotnet-exec config import --file team-configs.json --merge

# 导入别名
dotnet-exec alias import --file team-aliases.json
```

### 版本控制中的配置

```sh
# 在项目根目录创建配置目录
mkdir .dotnet-exec

# 导出项目配置
dotnet-exec config export --profile project-config \
  --output .dotnet-exec/project-config.json

# 导出项目别名
dotnet-exec alias export \
  --output .dotnet-exec/project-aliases.json

# 团队成员导入配置
dotnet-exec config import --file .dotnet-exec/project-config.json
dotnet-exec alias import --file .dotnet-exec/project-aliases.json
```

### 配置文件模板

```sh
# 创建配置模板
dotnet-exec profile set web-app \
  --web \
  --reference "nuget:Microsoft.EntityFrameworkCore.SqlServer" \
  --reference "nuget:Microsoft.AspNetCore.Authentication.JwtBearer" \
  --reference "nuget:Serilog.AspNetCore" \
  --reference "nuget:FluentValidation.AspNetCore"

# 基于模板创建配置文件
dotnet-exec profile set web-app my-web-project \
  --reference "-nuget:Microsoft.EntityFrameworkCore.PostgreSQL" \
  --reference "nuget:Npgsql.EntityFrameworkCore.PostgreSQL"
```

## CI/CD 集成

### GitHub Actions

```yaml
name: Run Scripts with dotnet-exec

on: [push, pull_request]

jobs:
  run-scripts:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Install dotnet-exec
      run: dotnet tool install -g dotnet-execute
    
    - name: Import team configuration
      run: dotnet-exec config import --file .dotnet-exec/ci-config.json
    
    - name: Run build script
      run: dotnet-exec build-script scripts/build.cs --profile ci-build
    
    - name: Run tests
      run: dotnet-exec test-script scripts/integration-tests.cs
```

### Azure DevOps

```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '8.0.x'

- script: dotnet tool install -g dotnet-execute
  displayName: 'Install dotnet-exec'

- script: dotnet-exec config import --file .dotnet-exec/azure-config.json
  displayName: 'Import Azure configuration'

- script: dotnet-exec deployment scripts/deploy.cs --profile azure-deploy
  displayName: 'Run deployment script'
```

## 最佳实践

### 配置文件命名

```sh
# 环境前缀
dotnet-exec config set-profile dev-web-api
dotnet-exec config set-profile prod-web-api
dotnet-exec config set-profile test-web-api

# 功能分组
dotnet-exec config set-profile auth-service
dotnet-exec config set-profile data-service
dotnet-exec config set-profile notification-service

# 团队约定
dotnet-exec config set-profile team-frontend
dotnet-exec config set-profile team-backend
dotnet-exec config set-profile team-devops
```

### 配置分层

```sh
# 基础层
dotnet-exec config set-profile base-company \
  --reference "nuget:CompanyCore" \
  --reference "nuget:CompanyLogging" \
  --using "Company.Core" \
  --using "Company.Logging"

# 应用层
dotnet-exec config set-profile app-web \
  --inherit-profile base-company \
  --web \
  --reference "nuget:CompanyWeb"

# 特定功能层
dotnet-exec config set-profile feature-auth \
  --inherit-profile app-web \
  --reference "nuget:CompanyAuth" \
  --using "Company.Auth"
```

### 安全注意事项

```sh
# 避免在配置文件中存储敏感信息
# 使用环境变量或密钥管理
dotnet-exec config set-profile secure-db \
  --reference "nuget:Microsoft.Data.SqlClient" \
  --property "UseEnvironmentVariables=true"

# 在脚本中使用环境变量
# var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
```

配置文件和别名系统使 dotnet-exec 成为一个强大的工作流程自动化工具，特别适合团队协作和复杂项目管理。