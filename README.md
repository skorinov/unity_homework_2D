# Ruby's Rise

A 2D endless vertical platformer inspired by Doodle Jump, built with Unity 6. Guide Ruby on her endless ascent through procedurally generated platforms with unique mechanics and challenges.

## 🎮 Game Features

### Core Gameplay
- **Endless vertical climbing** with smooth physics-based movement
- **Screen wrapping** - exit one side to appear on the other
- **Double jump** mechanics with coyote time for forgiving controls
- **Drop-through platforms** for strategic navigation
- **Dynamic camera** that follows player movement

### Platform Variety
- **Moving Platforms** - Isolated platforms that move horizontally when alone
- **Fragile Platforms** - Break after multiple landings when neighbors are present
- **Crumbling Platforms** - Gradually deteriorate while standing on them
- **Bounce Platforms** - Launch players higher with enhanced jump force

### Collectibles & Enemies
- **Coins** - Collect to increase your score
- **Saw Enemies** - Rotating hazards that patrol platforms
- **Smart Enemy Spawning** - Enemies adapt to platform layouts

### Advanced Systems
- **Object Pooling** - Optimized performance for endless gameplay
- **Modular Platform Actions** - Scriptable Object-based platform behaviors
- **Procedural Generation** - Multi-platform levels with intelligent spacing
- **Audio Management** - Dynamic music and sound effects
- **Save System** - Persistent statistics and settings

## 🎯 Controls

| Action | Default Key | Customizable |
|--------|-------------|--------------|
| Move Left/Right | A/D or Arrow Keys | ❌ |
| Jump | Space | ✅ |
| Drop Through Platform | S | ✅ |
| Menu Navigation | Arrow Keys/WASD | ❌ |
| Pause/Menu | Escape | ❌ |

## 🏗️ Technical Architecture

### Core Systems
- **Singleton Managers** - GameManager, AudioManager, UIManager, DataManager
- **Object Pooling** - Efficient memory management for platforms, coins, and enemies
- **Modular Actions** - Platform behaviors defined as ScriptableObjects
- **State Management** - Clean UI state transitions and game flow

### Performance Optimizations
- **Pooled Objects** - Reuse instead of instantiate/destroy
- **Conditional Updates** - Frame-skipped animation updates
- **Smart Cleanup** - Automatic removal of off-screen objects
- **Efficient Collision** - Optimized ground checking and trigger systems

### Code Organization
```
Scripts/
├── Constants/          # Game constants and configuration
├── Controllers/        # Player, Platform, Enemy, and Collectible controllers
├── Data/              # Save data and game state management
├── Managers/          # Core game systems (Audio, Input, Game, UI)
├── Pooling/           # Object pooling system
├── UI/                # User interface and navigation
└── Utilities/         # Helper classes and singletons
```

## 🎨 Platform Actions System

The game features a flexible ScriptableObject-based system for platform behaviors:

- **BounceAction** - Chance-based super jumps
- **CoinSpawnAction** - Collectible generation
- **CrumblingAction** - Time-based platform destruction
- **ConditionalFragileAction** - Context-aware fragile platforms
- **EnemySpawnAction** - Dynamic enemy placement
- **IsolatedMovingAction** - Smart moving platform behavior

## 🔧 Development Setup

### Requirements
- Unity 6000.0.45f1
- C# .NET Framework compatible IDE

### Key Dependencies
- Unity Physics2D
- Unity UI Toolkit
- TextMeshPro

### Build Targets
- Windows (Primary)

## 📊 Statistics Tracking

- **Session Height** - Highest point reached in current game
- **Session Coins** - Coins collected in current session
- **Best Height** - All-time highest climb
- **Best Coins** - Maximum coins collected in a single session

## ⚙️ Settings & Customization

### Audio
- **Music Volume** - Adjustable background music (0-100%)
- **SFX Volume** - Sound effects volume control (0-100%)
- **Dynamic Audio** - Context-sensitive music switching
- **Real-time Preview** - Immediate audio feedback when adjusting volumes

### Input
- **Custom Key Binding** - Rebind jump and drop keys with live input capture
- **Input Validation** - Prevents invalid key assignments and system key conflicts
- **Visual Feedback** - "Press Key..." prompt during key binding process

### UI Experience
- **Scrollable Settings Panel** - Smooth vertical scrolling for all options
- **Keyboard Navigation** - Full settings navigation with arrow keys/WASD
- **Auto-scroll to Selection** - Automatically scrolls to highlight current option
- **Animated Transitions** - Smooth scroll animations between menu items
- **Responsive Design** - Adapts to different screen sizes and aspect ratios

## 🚀 Performance Features

- **Frame Rate Independent** - Consistent gameplay across different FPS
- **Memory Efficient** - Object pooling prevents garbage collection spikes
- **Scalable Generation** - Platform creation scales with player progress
- **Smart Culling** - Automatic cleanup of distant objects

## 🎯 Game Flow

1. **Main Menu** - Start new game or resume existing session
2. **Gameplay** - Endless climbing with dynamic challenges
3. **Game Over** - Fall detection triggers score summary
4. **Statistics** - View personal records and achievements

## 📈 Future Expansion

The modular architecture supports easy addition of:
- New platform types via ScriptableObject actions
- Additional enemy varieties through the enemy pool system
- Power-ups and special abilities
- Theme variations and visual styles
- Achievement systems

## 🏆 Credits

**Ruby's Rise** - A modern take on endless vertical platformers with emphasis on smooth gameplay, modular design, and performance optimization.