# Arena Fall — Art Style Guide

## 1. Visual Direction

**Core Aesthetic:** "Clean Sci-Fi Minimalism"  
The game world is a simulated training facility — think clean lines, holographic interfaces, metallic surfaces, and stark landscapes with purposeful design.

### Inspirational Touchstones
- Clean, modern architecture
- Military training facilities
- Solar and wind energy installations
- Frosty, stark environments
- Holographic blue/orange lighting

### Visual Pillars
1. **Functional Design** — Everything looks like it serves a purpose
2. **Clean Surfaces** — Minimal clutter, smooth materials
3. **Holographic Accents** — Glowing blue/orange tech elements
4. **Stark Contrast** — Bright highlights against dark shadows
5. **Tactical Aesthetic** — Military-inspired gear and equipment

---

## 2. Color Palette

### Primary Colors
| Color | Hex | Usage |
|-------|-----|-------|
| Deep Navy | #0A1628 | Backgrounds, shadows |
| Steel Blue | #1E3A5F | Secondary surfaces |
| Midnight | #050B14 | Deep shadows, void |

### Accent Colors
| Color | Hex | Usage |
|-------|-----|-------|
| Neon Orange | #FF6B35 | Primary accent, UI highlights |
| Cyan | #00D4FF | Holographic elements, shields |
| Amber | #FFB800 | Warnings, loot tiers |
| Crimson | #FF2244 | Damage, danger, enemies |

### Neutral Colors
| Color | Hex | Usage |
|-------|-----|-------|
| Pure White | #FFFFFF | Text, UI elements |
| Light Gray | #B0B8C4 | Secondary text |
| Mid Gray | #607080 | Disabled elements |
| Dark Gray | #2A3440 | Panel backgrounds |

### Material Colors
| Color | Hex | Usage |
|-------|-----|-------|
| Titanium | #8899AA | Metal surfaces |
| Gunmetal | #445566 | Weapon bodies |
| Concrete | #8A9BA8 | Building exteriors |
| Snow White | #E8ECEF | Snow/ice surfaces |
| Rust Orange | #CC6633 | Industrial elements |

---

## 3. Character Design

### Silhouette
- Clean, athletic proportions
- Tactical armor plating
- Slim profile for gameplay clarity
- Gender-neutral base with masculine/feminine variants
- Height: 1.75m (average)

### Character Features
- Full-face helmets (primary style)
- Exposed faces optional (premium skins)
- Visors with holographic glow
- Armored shoulders, chest, knees
- Tactical vests with pouches
- Boots with tech accents

### Color Variants
- Default: Gray/Blue/Orange
- Rare: Metallic/Carbon
- Epic: Animated patterns
- Legendary: Full holographic effects

### Skin Tiers
1. **Common** — Color swap
2. **Uncommon** — Pattern/texture change
3. **Rare** — Gear differences
4. **Epic** — Silhouette changes
5. **Legendary** — Full model change + effects

---

## 4. Environment Art

### Architecture Style
- Brutalist concrete with tech overlays
- Large geometric forms
- Glass and steel structures
- Underground bunker aesthetic
- Modular building system

### Biome Color Keys
| Biome | Primary | Secondary | Accent |
|-------|---------|-----------|--------|
| Nexus | #1A2A4A | #334466 | #00D4FF |
| Solar Fields | #4A7A5A | #6A9A7A | #FFB800 |
| Industrial | #4A3A2A | #665544 | #FF6B35 |
| Hydro Station | #2A4A6A | #4A6A8A | #00AADD |
| Frost Depots | #8A9AAE | #AABBCC | #88CCFF |
| Transit Hub | #3A4A5A | #5A6A7A | #FF6B35 |
| Research Labs | #1A2A3A | #FFFFFF | #00D4FF |
| Outpost 7 | #6A5A4A | #8A7A6A | #FFB800 |
| Crash Site | #3A3A3A | #5A4A3A | #FF4422 |
| Echo Ridge | #6A7A6A | #8A9A8A | #88CC88 |

### Lighting
- Dynamic time of day (optional)
- Default: Overcast, cool lighting
- Holographic accent lights
- Volumetric fog for atmosphere
- Real-time shadows (characters only)
- Baked lighting for environment

---

## 5. UI Art Style

### Design Language
- Flat design with subtle gradients
- Holographic card elements
- Glowing accent borders
- Translucent panels (glass effect)
- Clean typography
- Icon-driven navigation

### UI Components
- **Buttons:** Flat with hover glow effect
- **Cards:** Rounded rectangles with border glow
- **Sliders:** Thin line with circular handle
- **Toggles:** Minimal switch design
- **Progress Bars:** Thin horizontal bars
- **Icons:** Line art style, consistent stroke

---

## 6. Visual Effects (VFX)

### Particle Systems
- **Holograms:** Blue/orange emissive particles
- **Shields:** Hexagonal grid overlay with glow
- **Damage:** Red sparks and impact flashes
- **Healing:** Green upward-flowing particles
- **Explosions:** Orange/red with smoke
- **Smoke:** Gray with blue undertones
- **Water:** Blue-white splashes
- **Fire:** Orange/yellow with heat distortion

### Post-Processing
- Neutral color grading (slightly desaturated)
- Subtle bloom for emissive elements
- Vignette for cinematic feel
- Chromatic aberration (minimal, on effects only)
- Depth of field (scoped weapons only)
- Motion blur (optional, performance dependent)

---

## 7. Texture Guidelines

### Resolution Targets
| Platform | Environment | Characters | UI |
|----------|------------|------------|-----|
| PC | 2048x2048 | 2048x2048 | 512x512 |
| Mobile | 1024x1024 | 1024x1024 | 256x256 |

### Texture Channels
- Albedo (RGB) + Metallic (A)
- Normal map (RG) + Roughness (B) + AO (A)
- Emission (RGB masked)
- Detail mask (RGBA for material layering)

---

## 8. Animation Style

### Movement
- Weighted, grounded movement
- Weapon sway with movement
- Realistic weapon reloads
- Smooth transitions between states
- Procedural recoil system

### Animation Priority
1. Full-body (movement, actions)
2. Upper body (aiming, shooting)
3. Additive (recoil, breathing)
4. Facial (not applicable - helmets)

### Key Animations Needed
- Idle (various stances)
- Walk/Jog/Sprint
- Crouch walk
- Prone crawl
- Jump/Fall/Land
- Slide
- Vault/Climb
- Swim
- Weapon Equip/Holster
- Reload (tactical/empty)
- Shoot (fire modes)
- Melee Attack
- Throw
- Use Item
- Vehicle Enter/Exit
- Death/Ragdoll
- Revive
- Emote poses
