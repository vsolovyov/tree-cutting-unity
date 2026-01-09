# Grove Scene - Look & Feel

## Goal
Create atmospheric forest scene that demonstrates the core vision: dense monoculture → restored ecosystem. Focus on the feeling of transformation when trees are removed.

Reference: ~/dev/gamedev-research/rewild/vision.txt

## The Vision
> "Дуби в цьому лісі настільки густо насаджені, що до землі не пробивається жоден промінчик сонця, там не росте трава, кущі, нема квітів та ліан, майже ніяких звірів та пташок. Дуже темно і сумрачно."

Dense oak plantation, industrial monoculture. No light reaches ground. No grass, flowers, animals. Dark and gloomy.

> "Кожне прибирання дерева має давати безпосередній зворотній зв'язок: пробиваються промені сонця, з'являється трава, квіточки, тощо. Змінюється звук. Відразу!"

Immediate feedback when tree removed: sunbeams break through, grass appears, flowers bloom, sounds change.

## Scene Layers

### 1. Ground Layer (Dead → Alive)
**Before clearing:**
- Bare dirt/leaf litter texture
- No grass, no flowers
- Maybe dead leaves, fallen branches

**After clearing (per-tree radius):**
- Grass spawns/grows in cleared area
- Small flowers pop up
- Moss on rocks becomes visible
- Could use shader that blends based on "light exposure" value

### 2. Canopy & Lighting
**Dense state:**
- Trees placed close together
- Overlapping canopies block directional light
- Ambient light only, dark/blue tint
- God rays disabled or very faint

**Cleared state:**
- Removing tree creates gap in canopy
- Directional light/sun hits ground through gap
- God rays visible in clearings
- Warm light color in cleared areas

### 3. Audio Landscape
**Dense forest:**
- Muffled, quiet
- Occasional creak
- No birds
- Wind in leaves (distant, high)

**Cleared areas:**
- Bird calls (proximity to clearings)
- Insects buzzing
- Wind through grass
- Water sounds if stream nearby

### 4. Wildlife (Progressive)
As forest opens up:
1. First: insects (butterflies, bees on flowers)
2. Then: small birds
3. Later: ground animals (rabbits, squirrels)
4. Endgame: larger animals, predators

## Technical Approach

### Tree Placement
- Grid or semi-random dense placement
- ~50-100 trees for prototype grove
- Each tree has CuttableTree component
- Mix of sizes? Or uniform (monoculture feel)

### Ground Response System
**Option A: Per-tree zones**
- Each tree has a "shadow zone" collider
- When tree removed, enable grass/flowers in that zone
- Pre-placed but disabled vegetation

**Option B: Shader-based**
- Ground shader samples "clearance map" (render texture)
- Cleared areas = white, dense = black
- Shader blends grass/dirt based on value
- More dynamic but complex

**Option C: Hybrid**
- Shader for grass density
- Spawned prefabs for flowers/plants
- Best of both worlds

### Lighting Response
- Use Light Probes or Adaptive Probe Volumes
- Update when trees removed
- Or: simple approach - each tree has a disabled spotlight pointing down, enable on removal

### Audio Zones
- AudioSource per clearing (bird sounds)
- Enable when tree in that zone removed
- Or: global ambience manager that crossfades based on % cleared

## Implementation Steps

### Phase 1: Basic Grove
1. Create new scene "ForestGrove"
2. Terrain or ground plane with dirt texture
3. Dense tree placement (copy from SceneSetup pattern)
4. Dark ambient lighting, no directional light reaching ground
5. Player spawn at edge

### Phase 2: Lighting Response
1. Add directional light (sun) but blocked by canopy
2. Each tree removal = gap in shadow
3. Test with simple spotlight per tree (enable on death)
4. Add god ray particles in clearings

### Phase 3: Ground Response
1. Create grass/flower prefabs (Polytope assets?)
2. Pre-place around each tree, disabled
3. Enable when that tree removed
4. Add growth animation (scale from 0)

### Phase 4: Audio
1. Dense forest ambience (quiet, creaky)
2. Bird sound sources in clearings
3. Crossfade system based on proximity to cleared areas
4. Victory sting when area fully cleared

### Phase 5: Polish
1. Particle effects (pollen, dust motes in light beams)
2. Butterflies/insects in clearings
3. Camera "hero shot" when zone cleared
4. Day/night cycle? (maybe out of scope)

## Assets Needed
- Ground textures (dirt, grass blend)
- Grass tufts, flowers, ferns (check Polytope pack)
- Bird/insect audio
- Forest ambience audio
- God ray particle or shader
- Skybox (forest appropriate)

## Open Questions
- How many trees in grove? (50-100 feels right)
- Which trees "must" be cut vs optional?
- How does player know they're done with a zone?
- Should there be a "zone complete" celebration?
- Third-person camera for hero shots?

## Metrics to Track
- Time to clear first tree
- Time to clear zone
- Does transformation feel rewarding?
- Is the before/after contrast strong enough?
