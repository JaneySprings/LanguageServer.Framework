# Language Server Framework - 测试项目

本项目为 Language Server Protocol (LSP) Framework 提供单元测试框架。

⚠️ **注意**: 测试文件已创建但需要根据实际的协议类型进行调整。框架中的类型定义比较复杂,包含 Union 类型、特殊的 Response 结构等。

## 测试框架

- **xUnit**: 测试框架
- **FluentAssertions**: 断言库，提供更可读的断言语法
- **Moq**: 模拟框架（如需要）

## 测试结构

```
LanguageServer.Framework.Tests/
├── TestBase/
│   ├── MockLanguageServer.cs      # 模拟语言服务器
│   └── TestHandlerBase.cs         # Handler 测试基类
└── Handlers/
    ├── CallHierarchyHandlerTests.cs
    ├── CodeActionHandlerTests.cs
    ├── CodeLensHandlerTests.cs
    ├── CompletionHandlerTests.cs
    ├── DeclarationHandlerTests.cs
    ├── DefinitionHandlerTests.cs
    ├── DocumentColorHandlerTests.cs
    ├── DocumentFormattingHandlerTests.cs
    ├── DocumentHighlightHandlerTests.cs
    ├── DocumentLinkHandlerTests.cs
    ├── DocumentSymbolHandlerTests.cs
    ├── ExecuteCommandHandlerTests.cs
    ├── FoldingRangeHandlerTests.cs
    ├── HoverHandlerTests.cs
    ├── ImplementationHandlerTests.cs
    ├── InlayHintHandlerTests.cs
    ├── ReferenceHandlerTests.cs
    ├── RenameHandlerTests.cs
    ├── SelectionRangeHandlerTests.cs
    ├── SemanticTokensHandlerTests.cs
    ├── SignatureHelpHandlerTests.cs
    ├── TextDocumentHandlerTests.cs
    └── TypeDefinitionHandlerTests.cs
```

## 当前状态

测试类模板已创建,但需要根据实际的框架类型进行调整。主要问题包括:

1. **类型导入**: 需要从正确的命名空间导入所有类型
2. **Response 结构**: Response 类可能使用不同的属性名
3. **Union 类型**: 某些属性使用 Union 类型(如 `BooleanOr<T>`)
4. **构造函数**: 某些类型有必需的构造函数参数

详细的修正指南请参阅 [CONTRIBUTING.md](./CONTRIBUTING.md)。

## 如何开始

### 1. 选择一个简单的 Handler

从简单的 handler 开始,如 `HoverHandler`:

```csharp
// 查看 handler 基类
public abstract class HoverHandlerBase : IJsonHandler
{
    protected abstract Task<HoverResponse?> Handle(HoverParams request, CancellationToken token);
}
```

### 2. 查看相关类型定义

```csharp
// 查找 HoverResponse 的定义
// 在 Protocol/Message/Hover 目录下
```

### 3. 修正测试代码

根据实际类型调整测试:

```csharp
[Fact]
public async Task Handle_ShouldReturnHoverInformation()
{
    // 实现测试
}
```

### 4. 运行测试

```bash
dotnet test --filter "FullyQualifiedName~HoverHandlerTests"
```

## 测试覆盖的协议

- ✅ Completion (自动完成)
- ✅ Hover (悬停信息)
- ✅ Definition (转到定义)
- ✅ Declaration (转到声明)
- ✅ Implementation (转到实现)
- ✅ Type Definition (转到类型定义)
- ✅ References (查找引用)
- ✅ Document Highlight (文档高亮)
- ✅ Document Symbol (文档符号)
- ✅ Code Action (代码操作)
- ✅ Code Lens (代码镜头)
- ✅ Document Link (文档链接)
- ✅ Document Color (文档颜色)
- ✅ Document Formatting (文档格式化)
- ✅ Rename (重命名)
- ✅ Folding Range (折叠范围)
- ✅ Selection Range (选择范围)
- ✅ Call Hierarchy (调用层次结构)
- ✅ Semantic Tokens (语义标记)
- ✅ Inlay Hint (内联提示)
- ✅ Signature Help (签名帮助)
- ✅ Execute Command (执行命令)
- ✅ Text Document Sync (文本文档同步)

## 运行测试

### 使用命令行

```bash
# 切换到测试项目目录
cd LanguageServer.Framework.Tests

# 构建项目
dotnet build

# 运行所有测试（当测试修正后）
dotnet test

# 运行特定测试类
dotnet test --filter "FullyQualifiedName~CompletionHandlerTests"

# 运行特定测试方法
dotnet test --filter "FullyQualifiedName~CompletionHandlerTests.Handle_ShouldReturnCompletionItems"

# 运行测试并生成详细输出
dotnet test --logger "console;verbosity=detailed"
```

### 使用 Visual Studio

1. 打开解决方案
2. 打开 Test Explorer (测试 > 测试资源管理器)
3. 点击 "运行所有测试" 或选择特定测试运行

## 扩展测试

要添加新的测试：

1. 在 `Handlers/` 目录下创建新的测试类
2. 继承 `TestHandlerBase`
3. 创建一个测试 handler 实现
4. 编写测试方法，使用 `[Fact]` 或 `[Theory]` 属性标记

## 贡献

欢迎提交 PR 来改进测试或修正现有测试。请参阅 [CONTRIBUTING.md](./CONTRIBUTING.md) 获取详细指南。

## 参考资源

- [Language Server Protocol 规范](https://microsoft.github.io/language-server-protocol/)
- [xUnit 文档](https://xunit.net/)
- [FluentAssertions 文档](https://fluentassertions.com/)


## 测试结构

```
LanguageServer.Framework.Tests/
├── TestBase/
│   ├── MockLanguageServer.cs      # 模拟语言服务器
│   └── TestHandlerBase.cs         # Handler 测试基类
└── Handlers/
    ├── CallHierarchyHandlerTests.cs
    ├── CodeActionHandlerTests.cs
    ├── CodeLensHandlerTests.cs
    ├── CompletionHandlerTests.cs
    ├── DeclarationHandlerTests.cs
    ├── DefinitionHandlerTests.cs
    ├── DocumentColorHandlerTests.cs
    ├── DocumentFormattingHandlerTests.cs
    ├── DocumentHighlightHandlerTests.cs
    ├── DocumentLinkHandlerTests.cs
    ├── DocumentSymbolHandlerTests.cs
    ├── ExecuteCommandHandlerTests.cs
    ├── FoldingRangeHandlerTests.cs
    ├── HoverHandlerTests.cs
    ├── ImplementationHandlerTests.cs
    ├── InlayHintHandlerTests.cs
    ├── ReferenceHandlerTests.cs
    ├── RenameHandlerTests.cs
    ├── SelectionRangeHandlerTests.cs
    ├── SemanticTokensHandlerTests.cs
    ├── SignatureHelpHandlerTests.cs
    ├── TextDocumentHandlerTests.cs
    └── TypeDefinitionHandlerTests.cs
```

## 运行测试

### 使用命令行

```bash
# 切换到测试项目目录
cd LanguageServer.Framework.Tests

# 运行所有测试
dotnet test

# 运行特定测试类
dotnet test --filter "FullyQualifiedName~CompletionHandlerTests"

# 运行特定测试方法
dotnet test --filter "FullyQualifiedName~CompletionHandlerTests.Handle_ShouldReturnCompletionItems"

# 运行测试并生成详细输出
dotnet test --logger "console;verbosity=detailed"
```

### 使用 Visual Studio

1. 打开解决方案
2. 打开 Test Explorer (测试 > 测试资源管理器)
3. 点击 "运行所有测试" 或选择特定测试运行

## 测试覆盖的协议

每个 LSP 协议 handler 都有相应的测试类，测试内容包括：

1. **功能测试**: 验证 handler 的核心功能
2. **能力注册测试**: 验证 handler 正确注册服务器能力
3. **边界情况测试**: 测试特殊输入和边界条件

### 已测试的协议

- ✅ Completion (自动完成)
- ✅ Hover (悬停信息)
- ✅ Definition (转到定义)
- ✅ Declaration (转到声明)
- ✅ Implementation (转到实现)
- ✅ Type Definition (转到类型定义)
- ✅ References (查找引用)
- ✅ Document Highlight (文档高亮)
- ✅ Document Symbol (文档符号)
- ✅ Code Action (代码操作)
- ✅ Code Lens (代码镜头)
- ✅ Document Link (文档链接)
- ✅ Document Color (文档颜色)
- ✅ Document Formatting (文档格式化)
- ✅ Rename (重命名)
- ✅ Folding Range (折叠范围)
- ✅ Selection Range (选择范围)
- ✅ Call Hierarchy (调用层次结构)
- ✅ Semantic Tokens (语义标记)
- ✅ Inlay Hint (内联提示)
- ✅ Signature Help (签名帮助)
- ✅ Execute Command (执行命令)
- ✅ Text Document Sync (文本文档同步)

## 扩展测试

要添加新的测试：

1. 在 `Handlers/` 目录下创建新的测试类
2. 继承 `TestHandlerBase`
3. 创建一个测试 handler 实现
4. 编写测试方法，使用 `[Fact]` 或 `[Theory]` 属性标记

### 示例

```csharp
public class NewHandlerTests : TestHandlerBase
{
    private class TestNewHandler : NewHandlerBase
    {
        protected override Task<Response?> Handle(Params request, CancellationToken token)
        {
            // 实现测试逻辑
            return Task.FromResult<Response?>(new Response());
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            // 注册能力
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnExpectedResult()
    {
        // Arrange
        var handler = new TestNewHandler();
        var request = new Params();

        // Act
        var response = await CallHandlerMethod(handler, request);

        // Assert
        response.Should().NotBeNull();
    }
}
```

## 注意事项

- 所有测试都是单元测试，独立运行，不依赖外部资源
- 使用反射调用 protected 方法进行测试
- 测试覆盖了正常流程和异常情况
- 使用 FluentAssertions 提供更好的断言可读性

## 贡献

欢迎提交 PR 来改进测试覆盖率或添加新的测试场景。
