# 为 Language Server Framework 编写测试的指南

## 概述

这个测试框架已经为项目中的主要协议 handler 创建了测试模板。但由于框架的复杂性,需要根据实际的类型定义进行调整。

## 当前状态

已创建测试类(需要修正):
- ✅ CompletionHandlerTests
- ✅ HoverHandlerTests
- ✅ DefinitionHandlerTests
- ✅ DeclarationHandlerTests
- ✅ ImplementationHandlerTests
- ✅ TypeDefinitionHandlerTests
- ✅ ReferenceHandlerTests
- ✅ DocumentHighlightHandlerTests
- ✅ DocumentSymbolHandlerTests
- ✅ CodeActionHandlerTests
- ✅ CodeLensHandlerTests
- ✅ DocumentLinkHandlerTests
- ✅ DocumentColorHandlerTests
- ✅ DocumentFormattingHandlerTests
- ✅ RenameHandlerTests
- ✅ FoldingRangeHandlerTests
- ✅ SelectionRangeHandlerTests
- ✅ CallHierarchyHandlerTests
- ✅ SemanticTokensHandlerTests
- ✅ InlayHintHandlerTests
- ✅ SignatureHelpHandlerTests
- ✅ ExecuteCommandHandlerTests
- ✅ TextDocumentHandlerTests

## 需要修正的主要问题

### 1. 类型导入问题

许多类型需要从正确的命名空间导入:

```csharp
// 需要添加:
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
```

### 2. Response 类型结构

许多 Response 类使用属性而不是独立的集合类:

```csharp
// 错误:
response.Locations[0]

// 正确 (可能的实现):
response.Value // 或其他属性名
```

### 3. Union 类型

一些属性使用 Union 类型,需要特殊处理:

```csharp
// 例如 CodeActionProvider 可能是:
BooleanOr<CodeActionOptions>

// 需要这样访问:
if (serverCapabilities.CodeActionProvider.HasValue)
{
    var options = serverCapabilities.CodeActionProvider.Value;
}
```

### 4. 构造函数参数

一些类型有必需的构造函数参数:

```csharp
// 检查类定义:
public class ExecuteCommandResponse(LSPAny? result)

// 调用时:
new ExecuteCommandResponse(result: myResult)
```

## 如何修正测试

### 步骤 1: 查看实际的类型定义

对于每个要测试的 handler,首先查看:

1. Handler 基类的方法签名
2. Request 和 Response 类型的定义
3. 相关的 Model 类型

示例:

```csharp
// 1. 查看 handler
public abstract class CompletionHandlerBase : IJsonHandler
{
    protected abstract Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken token);
}

// 2. 查看 Response 类型
public class CompletionResponse(List<CompletionItem> items)
{
    public List<CompletionItem> Items => items;
}

// 3. 相应地编写测试
```

### 步骤 2: 修正类型引用

确保所有使用的类型都已正确导入:

```csharp
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;
```

### 步骤 3: 修正 Response 访问

根据实际的 Response 类结构访问数据:

```csharp
// 查看 Response 类定义来确定正确的属性名
var result = await handler.Handle(request, token);
Assert.NotNull(result);
Assert.NotEmpty(result.Items); // 或 result.Value, result.Locations 等
```

### 步骤 4: 处理 Union 类型

对于 Union 类型的属性,使用适当的方法:

```csharp
// 对于 BooleanOr<T> 类型:
serverCapabilities.SomeProvider = new BooleanOr<SomeOptions>(new SomeOptions { ... });

// 或者只是 bool:
serverCapabilities.SomeProvider = true;
```

## 运行单个测试

在修正每个测试类后,可以单独运行它们:

```bash
# 运行单个测试类
dotnet test --filter "FullyQualifiedName~CompletionHandlerTests"

# 运行单个测试方法
dotnet test --filter "FullyQualifiedName~CompletionHandlerTests.Handle_ShouldReturnCompletionItems"
```

## 建议的修正顺序

1. 从简单的 handler 开始 (如 HoverHandler)
2. 修正一个,运行测试验证
3. 逐步处理更复杂的 (如 SemanticTokensHandler, CallHierarchyHandler)

## 示例:修正 CompletionHandlerTests

```csharp
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Framework.Tests.TestBase;
using FluentAssertions;
using Xunit;

namespace EmmyLua.LanguageServer.Framework.Tests.Handlers;

public class CompletionHandlerTests : TestHandlerBase
{
    private class TestCompletionHandler : CompletionHandlerBase
    {
        protected override Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken token)
        {
            // 根据实际的 CompletionResponse 构造函数
            var items = new List<CompletionItem>
            {
                new CompletionItem { Label = "test", Kind = CompletionItemKind.Method }
            };
            return Task.FromResult<CompletionResponse?>(new CompletionResponse(items));
        }

        protected override Task<CompletionItem> Resolve(CompletionItem item, CancellationToken token)
        {
            return Task.FromResult(item);
        }

        public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
        {
            // 根据实际的类型设置
            serverCapabilities.CompletionProvider = /* 正确的值 */;
        }
    }

    // ... 测试方法
}
```

## 额外资源

- LSP 规范: https://microsoft.github.io/language-server-protocol/
- 项目源码中的 Protocol/Message 目录包含所有消息类型定义
- 项目源码中的 Server/Handler 目录包含所有 handler 基类

## 贡献

如果你成功修正了某个测试类,欢迎提交 PR!请确保:
1. 测试能够编译通过
2. 测试能够成功运行
3. 添加适当的注释说明任何特殊处理
