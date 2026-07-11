# Arena Fall — UI Style Guide

## 1. Design Principles

- **Clarity First** — Information must be readable at a glance
- **Minimalist** — Show only what's needed, hide the rest
- **Sci-Fi Identity** — Holographic, clean, tech-forward aesthetic
- **Responsive** — Scales across PC and mobile
- **Accessible** — High contrast, scalable text, colorblind friendly

---

## 2. Typography

### Primary Font: "Orbitron" (or similar sci-fi sans-serif)
- **Weights:** Regular, Medium, Bold
- **Usage:** Headlines, titles, key numbers

### Secondary Font: "Inter" (or similar clean sans-serif)
- **Weights:** Regular, Medium, SemiBold
- **Usage:** Body text, descriptions, menus

### Monospace Font: "JetBrains Mono"
- **Usage:** Damage numbers, timers, coordinates

### Font Sizing
| Element | Size | Weight |
|---------|------|--------|
| Game Title | 48px | Bold |
| Section Header | 32px | Bold |
| Sub Header | 24px | Medium |
| Body Text | 16px | Regular |
| Small Text | 12px | Regular |
| Damage Number | 24px | Bold Mono |

---

## 3. Color System

### Semantic Colors
- **Primary:** #00D4FF (Cyan) — Interactive, highlights
- **Secondary:** #FF6B35 (Orange) — CTAs, important
- **Success:** #44FF55 (Green) — Heals, positive actions
- **Warning:** #FFB800 (Amber) — Caution, medium threat
- **Danger:** #FF2244 (Red) — Damage, enemies, death
- **Info:** #4488FF (Blue) — Information, friends

### UI Surface Colors
| Element | Color | Opacity |
|---------|-------|---------|
| Panel Background | #0A1628 | 90% |
| Card Background | #1E3A5F | 85% |
| Input Background | #0D1B2E | 80% |
| Tooltip Background | #1A2A4A | 95% |
| Overlay | #000000 | 60% |

### Text Colors
- **Primary Text:** #FFFFFF
- **Secondary Text:** #B0B8C4
- **Disabled Text:** #607080
- **Link Text:** #00D4FF

---

## 4. Component Library

### Buttons

**Primary Button**
```
Background: #FF6B35
Text: #FFFFFF
Hover: #FF8555
Press: #CC552A
Disabled: #4A3A2A
Border Radius: 6px
Padding: 12px 24px
```

**Secondary Button**
```
Background: Transparent
Border: 1px solid #00D4FF
Text: #00D4FF
Hover: rgba(0,212,255,0.1)
Press: rgba(0,212,255,0.2)
```

**Icon Button**
```
Size: 40x40px
Background: Transparent
Hover: rgba(255,255,255,0.1)
Icon Color: #B0B8C4
```

### Input Fields
```
Background: #0D1B2E
Border: 1px solid #1E3A5F
Text: #FFFFFF
Placeholder: #607080
Focus Border: #00D4FF
Padding: 10px 16px
```

### Cards
```
Background: #1E3A5F (85%)
Border: 1px solid rgba(0,212,255,0.2)
Border Radius: 8px
Padding: 16px
Shadow: 0 4px 12px rgba(0,0,0,0.3)
```

### Progress Bars
```
Height: 6px
Background: #1E3A5F
Fill: #00D4FF
Border Radius: 3px
```

### Tabs
```
Active Tab: Border bottom 2px #00D4FF
Inactive Tab: #607080 text
Hover Tab: #B0B8C4 text
```

---

## 5. HUD Layout

### Top Bar
```
[Team Info - Left]          [Alive: 45/100 - Center]     [Minimap - Right]
```

### Bottom Center
```
[Health Bar] [Shield Bar]
[Weapon: A-17 Striker] [Ammo: 30/120]
```

### Bottom Right
```
[Grenade] [Heal] [Ping] [Emote]
```

### Compass
```
Top Center, above minimap area
[ N ] [ NE ] [ E ] [ SE ] [ S ] [ SW ] [ W ] [ NW ]
```

### Kill Feed
```
Top Right
[Player] → [Player] (Weapon)
[Player] → [Player] (Weapon)
```

---

## 6. Screen Flow

```
Boot → Logo → Login → MainMenu → Lobby → Matchmaking → Game → Result
                                     ↓
                                Customization
                                Loadout
                                Settings
                                Training
```

---

## 7. Animation & Transitions

- **Panel transitions:** Slide in from right, 0.3s ease-out
- **Modal fade:** 0.2s fade in/out
- **Button hover:** Subtle scale 1.02
- **Button click:** Scale 0.95 flash
- **Notifications:** Slide down from top, 0.5s, auto-dismiss 3s
- **Tooltips:** Fade in 0.15s, delay 0.5s

---

## 8. Mobile Adaptations

- Larger touch targets (min 44x44px)
- Bottom-anchored navigation
- Simplified HUD (fewer elements)
- Gesture support (swipe to reload, etc.)
- Safe area awareness (notch support)
- Portrait/landscape support (preferred: landscape)
