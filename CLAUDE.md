# Tree Cutting Prototype

Unity 6.3 LTS project. Cozy solarpunk game - restore ecosystem by thinning dense monoculture forest.

Vision: `~/dev/gamedev-research/rewild/vision.txt`

## Quick Start
`Tools → Setup Tree Cutting Scene` regenerates test scene with all wiring.

## Architecture
- **CuttableTree** - health, damage, 3-phase fall animation, model swap
- **TreeCutter** - player component, targeting, input handling
- **CuttingMinigame** - dual-zone vertical timing bar, combo system
- **SceneSetup** (Editor) - creates scene, wires feedbacks, spawns trees

## Key Paths
- Scripts: `Assets/Scripts/{Tree,Player,UI,Editor}/`
- Input: `Assets/Input/GameInputActions.inputactions`

## Dependencies (not in repo)
Feel, Polytope Studio Lowpoly Environments, Starter Assets, TextMesh Pro

## Patterns
- MMFeedbacks for juice (camera shake, sound, particles)
- New Input System with BroadcastMessages
- Editor tooling over manual scene setup

## Future Work
See `wip-notches.md` (cut marks) and `wip-grove.md` (atmosphere scene)
