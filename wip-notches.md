# Tree Notches - Visual Cut Marks

## Goal
Show visible damage on tree as player chops. Each hit should leave a mark, building toward the final cut.

## Approaches

### A. Model Swap (Simplest)
Swap entire tree model at damage thresholds.
- `PT_Fruit_Tree_01_green` → `_notch1` → `_notch2` → `_cut`
- Requires artist to create notch variants
- Pro: Simple code, predictable look
- Con: Fixed notch positions, need many model variants

### B. Decal Projection
Project notch texture onto tree trunk at hit position.
- Use Unity's decal projector or custom shader
- Spawn decal at `cutHeight` facing player direction
- Pro: Dynamic positions, one texture asset
- Con: May look flat, UV seams on cylindrical trunk

### C. Additive Mesh (Fake Notch)
Spawn small "notch" mesh that sits on trunk surface.
- Pre-made wedge-shaped mesh with wood interior texture
- Position at hit point, rotate to face player
- Pro: 3D depth, simple spawning
- Con: Floating geometry, intersection issues

### D. Mesh Deformation (Complex)
Actually carve into tree mesh at runtime.
- CSG subtraction or vertex displacement
- Pro: Most realistic
- Con: Complex, performance cost, needs runtime mesh modification

### E. Hybrid: Decal + Particles
Decal for the cut surface + sawdust pile at base.
- Accumulating sawdust shows progress
- Pro: Sells the "material removal" feeling
- Con: Two systems to maintain

## Recommendation
Start with **A (Model Swap)** for prototype - ask artist for 2-3 notch stages.
If that feels too static, try **C (Additive Mesh)** for dynamic positioning.

## Implementation Steps (Model Swap)

1. **Create/obtain notch model variants**
   - `PT_Fruit_Tree_01_green_notch1.prefab` (25% damage)
   - `PT_Fruit_Tree_01_green_notch2.prefab` (50% damage)
   - `PT_Fruit_Tree_01_green_notch3.prefab` (75% damage)
   - Already have `_cut` for falling

2. **Update CuttableTree.cs**
   ```
   [Header("Notch Variants")]
   public GameObject[] notchPrefabs; // Progressive damage states

   void UpdateVisualState()
   {
       float healthPercent = GetHealthPercent();
       int notchIndex = Mathf.FloorToInt((1 - healthPercent) * notchPrefabs.Length);
       // Swap current model for notch variant
   }
   ```

3. **Call UpdateVisualState() in TakeDamage()**

4. **Ensure notch rotation matches accumulated cut direction**
   - Notch should face the player/cut side

## Implementation Steps (Additive Mesh)

1. **Create notch mesh asset**
   - Small wedge/V-shape, ~20cm wide
   - Wood grain texture on inside faces

2. **Update CuttableTree.cs**
   ```
   [Header("Notch Settings")]
   public GameObject notchMeshPrefab;
   public int maxVisibleNotches = 5;
   List<GameObject> spawnedNotches = new();

   void SpawnNotch(Vector3 hitDirection)
   {
       var notch = Instantiate(notchMeshPrefab, transform);
       notch.transform.localPosition = Vector3.up * cutHeight;
       notch.transform.rotation = Quaternion.LookRotation(-hitDirection);
       spawnedNotches.Add(notch);

       // Remove oldest if too many
       if (spawnedNotches.Count > maxVisibleNotches)
       {
           Destroy(spawnedNotches[0]);
           spawnedNotches.RemoveAt(0);
       }
   }
   ```

3. **Add slight random offset to avoid z-fighting**

4. **Parent notches to tree so they fall with it**

## Open Questions
- How many notch stages feel right? (Test with 3)
- Should notch position vary or always at cutHeight?
- Do notches transfer to the falling cut tree model?
- Sawdust accumulation worth adding?

## Assets Needed
- Notch mesh or model variants (ask artist or create in Blender)
- Wood interior texture (for notch inside)
