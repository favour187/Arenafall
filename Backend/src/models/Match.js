const mongoose = require('mongoose');
const crypto = require('crypto');

const matchSchema = new mongoose.Schema({
  matchId: {
    type: String,
    unique: true,
    default: () => `match_${crypto.randomBytes(6).toString('hex')}`
  },
  // ─── Match Info ────────────────────────────────────────────
  mode: {
    type: String,
    enum: ['solo', 'duos', 'squads', 'training'],
    required: true
  },
  map: { type: String, default: 'arena7' },
  status: {
    type: String,
    enum: ['waiting', 'in_progress', 'finished', 'cancelled'],
    default: 'waiting'
  },
  region: { type: String, default: 'auto' },

  // ─── Timing ────────────────────────────────────────────────
  startedAt: { type: Date },
  endedAt: { type: Date },
  duration: { type: Number }, // seconds
  tickRate: { type: Number, default: 20 },

  // ─── Zone ──────────────────────────────────────────────────
  safeZoneStages: [{
    centerX: Number,
    centerZ: Number,
    radius: Number,
    shrinkDuration: Number,
    warningDuration: Number,
    damagePerTick: Number
  }],

  // ─── Players ───────────────────────────────────────────────
  playerCount: { type: Number, default: 0 },
  maxPlayers: { type: Number, default: 60 },
  players: [{
    playerId: { type: String, ref: 'Player' },
    username: String,
    placement: Number,
    kills: { type: Number, default: 0 },
    deaths: { type: Number, default: 0 },
    assists: { type: Number, default: 0 },
    damageDealt: { type: Number, default: 0 },
    damageTaken: { type: Number, default: 0 },
    headshots: { type: Number, default: 0 },
    longestKill: { type: Number, default: 0 },
    revives: { type: Number, default: 0 },
    vehiclesDestroyed: { type: Number, default: 0 },
    survivalTime: { type: Number, default: 0 },
    distanceTraveled: { type: Number, default: 0 },
    weaponsUsed: [{
      weaponId: String,
      kills: Number,
      damage: Number,
      shots: Number,
      hits: Number
    }],
    xpEarned: { type: Number, default: 0 },
    creditsEarned: { type: Number, default: 0 },
    eliminatedBy: String,
    eliminatedAt: Date
  }],

  // ─── Events ────────────────────────────────────────────────
  killFeed: [{
    timestamp: Number,
    killerId: String,
    victimId: String,
    weaponId: String,
    headshot: Boolean,
    distance: Number
  }],

  // ─── Server Info ───────────────────────────────────────────
  serverId: { type: String },
  serverVersion: { type: String },
  checksum: { type: String }

}, {
  timestamps: true
});

// ─── Indexes ────────────────────────────────────────────────────
matchSchema.index({ matchId: 1 });
matchSchema.index({ status: 1, createdAt: -1 });
matchSchema.index({ 'players.playerId': 1 });
matchSchema.index({ mode: 1, createdAt: -1 });
matchSchema.index({ startedAt: -1 });

// ─── Methods ────────────────────────────────────────────────────
matchSchema.methods.addPlayer = function(playerData) {
  if (this.players.length >= this.maxPlayers) return false;
  this.players.push({
    playerId: playerData.playerId,
    username: playerData.username
  });
  this.playerCount = this.players.length;
  return true;
};

matchSchema.methods.recordKill = function(killerId, victimId, weaponId, headshot, distance) {
  const killer = this.players.find(p => p.playerId === killerId);
  const victim = this.players.find(p => p.playerId === victimId);
  if (killer) {
    killer.kills++;
    if (distance > killer.longestKill) killer.longestKill = distance;
  }
  if (victim) {
    victim.deaths++;
    victim.eliminatedBy = killerId;
    victim.eliminatedAt = new Date();
    victim.placement = this.players.filter(p => !p.eliminatedAt).length;
  }
  this.killFeed.push({
    timestamp: Date.now(),
    killerId,
    victimId,
    weaponId,
    headshot,
    distance
  });
};

matchSchema.methods.getWinner = function() {
  return this.players.find(p => p.placement === 1) || null;
};

module.exports = mongoose.model('Match', matchSchema);
