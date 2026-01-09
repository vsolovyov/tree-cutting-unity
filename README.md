# Rewild - Tree Cutting Prototype

Prototype for tree cutting mechanic in Rewild, a cozy solarpunk ecosystem restoration game.

## Setup

### Unity Version
Unity 6.3 LTS

### Required Assets (Asset Store)
Install these before opening the project:

1. **Feel** by More Mountains
   - Includes MMFeedbacks, NiceVibrations
   - Import to: `Assets/Feel/`

2. **Polytope Studio - Lowpoly Environments**
   - Trees, stumps, logs prefabs
   - Import to: `Assets/Polytope Studio/`

3. **Starter Assets - First Person Character Controller** (Unity Registry)
   - Via Package Manager or Asset Store
   - Import to: `Assets/Starter Assets/`

### After Importing Assets
1. Open the project in Unity
2. Go to **Tools → Setup Tree Cutting Scene**
3. Play the generated scene

## Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD | Left Stick |
| Look | Mouse | Right Stick |
| Interact/Chop | E | West Button (X/Square) |
| Cancel | Escape | East Button (B/Circle) |
| Sprint | Left Shift | Left Stick Press |
| Jump | Space | South Button (A/Cross) |

## Project Structure

```
Assets/
├── Scripts/
│   ├── Tree/           # CuttableTree, TreeHighlight
│   ├── Player/         # TreeCutter, CuttingMinigame, Movement, Look
│   ├── UI/             # CuttingMinigameUI
│   └── Editor/         # SceneSetup tool
├── Scenes/             # TreeCuttingPrototype
├── Audio/              # Custom audio (falling-tree.wav)
└── Input/              # GameInputActions
```

## Mechanic Overview

- Walk near tree → highlight + prompt
- Press E → timing minigame activates (vertical bar, two hit zones)
- Time hits in green zones → damage tree
- Perfect hits → zones shrink, speed increases, combo builds
- Tree health 0 → stump appears, trunk falls with 3-phase animation
- Audio synced to fall (cracking → whoosh → impact → settle)

## Future Work

See `wip-notches.md` and `wip-grove.md` for planned features.
