# Arena Fall — Coding Standards

## 1. C# Coding Conventions

### Naming Conventions
| Scope | Convention | Example |
|-------|------------|---------|
| Classes | PascalCase | `WeaponController` |
| Interfaces | IPascalCase | `IDamageable` |
| Methods | PascalCase | `TakeDamage()` |
| Public Fields | camelCase | `moveSpeed` |
| Private Fields | _camelCase | `_currentHealth` |
| Serialized Fields | camelCase | `[SerializeField] weaponData` |
| Properties | PascalCase | `MaxHealth { get; }` |
| Local Variables | camelCase | `damageAmount` |
| Constants | SCREAMING_SNAKE | `MAX_PLAYERS` |
| Events | PascalCase | `OnPlayerDied` |
| Event Handlers | PascalCase | `HandlePlayerDied` |
| Parameters | camelCase | `GameObject target` |

### File Organization
- One class per file (exceptions: small helper classes)
- File name matches class name
- Namespace matches folder structure
- Region blocks for organizing large classes

### Class Structure
```csharp
namespace ArenaFall.Gameplay.Weapons
{
    public class WeaponController : MonoBehaviour
    {
        // Serialized Fields
        // Private Fields
        // Properties
        // Unity Lifecycle (Awake, Start, Update, etc.)
        // Public Methods
        // Private Methods
        // Event Handlers
        // Interfaces Implementation
    }
}
```

---

## 2. Script Architecture Rules

### SOLID Principles
- **S** — Single Responsibility: Each script does one thing
- **O** — Open/Closed: Extend via ScriptableObjects, not modification
- **L** — Liskov Substitution: Derived classes usable via base interface
- **I** — Interface Segregation: Small, focused interfaces
- **D** — Dependency Inversion: Depend on abstractions, not concretions

### Dependency Injection
- Use ServiceLocator for managers
- Constructor injection for services
- Avoid static classes (use ServiceLocator instead)
- MonoBehaviour dependencies set via Inspector or DI

### Event-Driven Communication
```csharp
// Firing event
EventBus.Raise(new PlayerDiedEvent { PlayerId = id, KillerId = killerId });

// Listening
EventBus.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
EventBus.Unsubscribe<PlayerDiedEvent>(HandlePlayerDied);
```

---

## 3. Performance Rules

### General
- No allocations in Update/FixedUpdate
- Cache component references in Awake
- Use object pools for frequent spawns
- Avoid expensive operations per frame
- Use LOD and culling where possible

### Strings
- Use StringBuilder for concatenation
- Cache `ToString()` calls
- Use `nameof()` for reflection
- Avoid string comparisons in Update

### Collections
- Use Array or List<T> over ArrayList
- Specify capacity when known
- Use for loops over foreach (performance critical)
- Pool frequently created lists

### Physics
- Use non-allocating physics queries
- Cache RaycastHit arrays
- Use layer masks for filtering
- Batch raycast calls

---

## 4. Network Code Standards

### NetworkBehaviours
- Minimize NetworkVariable count
- Use RPCs for events, NetworkVariables for state
- Server is authoritative
- Validate inputs on server
- Use NetworkAnimator for animation sync

### RPC Naming
- `ServerRpc_Prefix` for server RPCs
- `ClientRpc_Prefix` for client RPCs
- `Rpc_Prefix` for both directions

### State Sync
```csharp
[ServerRpc]
private void ServerRpc_FireWeapon(Vector3 aimPoint, float chargeTime) 
{
    // Validate
    if (!CanFire()) return;
    // Process
    FireProjectile(aimPoint, chargeTime);
    // Broadcast
    ClientRpc_OnWeaponFired(aimPoint);
}

[ClientRpc]
private void ClientRpc_OnWeaponFired(Vector3 aimPoint)
{
    // Play effects
    _muzzleFlash.Play();
    _audioSource.PlayOneShot(_fireSound);
}
```

---

## 5. Documentation Rules

### Inline Comments
- Explain WHY, not WHAT
- XML comments on all public APIs
- Summary for every class and public method

### Code Documentation
```csharp
/// <summary>
/// Controls weapon firing, reloading, and attachment behavior.
/// Uses data from WeaponData ScriptableObject for configuration.
/// </summary>
public class WeaponController : NetworkBehaviour
{
    /// <summary>
    /// Fires the weapon towards the target point.
    /// Server authoritative - validates ammo and fire rate.
    /// </summary>
    /// <param name="aimPoint">World position of aim target</param>
    /// <param name="fireMode">Current fire mode index</param>
    [ServerRpc]
    private void ServerRpc_Fire(Vector3 aimPoint, int fireMode) { }
}
```

---

## 6. Version Control

### Commit Messages
Format: `[Category] Brief description`

Categories:
- `[FEATURE]` — New feature
- `[FIX]` — Bug fix
- `[REFACTOR]` — Code restructuring
- `[OPTIMIZE]` — Performance improvement
- `[UI]` — UI changes
- `[AUDIO]` — Audio changes
- `[ART]` — Art/asset changes
- `[NETCODE]` — Network code changes
- `[DOCS]` — Documentation
- `[TEST]` — Test additions/changes

### Branch Strategy
- `main` — Release ready
- `develop` — Integration branch
- `feature/feature-name` — Feature work
- `bugfix/bug-name` — Bug fixes
- `release/version` — Release preparation
