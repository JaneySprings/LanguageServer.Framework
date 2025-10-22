# Language Server Framework

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/EmmyLua.LanguageServer.Framework.svg)](https://www.nuget.org/packages/EmmyLua.LanguageServer.Framework/)

A modern, high-performance .NET framework for building [Language Server Protocol (LSP)](https://microsoft.github.io/language-server-protocol/) servers with full LSP 3.18 specification support.

## ✨ Features

- 🎯 **Full LSP 3.18 Support** - Complete implementation of the latest LSP specification
- ⚡ **High Performance** - Built with System.Text.Json source generation for optimal performance
- 🚀 **AOT Ready** - Full support for Ahead-of-Time (AOT) compilation
- 📦 **Zero Dependencies** - No dependency on Newtonsoft.Json or other third-party libraries
- 🪟 **Window & Progress** - Complete support for window messages, logging, and progress reporting
- 🔧 **Easy to Use** - Simple, intuitive API for building language servers
- 📊 **Performance Metrics** - Built-in performance monitoring and telemetry support
- 🎨 **Type Safe** - Strongly typed protocol messages with full IntelliSense support

## 📦 Installation

Install via NuGet Package Manager:

```bash
dotnet add package EmmyLua.LanguageServer.Framework
```

Or via Package Manager Console:

```powershell
Install-Package EmmyLua.LanguageServer.Framework
```

## 🚀 Quick Start

### Basic Language Server

```csharp
using EmmyLua.LanguageServer.Framework.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Initialize;

// Create a language server
var server = LanguageServer.From(Console.OpenStandardInput(), Console.OpenStandardOutput());

// Handle initialization
server.OnInitialize((request, serverInfo) =>
{
    serverInfo.Name = "My Language Server";
    serverInfo.Version = "1.0.0";
    return Task.CompletedTask;
});

// Handle initialized notification
server.OnInitialized(async (request) =>
{
    await server.Client.LogInfo("Server initialized!");
});

// Run the server
await server.Run();
```

### Adding Handlers

```csharp
using EmmyLua.LanguageServer.Framework.Server.Handler;

// Create a hover handler
public class MyHoverHandler : HoverHandlerBase
{
    protected override Task<Hover?> Handle(HoverParams request, CancellationToken token)
    {
        var hover = new Hover
        {
            Contents = new MarkupContent
            {
                Kind = MarkupKind.Markdown,
                Value = "**Hello from LSP!**"
            }
        };
        return Task.FromResult<Hover?>(hover);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.HoverProvider = true;
    }
}

// Add handler to server
server.AddHandler(new MyHoverHandler());
```

## 📚 Documentation

### Window Features

Display messages and collect user input:

```csharp
// Show a message
await server.Client.ShowMessage(new ShowMessageParams
{
    Type = MessageType.Info,
    Message = "Operation completed successfully!"
});

// Request user action
var result = await server.Client.ShowMessageRequest(new ShowMessageRequestParams
{
    Type = MessageType.Warning,
    Message = "Do you want to continue?",
    Actions = new List<MessageActionItem>
    {
        new() { Title = "Yes" },
        new() { Title = "No" }
    }
}, cancellationToken);

// Log messages
await server.Client.LogError("An error occurred");
await server.Client.LogInfo("Processing file...");
```

### Progress Reporting

Report progress for long-running operations:

```csharp
// Using the helper class (recommended)
using var progress = await WorkDoneProgressReporter.Create(
    server.Client,
    title: "Analyzing workspace",
    cancellable: true
);

for (int i = 0; i < 100; i++)
{
    await progress.Report($"Processing file {i + 1}/100", (uint)i);
}

await progress.End("Analysis complete!");
```

For detailed examples, see [LSP_WINDOW_PROGRESS_USAGE.md](LSP_WINDOW_PROGRESS_USAGE.md)

### Performance Monitoring

Enable performance metrics to monitor your server:

```csharp
var options = new LanguageServerOptions
{
    EnablePerformanceTracing = true,
    PerformanceMetricsPrintInterval = TimeSpan.FromMinutes(5)
};

var server = LanguageServer.From(input, output, options);

// Get metrics at any time
var metrics = server.GetMetrics();
Console.WriteLine($"Total requests: {metrics.TotalRequestsHandled}");
Console.WriteLine($"Average duration: {metrics.AverageRequestDurationMs}ms");
```

## 🎯 Supported LSP Features

### Text Document Synchronization
- ✅DidOpen / DidChange / DidClose / DidSave
- ✅ WillSave / WillSaveWaitUntil

### Language Features
- ✅ Completion (with resolve support)
- ✅ Hover
- ✅ Signature Help
- ✅ Definition / Declaration / Type Definition / Implementation
- ✅ References
- ✅ Document Highlight
- ✅ Document Symbol
- ✅ Code Action
- ✅ Code Lens
- ✅ Document Link
- ✅ Document Color / Color Presentation
- ✅ Document Formatting / Range Formatting / On Type Formatting
- ✅ Rename / Prepare Rename
- ✅ Folding Range
- ✅ Selection Range
- ✅ Call Hierarchy
- ✅ Semantic Tokens
- ✅ Inlay Hint
- ✅ Inline Value
- ✅ Type Hierarchy
- ✅ Inline Completion
- ✅ Linked Editing Range

### Workspace Features
- ✅ Workspace Symbols
- ✅ Configuration
- ✅ Workspace Folders
- ✅ File Operations (Create/Rename/Delete)
- ✅ File Watching
- ✅ Diagnostics
- ✅ Execute Command
- ✅ Apply Edit

### Window Features
- ✅ ShowMessage / ShowMessageRequest
- ✅ LogMessage
- ✅ Work Done Progress
- ✅ Telemetry

## 🏗️ Architecture

The framework is designed with extensibility and performance in mind:

```
┌─────────────────────────────────────────┐
│     Language Server Application         │
├─────────────────────────────────────────┤
│           Handler Layer                 │
│  (HoverHandler, CompletionHandler...)   │
├─────────────────────────────────────────┤
│         LanguageServer Core             │
│   (Protocol, Routing, Lifecycle)        │
├─────────────────────────────────────────┤
│      JSON-RPC Protocol Layer            │
│  (JsonProtocolReader/Writer)            │
├─────────────────────────────────────────┤
│         Transport Layer (stdio)         │
└─────────────────────────────────────────┘
```

## 📖 Examples

Check out the `LanguageServer.Test` project for a complete working example that demonstrates:
- Server initialization
- Multiple handlers implementation
- Configuration management
- Progress reporting
- Error handling

## 🌟 Projects Using This Framework

- [EmmyLuaAnalyzer](https://github.com/CppCXY/EmmyLuaAnalyzer) - A feature-rich Lua language server

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details on how to submit pull requests, report issues, and contribute to the project.

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Resources

- [LSP Specification 3.18](https://microsoft.github.io/language-server-protocol/specifications/lsp/3.18/specification/)
- [Language Server Protocol](https://microsoft.github.io/language-server-protocol/)
- [NuGet Package](https://www.nuget.org/packages/EmmyLua.LanguageServer.Framework/)

## 💬 Support

- 📫 Report bugs via [GitHub Issues](https://github.com/CppCXY/LanguageServer.Framework/issues)
- 💡 Request features via [GitHub Discussions](https://github.com/CppCXY/LanguageServer.Framework/discussions)

---

Made with ❤️ by the EmmyLua community
