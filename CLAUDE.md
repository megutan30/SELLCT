# CLAUDE.md

日本語で出力

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SELLCT is a **meta-adventure educational game** that demonstrates social engineering concepts and the dangers of blind trust in software. The project is designed as a **defensive security educational tool** showing how malicious software can manipulate users through seemingly innocent interactions.

### ⚠️ Important Security Notice

This is an educational project that simulates malicious behavior patterns for demonstration purposes. The code includes:

- File system manipulation
- Registry modifications 
- Process control
- Explorer termination

**All operations include recovery mechanisms and are designed for educational/research purposes only.**

## Architecture Overview

The project uses **Clean Architecture** principles with the following layers:

```
SELLCT/
├── Core/                    # Domain Layer - Business logic and entities
│   ├── Entities/           # GameComponent, GameState, PuzzleDefinition
│   ├── Events/            # Domain events (ComponentCreated, ComponentDeleted, etc.)
│   ├── Interfaces/        # Abstractions (IEventDispatcher, IPuzzleActionHandler)
│   └── ValueObjects/      # Value types
├── Application/            # Application Layer - Use cases and orchestration  
│   └── Services/          # PuzzleService
├── Infrastructure/         # Infrastructure Layer - External concerns
│   ├── FileSystem/        # File operations
│   ├── Persistence/       # Data storage
│   └── Services/          # ComponentManager, EventDispatcher, etc.
├── Presentation/           # Presentation Layer - UI and user interaction
│   └── Views/             # WPF views (MainWindow.xaml)
└── ViewModels/            # MVVM view models
```

## Key Components

### Core Architecture

- **GameComponent**: Core entity representing game elements (UI, Text, Visual, System types)
- **PuzzleService**: Orchestrates game logic and puzzle solving mechanics
- **ComponentManager**: Manages file system monitoring and component lifecycle
- **EventDispatcher**: Handles domain events between layers

### Game Mechanics

The game operates in two phases:

**Phase 1: Social Engineering Simulation**
- Players interact with a "trapped AI" through file creation/deletion
- File operations in `components/` folder trigger puzzle events
- Progressive trust-building through seemingly innocent requests

**Phase 2: System Control Demo** 
- Demonstrates how accumulated "permissions" enable system control
- Shows command prompt battles and explorer manipulation
- Educational demonstration of malware escalation tactics

## Development Commands

### Building and Running

```bash
# Build the solution
dotnet build SELLCT.sln

# Run the application  
dotnet run --project SELLCT/SELLCT.csproj

# Build for release
dotnet publish SELLCT/SELLCT.csproj -c Release --self-contained false
```

### Project Configuration

- **Framework**: .NET 6 Windows with WPF
- **UI Library**: ModernWpfUI 0.9.6
- **MVVM**: Microsoft.Toolkit.Mvvm 7.1.2
- **Serialization**: Newtonsoft.Json 13.0.3

### Testing and Debugging

No formal test framework is configured. The application includes:

- Debug console output throughout
- Debug panel in UI (toggle-able)
- Extensive logging in ComponentManager and PuzzleService

## Important Implementation Details

### File System Integration

The `ComponentManager` class monitors the `components/` directory:

```csharp
// Key directories created on startup:
components/
├── UI/          # User interface components
├── Text/        # Text-based components  
├── Visual/      # Visual elements
├── System/      # System control components (hidden)
└── SELLCT/      # Authority/permission files
```

### Component-Based Puzzle System

Game logic is driven by file operations:

- **File Creation**: Triggers `ComponentCreatedEvent`
- **File Deletion**: Triggers `ComponentDeletedEvent` 
- **File Rename**: Triggers `ComponentRenamedEvent`

The `PuzzleService` responds to these events with predefined puzzle logic.

### Event-Driven Architecture

Key events flow through `IEventDispatcher`:

```csharp
// Primary game events
ComponentCreatedEvent    // File created in components/
ComponentDeletedEvent    // File deleted from components/
ComponentRenamedEvent    // File renamed in components/
HiddenItemRevealedEvent  // Hidden file becomes visible
```

### Safety Mechanisms

The codebase includes multiple safety features:

- Emergency recovery functions in `MetaGameController`
- Registry cleanup on application exit
- File system restoration capabilities
- Debug mode toggles for dangerous operations

## Development Guidelines

### Code Style

- **C# 10** syntax with nullable reference types
- **MVVM pattern** for UI separation
- **Clean Architecture** boundaries are strictly enforced
- **Event-driven** communication between layers

### Security Considerations

When modifying this code:

1. **Always maintain recovery mechanisms** - any system modification must include undo functionality
2. **Preserve educational intent** - changes should enhance the learning experience
3. **Document dangerous operations** - clearly mark any code that affects system state
4. **Test recovery paths** - ensure emergency restoration works in all scenarios

### Key Files to Understand

| File | Purpose |
|------|---------|
| `PuzzleService.cs:32-607` | Core game logic and puzzle definitions |
| `ComponentManager.cs:16-866` | File system monitoring and component lifecycle |
| `MainWindow.xaml.cs` | UI event handling and state management |
| `MetaGameController.cs` | Phase 2 system control demonstration |

### Component Lifecycle

Understanding the component system is crucial:

1. **Startup**: Initial components created in `ComponentManager.CreateInitialComponents()`
2. **Monitoring**: `FileSystemWatcher` detects changes in `components/` directory
3. **Events**: File operations trigger domain events
4. **Puzzles**: `PuzzleService` evaluates conditions and executes actions
5. **UI Updates**: Events propagate to UI layer for visual updates

## Educational Context

This project serves as a **practical demonstration** of:

- Social engineering attack patterns
- Progressive trust exploitation  
- Software permission escalation
- File system security implications
- Registry-based persistence techniques

The implementation prioritizes **educational clarity** over production robustness, making the attack vectors visible and understandable for learning purposes.

## Emergency Recovery

If system issues occur during development:

```csharp
// Emergency restoration (in MetaGameController)
Process.Start("explorer.exe");                    // Restore Explorer
Registry.CurrentUser.DeleteSubKey("...\\Run");    // Remove startup entries
Directory.Delete("components", true);             // Clean up files
```

## Notes for Future Development

- The puzzle system is highly extensible - new puzzles can be added to `PuzzleService.LoadPuzzles()`
- Component types are enum-based and can be extended in `ComponentType`
- The Clean Architecture allows for easy testing and UI framework changes
- All dangerous operations are abstracted behind interfaces for easier mocking

This codebase demonstrates advanced C# concepts including file system monitoring, registry manipulation, process control, and event-driven architecture within a Clean Architecture framework.