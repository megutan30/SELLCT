# CLAUDE.md
日本語で回答するように
This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SELLCT is a meta-puzzle game built with WPF (.NET 6) that blurs the line between game and reality. Players manipulate game components by creating, deleting, and renaming files in the file system, which directly affects the game's behavior and UI. The game has two phases: Phase 1 (adventure game) and Phase 2 (command prompt battle).

## Build & Development Commands

```bash
# Build the solution
dotnet build SELLCT.sln

# Run the application
dotnet run --project SELLCT/SELLCT.csproj

# Build for release
dotnet build SELLCT.sln -c Release

# Publish single-file executable
dotnet publish SELLCT/SELLCT.csproj -c Release --self-contained true -p:PublishSingleFile=true
```

## Architecture Overview

The project follows Clean Architecture principles with clear separation of concerns:

### Core Layer (`SELLCT/Core/`)
- **Entities**: `GameComponent`, `PuzzleDefinition`, `PuzzleAction`, `PuzzleTrigger`
- **Events**: Domain events for component lifecycle and game state changes
- **Interfaces**: `IEventDispatcher`, `IPuzzleActionHandler`
- Contains pure business logic with no external dependencies

### Application Layer (`SELLCT/Application/`)
- **Services**: `PuzzleService` - orchestrates puzzle logic and component interactions
- Coordinates between Core domain logic and Infrastructure services

### Infrastructure Layer (`SELLCT/Infrastructure/`)
- **Services**: `ComponentManager` - handles file system watching and component lifecycle
- **Services**: `EventDispatcher`, `LetterService`, `MetaGameController`
- Manages file I/O, system integration, and external dependencies

### Presentation Layer (`SELLCT/Views/`, `SELLCT/ViewModels/`)
- **Views**: `MainWindow.xaml` - main game interface
- **ViewModels**: Data binding and UI logic
- Uses WPF with ModernWpf UI library

## Key Concepts

### Component System
The game revolves around "components" that exist both as files in the `components/` folder and as UI elements:
- Components are organized by type: `UI/`, `Text/`, `Visual/`, `System/`
- File operations (create/delete/rename/move) trigger game events
- Special components: `Button.txt`, `GameWindow.txt`, `KEY.txt`, `TextWindow.txt`, `Explorer.txt`, `Mouse.txt`, `Keyboard.txt`

### Event-Driven Architecture
- `ComponentManager` watches the file system and dispatches events
- `PuzzleService` subscribes to component events and triggers game actions
- Events flow: File Change → ComponentManager → Event → PuzzleService → Game Action

### File System Integration
- `components/` folder structure mirrors game component hierarchy
- Hidden files (with Hidden attribute) represent system components
- Real-time file watching enables meta-game mechanics

## Development Patterns

### Adding New Components
1. Define the component in `ComponentManager.CreateInitialComponents()`
2. Add puzzle definitions in `PuzzleService.LoadPuzzles()`
3. Create corresponding UI elements in `MainWindow.xaml`
4. Implement action handlers in the puzzle action handler

### Event Flow Example
```
File Created → FileSystemWatcher → ComponentManager.OnFileCreated() 
→ ComponentCreatedEvent → PuzzleService.CheckPuzzlesOnComponentCreated() 
→ PuzzleAction execution → UI update
```

### Testing Strategy
- Component operations can be tested by manipulating files in the `components/` folder
- Event-driven architecture allows for easy mocking and unit testing
- File system operations should be tested with temporary directories

## Important Files

- `ComponentManager.cs` - Core file watching and component lifecycle
- `PuzzleService.cs` - Puzzle logic and game rule definitions  
- `MainWindow.xaml(.cs)` - Primary game UI and user interactions
- `GameComponent.cs` - Component entity with properties and behavior
- `指示書.txt` - Detailed component behavior specifications (Japanese)

## Meta-Game Features

The game includes system-level interactions:
- Explorer termination when `Explorer.txt` is deleted
- Input device disabling when `Mouse.txt`/`Keyboard.txt` are removed
- Game window closure triggering Phase 2 transition
- Command prompt battle system in Phase 2

## UI Framework

- Uses ModernWpf for modern Windows 11 styling
- Canvas-based layout for precise positioning
- Custom pixel art button styles
- Animation support for UI transitions
- Resource management for images and assets

## Dependencies

- **ModernWpfUI** (0.9.6) - Modern Windows styling
- **Microsoft.Toolkit.Mvvm** (7.1.2) - MVVM helpers
- **Newtonsoft.Json** (13.0.3) - JSON serialization
- **.NET 6 Windows** - Target framework with WPF support

## Security Considerations

The game performs system-level operations that require careful handling:
- File system manipulation should be scoped to the `components/` directory
- Process termination (Explorer) should include safety checks
- Input disabling features should be reversible
- Backup mechanisms should be implemented for system state changes