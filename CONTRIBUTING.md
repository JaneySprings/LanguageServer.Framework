# Contributing to Language Server Framework

Thank you for your interest in contributing to the Language Server Framework! This document provides guidelines and instructions for contributing to the project.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How to Contribute](#how-to-contribute)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Enhancements](#suggesting-enhancements)

## 📜 Code of Conduct

This project follows a Code of Conduct that all contributors are expected to adhere to. Please be respectful and considerate in your interactions with other contributors.

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- A code editor (Visual Studio, VS Code, or Rider recommended)
- Git
- Basic understanding of the [Language Server Protocol](https://microsoft.github.io/language-server-protocol/)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/LanguageServer.Framework.git
   cd LanguageServer.Framework
   ```
3. Add the upstream remote:
   ```bash
   git remote add upstream https://github.com/CppCXY/LanguageServer.Framework.git
   ```

## 🤝 How to Contribute

### Types of Contributions

We welcome various types of contributions:

- 🐛 **Bug Fixes** - Fix issues reported in the issue tracker
- ✨ **New Features** - Implement new LSP features or framework capabilities
- 📝 **Documentation** - Improve or add documentation
- ✅ **Tests** - Add or improve test coverage
- 🎨 **Code Quality** - Refactor code, improve performance
- 🌐 **Examples** - Add examples or sample implementations

## 💻 Development Setup

### Build the Project

```bash
dotnet restore
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Run the Test Language Server

```bash
cd LanguageServer.Test
dotnet run
```

### Project Structure

```
LanguageServer.Framework/
├── LanguageServer.Framework/        # Main framework library
│   ├── Protocol/                    # LSP protocol definitions
│   │   ├── Message/                 # Protocol messages
│   │   ├── Model/                   # Data models
│   │   └── Capabilities/            # Client/Server capabilities
│   ├── Server/                      # Server implementation
│   │   ├── Handler/                 # Base handler classes
│   │   ├── JsonProtocol/            # JSON-RPC implementation
│   │   └── Metrics/                 # Performance metrics
│   └── LSPCommunicationBase.cs      # Core communication logic
├── LanguageServer.Framework.Tests/  # Unit tests
├── LanguageServer.Test/             # Example language server
└── README.md
```

## 📏 Coding Standards

### General Guidelines

- Follow C# coding conventions
- Use meaningful variable and method names
- Write clear, self-documenting code
- Add XML documentation comments for public APIs
- Keep methods focused and concise

### Code Style

```csharp
// ✅ Good
/// <summary>
/// Handles hover requests for the language server.
/// </summary>
public class HoverHandler : HoverHandlerBase
{
    protected override Task<Hover?> Handle(HoverParams request, CancellationToken token)
    {
        // Implementation
    }
}

// ❌ Avoid
public class hoverhandler { // Bad naming, no docs
    protected override Task<Hover?> Handle(HoverParams request, CancellationToken token){
        //Implementation
    }
}
```

### Naming Conventions

- **Classes**: PascalCase (e.g., `LanguageServer`, `HoverHandler`)
- **Methods**: PascalCase (e.g., `RegisterHandler`, `SendNotification`)
- **Properties**: PascalCase (e.g., `ClientCapabilities`, `ServerInfo`)
- **Fields**: camelCase with underscore prefix for private (e.g., `_disposed`, `_client`)
- **Parameters**: camelCase (e.g., `request`, `cancellationToken`)
- **Local Variables**: camelCase (e.g., `result`, `response`)

### Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Sends a log message to the client.
/// </summary>
/// <param name="type">The message type (Error, Warning, Info, Debug)</param>
/// <param name="message">The message content</param>
/// <returns>A task representing the asynchronous operation</returns>
public Task LogMessage(MessageType type, string message)
{
    // Implementation
}
```

## ✅ Testing Guidelines

### Writing Tests

- Use xUnit as the testing framework
- Use FluentAssertions for assertions
- Follow the AAA pattern (Arrange, Act, Assert)
- Test edge cases and error conditions
- Mock external dependencies

### Example Test

```csharp
[Fact]
public async Task HoverHandler_ShouldReturnHoverInformation()
{
    // Arrange
    var handler = new MyHoverHandler();
    var request = new HoverParams
    {
        TextDocument = new TextDocumentIdentifier { Uri = "file:///test.txt" },
        Position = new Position { Line = 0, Character = 0 }
    };

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result!.Contents.Should().NotBeNull();
}
```

### Test Coverage

- Aim for at least 80% code coverage for new features
- Run tests before submitting a pull request
- Ensure all tests pass

## 🔄 Pull Request Process

### Before Submitting

1. **Update your fork:**
   ```bash
   git fetch upstream
   git checkout main
   git merge upstream/main
   ```

2. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes:**
   - Write clear, concise commit messages
   - Follow the coding standards
   - Add tests for new functionality
   - Update documentation as needed

4. **Test your changes:**
   ```bash
   dotnet build
   dotnet test
   ```

5. **Commit your changes:**
   ```bash
   git add .
   git commit -m "Add feature: brief description"
   ```

### Commit Message Format

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>: <description>

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `test`: Adding or updating tests
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `chore`: Maintenance tasks

Examples:
```
feat: add support for inline completion
fix: correct hover position calculation
docs: update README with new examples
test: add tests for completion handler
```

### Submitting the Pull Request

1. **Push to your fork:**
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create a pull request** on GitHub with:
   - Clear title describing the change
   - Detailed description of what changed and why
   - Reference to related issues (e.g., "Fixes #123")
   - Screenshots or examples if applicable

3. **Code Review Process:**
   - Maintainers will review your code
   - Address any feedback or requested changes
   - Once approved, your PR will be merged

### Pull Request Checklist

- [ ] Code follows the project's coding standards
- [ ] All tests pass
- [ ] New tests added for new functionality
- [ ] Documentation updated (if applicable)
- [ ] Commit messages are clear and follow conventions
- [ ] No merge conflicts with main branch
- [ ] Self-review completed

## 🐛 Reporting Bugs

### Before Reporting

- Check if the bug has already been reported
- Verify the bug exists in the latest version
- Collect relevant information

### Bug Report Template

When reporting a bug, please include:

```markdown
**Describe the bug**
A clear description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Create a language server with '...'
2. Send request '...'
3. See error

**Expected behavior**
What you expected to happen.

**Actual behavior**
What actually happened.

**Environment:**
- OS: [e.g., Windows 11, Ubuntu 22.04]
- .NET Version: [e.g., .NET 8.0]
- Framework Version: [e.g., 1.0.0]

**Additional context**
- Error messages or stack traces
- Code samples
- Relevant logs
```

## 💡 Suggesting Enhancements

### Enhancement Request Template

```markdown
**Feature Description**
Clear description of the proposed feature.

**Use Case**
Why is this feature needed? What problem does it solve?

**Proposed Solution**
How should this feature work?

**Alternatives Considered**
Other approaches you've considered.

**Additional Context**
Any other relevant information, mockups, or examples.
```

## 🔍 Code Review Guidelines

### For Reviewers

- Be respectful and constructive
- Focus on the code, not the person
- Explain your reasoning
- Suggest improvements with examples
- Approve when ready or request changes with clear feedback

### For Contributors

- Don't take feedback personally
- Ask for clarification if needed
- Be open to suggestions
- Respond to all comments
- Make requested changes promptly

## 📚 Resources

- [LSP Specification](https://microsoft.github.io/language-server-protocol/specifications/lsp/3.18/specification/)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)

## 🙏 Recognition

Contributors will be recognized in:
- The project's README
- Release notes
- GitHub contributors page

Thank you for contributing to making this framework better! 🎉

## 📞 Questions?

If you have questions about contributing:
- Open a [Discussion](https://github.com/CppCXY/LanguageServer.Framework/discussions)
- Ask in an existing issue
- Reach out to maintainers

---

Happy Contributing! 🚀
