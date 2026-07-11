# Arena Fall — Optimization Checklist

## 1. GPU Optimization
- [ ] URP Forward Rendering Path
- [ ] LOD Groups on all complex meshes (3 levels)
- [ ] GPU Instancing enabled on shared materials
- [ ] Occlusion Culling baked for all scenes
- [ ] Texture atlas for UI sprites
- [ ] Reduce overdraw (minimize transparent elements)
- [ ] Mobile: MSAA off, Post-processing quality reduced
- [ ] Shader variants stripped per platform
- [ ] Batching: Dynamic batching for small meshes
- [ ] Batching: Static batching for environment

## 2. CPU Optimization
- [ ] Object Pooling for: projectiles, loot items, VFX, audio
- [ ] Async operations for loading, saving, network
- [ ] Job System for AI pathfinding calculations
- [ ] Burst compiler for physics computations
- [ ] Profile CPU: < 33ms per frame (30fps target)
- [ ] Minimize Update() usage (use events where possible)
- [ ] Coroutines for delayed actions (avoid Update timers)
- [ ] Physics: Fixed timestep at 0.02s (50fps physics)
- [ ] NavMesh: Agent count limited to active bots

## 3. Memory Optimization
- [ ] Addressables for asset management
- [ ] Texture streaming enabled
- [ ] Unused asset unloading on scene transitions
- [ ] Pool limit enforcement (max 50 projectiles, etc.)
- [ ] Audio: OGG compression (128k SFX, 64k ambient)
- [ ] Mesh compression enabled
- [ ] Animation compression: Keyframe reduction
- [ ] Mobile: Texture max size 1024
- [ ] Mobile: Disable unused features (shadows, etc.)

## 4. Network Optimization
- [ ] Delta compression for state updates
- [ ] Interest management (relevancy culling)
- [ ] Adaptive tickrate: 20Hz (full), 10Hz (30+ players)
- [ ] Bandwidth limiting per connection
- [ ] NetworkVariable update rates adjusted per type
- [ ] RPC frequency limiting (cooldowns)
- [ ] Snapshot compression
- [ ] State change detection (only send changes)

## 5. Loading Optimization
- [ ] Scene loading: AsyncOperation with progress
- [ ] Addressables: Preload critical assets
- [ ] Background loading during menus
- [ ] Splash screen while loading
- [ ] Level streaming for large map
- [ ] Asset bundles built per platform

## 6. Mobile-Specific
- [ ] Graphics: Medium quality preset
- [ ] Resolution scaling (dynamic resolution)
- [ ] Particle quality reduced (max particles halved)
- [ ] Shadow distance: 30m (PC: 100m)
- [ ] Draw distance: 500m (PC: 2000m)
- [ ] Disable motion blur
- [ ] Disable volumetric effects
- [ ] Reduce animation update rate to 15fps for distant
- [ ] Single-pass stereo rendering (VR unused but good practice)
