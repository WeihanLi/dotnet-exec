# 架构

dotnet-exec 采用模块化、可扩展的架构，支持各种执行场景和编译策略。

### 核心组件

#### 1. 命令处理器（`CommandHandler.cs`）

主要协调器，负责：
- 处理命令行参数
- 确定执行模式（REPL vs 脚本执行）
- 协调编译和执行管道

#### 2. 引用解析（`ReferenceResolver`）

处理所有类型的引用：
- **NuGet 包解析**：下载和解析 NuGet 包
- **框架引用**：.NET 框架程序集
- **本地文件引用**：DLL 文件和项目引用
- **依赖解析**：传递依赖处理

#### 3. 编译系统

**编译器工厂模式**：
- `ICompilerFactory`：创建适当的编译器实例
- 多个编译器实现支持不同场景
- 支持不同 .NET 版本和编译选项

**脚本选项配置**：
- 语言版本选择
- 优化级别
- 不安全代码支持
- 全局 using 语句

#### 4. 执行系统

**执行器工厂模式**：
- `IExecutorFactory`：创建适当的执行器实例
- 支持不同执行环境
- 入口点解析（Main、自定义方法）

#### 5. REPL 系统（`Repl.cs`）

**交互执行引擎**：
- 基于 Microsoft.CodeAnalysis.CSharp.Scripting 构建
- 语句间状态持久化
- 动态引用加载
- 补全服务集成

**脚本状态管理**：
- 维护执行上下文
- 变量和类型定义
- 异常处理和恢复

### 架构模式

#### 1. 管道模式

**选项配置管道**：
```
原始选项 → 预配置 → 脚本获取 → 配置 → 编译选项
```

**中间件组件**：
- `AliasOptionsPreConfigureMiddleware`：处理命令别名
- `ProjectFileOptionsConfigureMiddleware`：处理项目文件引用
- 可扩展的中间件系统用于自定义处理

#### 2. 工厂模式

**编译器工厂**：
- 抽象编译策略选择
- 支持多个编译后端
- 便于扩展新的编译场景

**执行器工厂**：
- 抽象执行策略选择
- 支持不同运行时环境
- 处理入口点解析

#### 3. 服务层架构

**依赖注入**：
- 所有服务在 DI 容器中注册
- 基于接口的设计提高可测试性
- 作用域服务生命周期

**核心服务**：
- `IScriptContentFetcher`：从各种来源检索脚本内容
- `IConfigProfileManager`：管理配置文件
- `IScriptCompletionService`：提供类似 IntelliSense 的补全
- `IUriTransformer`：处理 URL 转换和快捷方式

### 数据流

#### 脚本执行流程

```
1. 命令行解析
   ↓
2. 选项绑定和配置文件加载
   ↓
3. 预配置管道
   ↓
4. 脚本内容获取
   ↓
5. 配置管道
   ↓
6. 引用解析
   ↓
7. 编译
   ↓
8. 程序集执行
```

#### REPL 流程

```
1. REPL 初始化
   ↓
2. 脚本选项设置
   ↓
3. 交互循环：
   - 读取输入
   - 解析命令
   - 处理特殊命令
   - 编译和执行
   - 显示结果
   - 更新状态
```

### 扩展点

#### 1. 自定义中间件

实现 `IOptionsPreConfigureMiddleware` 或 `IOptionsConfigureMiddleware`：

```csharp
public class CustomMiddleware : IOptionsConfigureMiddleware
{
    public Task Execute(ExecOptions options)
    {
        // 自定义选项处理逻辑
        return Task.CompletedTask;
    }
}
```

#### 2. 自定义编译器

实现 `ICompiler` 接口：

```csharp
public class CustomCompiler : ICompiler
{
    public async Task<Result<CompileResult>> Compile(ExecOptions options, string sourceText)
    {
        // 自定义编译逻辑
    }
}
```

#### 3. 自定义执行器

实现 `IExecutor` 接口：

```csharp
public class CustomExecutor : IExecutor
{
    public async Task<int> Execute(ExecOptions options, CompileResult compileResult)
    {
        // 自定义执行逻辑
    }
}
```

### 性能考虑

#### 1. 编译缓存

- 基于源代码内容哈希的程序集缓存
- 引用解析缓存
- 元数据引用重用

#### 2. 引用解析优化

- 并行包下载
- 本地缓存利用
- 增量解析

#### 3. REPL 优化

- 脚本状态重用
- 增量编译
- 长期运行会话的内存管理

### 安全模型

#### 1. 代码执行

- 默认无沙箱
- 完整的 .NET 运行时功能
- 用户负责代码安全

#### 2. 引用解析

- NuGet 包验证
- 本地文件访问控制
- 远程包的网络访问

#### 3. REPL 安全

- 与脚本执行相同的安全模型
- 动态引用加载能力
- 会话隔离

### 集成场景

#### 1. 构建系统

- MSBuild 集成
- GitHub Actions 工作流
- Docker 容器执行

#### 2. 开发工具

- IDE 集成模式
- 笔记本式开发
- 调试功能

#### 3. 自动化脚本

- CI/CD 管道集成
- 系统管理任务
- 数据处理工作流

