const mongoose = require('mongoose');
const bcrypt = require('bcryptjs');
const crypto = require('crypto');

const playerSchema = new mongoose.Schema({
  // ─── Identity ──────────────────────────────────────────────
  username: {
    type: String,
    required: [true, 'Username is required'],
    unique: true,
    trim: true,
    minlength: [3, 'Username must be at least 3 characters'],
    maxlength: [24, 'Username max 24 characters'],
    match: [/^[a-zA-Z0-9_-]+$/, 'Username can only contain letters, numbers, hyphens and underscores']
  },
  email: {
    type: String,
    required: [true, 'Email is required'],
    unique: true,
    lowercase: true,
    match: [/^\S+@\S+\.\S+$/, 'Invalid email format']
  },
  passwordHash: {
    type: String,
    required: true,
    select: false
  },
  playerId: {
    type: String,
    unique: true,
    default: () => crypto.randomBytes(8).toString('hex')
  },

  // ─── Profile ───────────────────────────────────────────────
  displayName: { type: String, default: '' },
  avatarUrl: { type: String, default: '' },
  selectedCharacter: { type: String, default: 'vanguard' },
  title: { type: String, default: 'Recruit' },
  banner: { type: String, default: 'default' },

  // ─── Progression ───────────────────────────────────────────
  level: { type: Number, default: 1 },
  xp: { type: Number, default: 0 },
  totalXp: { type: Number, default: 0 },
  prestige: { type: Number, default: 0 },

  // ─── Currency ──────────────────────────────────────────────
  credits: { type: Number, default: 0 },
  premiumCurrency: { type: Number, default: 0 },
  totalEarnedCredits: { type: Number, default: 0 },

  // ─── Battle Pass ───────────────────────────────────────────
  battlePass: {
    tier: { type: Number, default: 1 },
    xp: { type: Number, default: 0 },
    premium: { type: Boolean, default: false },
    claimedTiers: [{ type: Number }],
    claimedPremium: [{ type: Number }]
  },

  // ─── Career Stats ──────────────────────────────────────────
  stats: {
    matchesPlayed: { type: Number, default: 0 },
    wins: { type: Number, default: 0 },
    top10: { type: Number, default: 0 },
    kills: { type: Number, default: 0 },
    deaths: { type: Number, default: 0 },
    assists: { type: Number, default: 0 },
    damageDealt: { type: Number, default: 0 },
    damageTaken: { type: Number, default: 0 },
    headshots: { type: Number, default: 0 },
    longestKill: { type: Number, default: 0 },
    revives: { type: Number, default: 0 },
    vehiclesDestroyed: { type: Number, default: 0 },
    totalPlayTime: { type: Number, default: 0 },
    longestSurvivalTime: { type: Number, default: 0 },
    totalDistanceTraveled: { type: Number, default: 0 }
  },

  // ─── Loadouts ──────────────────────────────────────────────
  loadouts: [{
    name: { type: String, default: 'Default' },
    character: { type: String, default: 'vanguard' },
    primaryWeapon: { type: String, default: 'a17_striker' },
    secondaryWeapon: { type: String, default: 'p25_sidearm' },
    melee: { type: String, default: 'combat_knife' },
    throwable: { type: String, default: 'frag_grenade' },
    primaryAttachments: [{ type: String }],
    secondaryAttachments: [{ type: String }]
  }],

  // ─── Inventory ─────────────────────────────────────────────
  ownedCharacters: [{ type: String }],
  ownedWeaponSkins: [{ type: String }],
  ownedEmotes: [{ type: String }],
  ownedSkins: [{ type: String }],
  ownedBanners: [{ type: String }],
  unlockedAchievements: [{ type: String }],

  // ─── Missions ──────────────────────────────────────────────
  missions: {
    daily: [{
      missionId: String,
      progress: { type: Number, default: 0 },
      target: { type: Number, default: 1 },
      completed: { type: Boolean, default: false },
      claimed: { type: Boolean, default: false },
      expiresAt: Date
    }],
    weekly: [{
      missionId: String,
      progress: { type: Number, default: 0 },
      target: { type: Number, default: 1 },
      completed: { type: Boolean, default: false },
      claimed: { type: Boolean, default: false },
      expiresAt: Date
    }]
  },

  // ─── Security ──────────────────────────────────────────────
  lastLogin: { type: Date },
  lastIp: { type: String },
  refreshTokens: [{ type: String }],
  isBanned: { type: Boolean, default: false },
  banReason: { type: String },
  twoFactorEnabled: { type: Boolean, default: false },
  twoFactorSecret: { type: String, select: false },

  // ─── Device & Session ──────────────────────────────────────
  deviceId: { type: String },
  platform: { type: String, enum: ['pc', 'android', 'ios', 'console'] },

  // ─── Settings ──────────────────────────────────────────────
  settings: {
    masterVolume: { type: Number, default: 0.8 },
    musicVolume: { type: Number, default: 0.7 },
    sfxVolume: { type: Number, default: 0.8 },
    sensitivity: { type: Number, default: 5.0 },
    invertY: { type: Boolean, default: false },
    colorblindMode: { type: Boolean, default: false },
    crosshair: { type: String, default: 'default' }
  }

}, {
  timestamps: true,
  toJSON: { virtuals: true },
  toObject: { virtuals: true }
});

// ─── Indexes ────────────────────────────────────────────────────
playerSchema.index({ username: 1 });
playerSchema.index({ email: 1 });
playerSchema.index({ playerId: 1 });
playerSchema.index({ 'stats.wins': -1 });
playerSchema.index({ 'stats.kills': -1 });
playerSchema.index({ level: -1 });

// ─── Virtuals ───────────────────────────────────────────────────
playerSchema.virtual('winRate').get(function() {
  if (this.stats.matchesPlayed === 0) return 0;
  return (this.stats.wins / this.stats.matchesPlayed * 100).toFixed(1);
});

playerSchema.virtual('kdr').get(function() {
  if (this.stats.deaths === 0) return this.stats.kills;
  return (this.stats.kills / this.stats.deaths).toFixed(2);
});

playerSchema.virtual('accuracy').get(function() {
  return 0; // Calculated from weapon stats
});

// ─── Pre-save Hooks ─────────────────────────────────────────────
playerSchema.pre('save', async function(next) {
  if (this.isModified('passwordHash')) {
    this.passwordHash = await bcrypt.hash(this.passwordHash, 12);
  }
  if (this.isModified('credits') && this.credits > this.totalEarnedCredits) {
    this.totalEarnedCredits = this.credits;
  }
  next();
});

// ─── Instance Methods ──────────────────────────────────────────
playerSchema.methods.comparePassword = async function(password) {
  return bcrypt.compare(password, this.passwordHash);
};

playerSchema.methods.addXp = function(amount) {
  this.xp += amount;
  this.totalXp += amount;
  const xpForNext = Math.floor(100 * Math.pow(1.15, this.level));
  while (this.xp >= xpForNext) {
    this.xp -= xpForNext;
    this.level++;
  }
};

playerSchema.methods.addStats = function(matchStats) {
  this.stats.matchesPlayed++;
  if (matchStats.placement === 1) this.stats.wins++;
  if (matchStats.placement <= 10) this.stats.top10++;
  this.stats.kills += matchStats.kills || 0;
  this.stats.deaths += matchStats.deaths || 0;
  this.stats.assists += matchStats.assists || 0;
  this.stats.damageDealt += matchStats.damageDealt || 0;
  this.stats.damageTaken += matchStats.damageTaken || 0;
  this.stats.headshots += matchStats.headshots || 0;
  this.stats.revives += matchStats.revives || 0;
  if (matchStats.longestKill > this.stats.longestKill) {
    this.stats.longestKill = matchStats.longestKill;
  }
  this.stats.totalPlayTime += matchStats.survivalTime || 0;
  if (matchStats.survivalTime > this.stats.longestSurvivalTime) {
    this.stats.longestSurvivalTime = matchStats.survivalTime;
  }
};

// ─── Static Methods ─────────────────────────────────────────────
playerSchema.statics.getLeaderboard = async function(category = 'wins', limit = 100) {
  const validCategories = {
    wins: { 'stats.wins': -1 },
    kills: { 'stats.kills': -1 },
    damage: { 'stats.damageDealt': -1 },
    kdr: {},
    level: { level: -1 },
    score: {}
  };

  const sort = validCategories[category] || { 'stats.wins': -1 };
  return this.find({ isBanned: false })
    .sort(sort)
    .limit(limit)
    .select('username displayName level stats credits playerId');
};

module.exports = mongoose.model('Player', playerSchema);
