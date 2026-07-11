# Arena Fall — Audio Style Guide

## 1. Audio Direction

**Style:** Electronic/Sci-Fi with cinematic intensity  
**Tone:** Tension, urgency, triumph  
**Inspiration:** Synthwave, cyberpunk ambient, military sound design

### Core Audio Pillars
1. **Clean & Punchy** — Every sound has impact
2. **Layered** — Sounds built from multiple layers
3. **Dynamic** — Audio adapts to gameplay intensity
4. **Spatial** — 3D positional audio for immersion
5. **Informative** — Audio conveys gameplay information

---

## 2. Music System

### Dynamic Music Layers
- **Layer 1 (Ambient):** Low pads, atmospheric
- **Layer 2 (Tension):** Percussion, pulses
- **Layer 3 (Combat):** Full drums, bass
- **Layer 4 (Intense):** Lead synths, climax

### Music State Transitions
| State | Trigger | Crossfade |
|-------|---------|-----------|
| Lobby | Pre-match | 1s |
| Drop | Freefall | 0.5s |
| Loot | Landing | 2s |
| Exploration | No enemies near | 3s |
| Warning | Zone closing | 1s |
| Combat | Enemy nearby | 0.5s |
| Final | 10 players left | 1s |
| Victory | Win | 0s (immediate) |
| Defeat | Eliminated | 1s |

### Track Requirements
- Main Menu Theme (loop)
- Lobby Theme (loop)
- Match Ambient (procedural)
- Combat Intensity (procedural)
- Victory Fanfare
- Defeat Theme
- Training Theme (loop)

---

## 3. Sound Effects

### Weapon Sounds
Each weapon needs:
- **Fire:** Close, medium, distant variants
- **Fire (Suppressed):** Close, medium, distant
- **Reload (Tactical):** Magazine out, mag in, bolt
- **Reload (Empty):** Same + bolt release
- **Equip:** Weapon draw sound
- **Holster:** Weapon stow sound

### Footsteps
| Surface | Material | Variations |
|---------|----------|------------|
| Concrete | Hard | 5 |
| Metal | Metallic | 5 |
| Grass | Soft | 5 |
| Wood | Hollow | 5 |
| Water | Splash | 3 |
| Snow | Crunch | 5 |
| Gravel | Scrape | 5 |

### Impact Sounds
| Surface | Bullet | Melee |
|---------|--------|-------|
| Concrete | Ricochet | Thud |
| Metal | Pinging | Clang |
| Flesh | Wet impact | Slash |
| Wood | Splinter | Crack |
| Water | Splash | Splash |

### UI Sounds
- **Button Click:** Short digital blip (0.1s)
- **Button Hover:** Soft rise (0.05s)
- **Panel Open:** Whoosh (0.3s)
- **Panel Close:** Reverse whoosh (0.2s)
- **Notification:** Double chime (0.5s)
- **Warning:** Pulse beep (0.3s)
- **Error:** Low buzz (0.2s)
- **Level Up:** Ascending chime (1s)
- **Kill Confirmed:** Short sting (0.5s)
- **Match Start:** Countdown beeps + horn
- **Zone Warning:** Alarm pulse

### Ambient Sounds
- **Wind:** Low rumble with gusts
- **Water:** River/lake ambient loop
- **Machinery:** Industrial hum
- **Wildlife:** Synthetic chirps/buzzes
- **Distant Combat:** Muffled gunfire

---

## 4. Voice System

### Announcer
- Match starts: "Welcome to Arena Fall"
- Countdown: "3... 2... 1..."
- Zone closing: "Zone closing in 60 seconds"
- Final zones: "Final zone closing"
- Kill leader: "[Player] is the kill leader"
- Winner: "Battle Royale! Winner!"

### Player Voice (Contextual)
- Taking damage: "Taking fire!"
- Enemy spotted: "Enemy spotted!"
- Grenade thrown: "Grenade out!"
- Reloading: "Reloading!"
- Downed: "I'm down! Need help!"
- Revived: "Thanks, I'm back!"

---

## 5. Audio Implementation

### Mixer Groups
```
Master
├── Music
│   └── Dynamic Layers
├── SFX
│   ├── Weapons
│   ├── Footsteps
│   ├── Impacts
│   ├── UI
│   └── Environment
├── Voice
│   ├── Announcer
│   └── Player
└── Ambient
```

### Spatial Settings
| Category | Attenuation | Reverb Zone |
|----------|-------------|-------------|
| Weapons | 300m | Yes |
| Footsteps | 30m | Yes |
| Voice | 50m | Slight |
| Ambient | 100m | Full |
| UI | 2D | None |

### Audio Optimization
- Max simultaneous SFX: 32 (PC), 16 (Mobile)
- Voice channels: 2 (PC), 1 (Mobile)
- Music channels: 2 (stereo)
- Pool all frequently used sounds
- Compress to OGG (128kbps SFX, 64kbps Ambient)
- MIP-map audio (lower quality at distance)
