/**
 * Anti-Cheat Service
 * Detects and prevents cheating through server-side validation
 */
class AntiCheatService {
  constructor(logger) {
    this.logger = logger;
    this.reports = new Map();
    this.bannedPlayers = new Set();
    this.suspiciousActivity = new Map();
    this.PING_MAX = parseInt(process.env.MAX_PING_MS) || 500;
    this.SPEED_HACK_THRESHOLD = parseFloat(process.env.SPEED_HACK_THRESHOLD) || 1.5;
    this.REPORT_THRESHOLD = 5;
    this.SUSPICION_DECAY = 300000; // 5 minutes
  }

  /**
   * Process an anti-cheat report from a client
   */
  report(socket, data) {
    const { type, details } = data || {};
    const playerId = socket.playerId;

    // Track report
    if (!this.suspiciousActivity.has(playerId)) {
      this.suspiciousActivity.set(playerId, {
        reports: [],
        suspicionScore: 0,
        firstSeen: Date.now()
      });
    }

    const activity = this.suspiciousActivity.get(playerId);
    
    const report = {
      type,
      details,
      timestamp: Date.now(),
      ping: socket.pingTime || 0
    };

    activity.reports.push(report);
    activity.suspicionScore += this.calculateSuspicion(report);

    this.logger.warn(`⚠️ AntiCheat: ${socket.playerName} - ${type} (score: ${activity.suspicionScore})`);

    // Take action based on suspicion score
    if (activity.suspicionScore >= 100) {
      this.flagPlayer(socket, 'HIGH_SUSPICION');
    }

    if (activity.suspicionScore >= 200) {
      this.tempBanPlayer(socket, 'Automatic detection - excessive violations');
    }

    // Acknowledge
    socket.emit('anticheat:ack', {
      reportId: `${playerId}_${Date.now()}`,
      action: activity.suspicionScore >= 100 ? 'flagged' : 'noted'
    });
  }

  /**
   * Validate player movement for speed hacks
   */
  validateMovement(playerId, currentPos, lastPos, deltaTime) {
    if (!lastPos || deltaTime <= 0) return true;

    const distance = this.calculateDistance(currentPos, lastPos);
    const speed = distance / deltaTime;
    const maxSpeed = 15; // Max legitimate speed (sprint + slides)

    if (speed > maxSpeed * this.SPEED_HACK_THRESHOLD) {
      this.addSuspicion(playerId, 'SPEED_HACK', speed);
      return false;
    }

    // Check for teleportation
    if (distance > 100) {
      this.addSuspicion(playerId, 'TELEPORT', distance);
      return false;
    }

    return true;
  }

  /**
   * Validate weapon fire rate
   */
  validateFireRate(playerId, weaponId, fireRate) {
    const activity = this.suspiciousActivity.get(playerId);
    if (!activity) return true;

    const now = Date.now();
    const lastFire = activity.lastFireTime?.get(weaponId) || 0;
    const minInterval = 60000 / (fireRate * 1.1); // 10% tolerance

    if (now - lastFire < minInterval) {
      this.addSuspicion(playerId, 'RATE_OF_FIRE_HACK', now - lastFire);
      return false;
    }

    if (!activity.lastFireTime) activity.lastFireTime = new Map();
    activity.lastFireTime.set(weaponId, now);
    return true;
  }

  /**
   * Validate damage numbers
   */
  validateDamage(playerId, damage, weapon, distance) {
    // Check against weapon damage tables
    const maxDamage = this.getMaxWeaponDamage(weapon);
    if (damage > maxDamage * 1.2) {
      this.addSuspicion(playerId, 'DAMAGE_HACK', damage);
      return false;
    }
    return true;
  }

  /**
   * Validate player position against zone
   */
  validateZonePosition(playerId, position, safeZone) {
    if (!safeZone) return true;

    const distFromCenter = this.calculateDistance(position, {
      x: safeZone.centerX,
      z: safeZone.centerZ
    });

    // If player is outside zone but taking no damage, suspicious
    if (distFromCenter > safeZone.radius + 2) {
      this.addSuspicion(playerId, 'ZONE_HACK', distFromCenter);
      return false;
    }
    return true;
  }

  /**
   * Ban a player permanently
   */
  banPlayer(playerId, reason) {
    this.bannedPlayers.add(playerId);
    this.logger.error(`🚫 PLAYER BANNED: ${playerId} - ${reason}`);

    // Disconnect if connected
    const socket = this.findSocketByPlayerId(playerId);
    if (socket) {
      socket.emit('anticheat:banned', { reason, permanent: true });
      socket.disconnect(true);
    }
  }

  /**
   * Temporary ban
   */
  tempBanPlayer(socket, reason) {
    this.logger.warn(`🚫 Player kicked: ${socket.playerName} - ${reason}`);
    socket.emit('anticheat:kicked', { reason, duration: 300 });
    socket.disconnect(true);
  }

  /**
   * Flag a player for review
   */
  flagPlayer(socket, reason) {
    this.logger.warn(`🚩 Player flagged: ${socket.playerName} - ${reason}`);
    socket.emit('anticheat:flagged', { reason });

    // In production: save to database for admin review
  }

  /**
   * Add suspicion score
   */
  addSuspicion(playerId, type, value) {
    if (!this.suspiciousActivity.has(playerId)) {
      this.suspiciousActivity.set(playerId, {
        reports: [],
        suspicionScore: 0,
        firstSeen: Date.now()
      });
    }

    const activity = this.suspiciousActivity.get(playerId);
    activity.reports.push({ type, value, timestamp: Date.now() });
    activity.suspicionScore += this.calculateTypeWeight(type);
  }

  /**
   * Calculate suspicion score for a report
   */
  calculateSuspicion(report) {
    const weights = {
      'SPEED_HACK': 30,
      'AIMBOT': 40,
      'WALLHACK': 35,
      'DAMAGE_HACK': 50,
      'RATE_OF_FIRE_HACK': 25,
      'TELEPORT': 45,
      'ZONE_HACK': 20,
      'FLY_HACK': 35,
      'NO_CLIP': 40
    };

    return weights[report.type] || 10;
  }

  calculateTypeWeight(type) {
    const weights = {
      'SPEED_HACK': 15,
      'AIMBOT': 20,
      'WALLHACK': 18,
      'DAMAGE_HACK': 25,
      'RATE_OF_FIRE_HACK': 12,
      'TELEPORT': 22,
      'ZONE_HACK': 10,
      'FLY_HACK': 18,
      'NO_CLIP': 20
    };
    return weights[type] || 5;
  }

  getMaxWeaponDamage(weaponId) {
    const damageTable = {
      a17_striker: 28,
      a23_phantom: 32,
      a41_vanguard: 24,
      s9_viper: 22,
      s14_stinger: 26,
      sg12_breaker: 144, // 18 * 8 pellets
      sg20_devastator: 132, // 22 * 6 pellets
      sr25_longshot: 95,
      sr40_eliminator: 110,
      lmg60_suppressor: 30,
      lmg80_storm: 34,
      p25_sidearm: 26,
      p38_heavy: 38,
      combat_knife: 50,
      energy_blade: 75,
      impact_staff: 60,
      frag_grenade: 200
    };
    return damageTable[weaponId] || 50;
  }

  calculateDistance(a, b) {
    const dx = (a.x || a.x || 0) - (b.x || b.x || 0);
    const dz = (a.z || a.z || 0) - (b.z || b.z || 0);
    return Math.sqrt(dx * dx + dz * dz);
  }

  findSocketByPlayerId(playerId) {
    // Would need io reference - in production passed via constructor
    return null;
  }

  /**
   * Get anti-cheat status for a player
   */
  getStatus(playerId) {
    return this.suspiciousActivity.get(playerId) || null;
  }

  /**
   * Cleanup old suspicions
   */
  cleanup() {
    const now = Date.now();
    for (const [playerId, activity] of this.suspiciousActivity) {
      if (now - activity.firstSeen > this.SUSPICION_DECAY) {
        this.suspiciousActivity.delete(playerId);
      }
    }
  }
}

module.exports = AntiCheatService;
