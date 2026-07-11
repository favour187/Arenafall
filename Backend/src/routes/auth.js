const express = require('express');
const jwt = require('jsonwebtoken');
const crypto = require('crypto');
const { body, validationResult } = require('express-validator');
const { authenticate } = require('../middleware/auth');
const Player = require('../models/Player');
const { logger } = require('../server');

const router = express.Router();

// ─── Helpers ────────────────────────────────────────────────────
const generateTokens = (player) => {
  const secret = process.env.JWT_SECRET || 'arenafall-dev-secret-key-2024';
  const refreshSecret = process.env.JWT_REFRESH_SECRET || 'arenafall-refresh-secret';

  const accessToken = jwt.sign(
    {
      playerId: player.playerId,
      username: player.username,
      email: player.email,
      level: player.level
    },
    secret,
    { expiresIn: process.env.JWT_EXPIRY || '24h' }
  );

  const refreshToken = jwt.sign(
    { playerId: player.playerId },
    refreshSecret,
    { expiresIn: process.env.JWT_REFRESH_EXPIRY || '7d' }
  );

  return { accessToken, refreshToken };
};

const sanitizePlayer = (player) => ({
  playerId: player.playerId,
  username: player.username,
  displayName: player.displayName,
  email: player.email,
  level: player.level,
  xp: player.xp,
  credits: player.credits,
  premiumCurrency: player.premiumCurrency,
  selectedCharacter: player.selectedCharacter,
  title: player.title,
  stats: player.stats,
  loadouts: player.loadouts,
  ownedCharacters: player.ownedCharacters,
  ownedSkins: player.ownedSkins,
  ownedEmotes: player.ownedEmotes,
  battlePass: player.battlePass,
  settings: player.settings,
  createdAt: player.createdAt
});

// ─── POST /register ─────────────────────────────────────────────
router.post('/register', [
  body('username').trim().isLength({ min: 3, max: 24 }).matches(/^[a-zA-Z0-9_-]+$/),
  body('email').isEmail().normalizeEmail(),
  body('password').isLength({ min: 8, max: 128 })
    .matches(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/, 'Password must contain uppercase, lowercase, and number')
], async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ error: 'Validation failed', code: 'VALIDATION_ERROR', errors: errors.array() });
    }

    const { username, email, password } = req.body;

    // Check existing
    const existingUser = await Player.findOne({
      $or: [{ username: username.toLowerCase() }, { email }]
    });
    if (existingUser) {
      const field = existingUser.username === username.toLowerCase() ? 'Username' : 'Email';
      return res.status(409).json({ error: `${field} already taken`, code: 'DUPLICATE' });
    }

    // Create player
    const player = new Player({
      username: username.toLowerCase(),
      email,
      passwordHash: password, // Will be hashed by pre-save hook
      displayName: username,
      ownedCharacters: ['vanguard'],
      loadouts: [{
        name: 'Default',
        character: 'vanguard',
        primaryWeapon: 'a17_striker',
        secondaryWeapon: 'p25_sidearm',
        melee: 'combat_knife',
        throwable: 'frag_grenade'
      }]
    });

    await player.save();
    const tokens = generateTokens(player);

    logger.info(`👤 New player registered: ${username}`);
    res.status(201).json({
      message: 'Registration successful',
      player: sanitizePlayer(player),
      ...tokens
    });
  } catch (err) {
    logger.error('Registration error:', err);
    res.status(500).json({ error: 'Registration failed', code: 'SERVER_ERROR' });
  }
});

// ─── POST /login ────────────────────────────────────────────────
router.post('/login', [
  body('username').trim().notEmpty(),
  body('password').notEmpty()
], async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ error: 'Validation failed', code: 'VALIDATION_ERROR' });
    }

    const { username, password } = req.body;

    const player = await Player.findOne({
      $or: [
        { username: username.toLowerCase() },
        { email: username.toLowerCase() }
      ]
    }).select('+passwordHash');

    if (!player) {
      return res.status(401).json({ error: 'Invalid credentials', code: 'INVALID_CREDENTIALS' });
    }

    if (player.isBanned) {
      return res.status(403).json({
        error: 'Account suspended',
        code: 'BANNED',
        reason: player.banReason || 'Violation of terms of service'
      });
    }

    const validPassword = await player.comparePassword(password);
    if (!validPassword) {
      return res.status(401).json({ error: 'Invalid credentials', code: 'INVALID_CREDENTIALS' });
    }

    player.lastLogin = new Date();
    player.lastIp = req.ip;
    await player.save();

    const tokens = generateTokens(player);

    logger.info(`🔑 Player logged in: ${username}`);
    res.json({
      message: 'Login successful',
      player: sanitizePlayer(player),
      ...tokens
    });
  } catch (err) {
    logger.error('Login error:', err);
    res.status(500).json({ error: 'Login failed', code: 'SERVER_ERROR' });
  }
});

// ─── POST /refresh ──────────────────────────────────────────────
router.post('/refresh', async (req, res) => {
  try {
    const { refreshToken } = req.body;
    if (!refreshToken) {
      return res.status(400).json({ error: 'Refresh token required', code: 'TOKEN_REQUIRED' });
    }

    const refreshSecret = process.env.JWT_REFRESH_SECRET || 'arenafall-refresh-secret';
    const decoded = jwt.verify(refreshToken, refreshSecret);

    const player = await Player.findOne({ playerId: decoded.playerId });
    if (!player) {
      return res.status(401).json({ error: 'Player not found', code: 'PLAYER_NOT_FOUND' });
    }

    const tokens = generateTokens(player);
    res.json({ message: 'Token refreshed', ...tokens });
  } catch (err) {
    if (err.name === 'TokenExpiredError') {
      return res.status(401).json({ error: 'Refresh token expired', code: 'REFRESH_EXPIRED' });
    }
    return res.status(401).json({ error: 'Invalid refresh token', code: 'INVALID_TOKEN' });
  }
});

// ─── GET /me ────────────────────────────────────────────────────
router.get('/me', authenticate, async (req, res) => {
  try {
    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) {
      return res.status(404).json({ error: 'Player not found', code: 'NOT_FOUND' });
    }
    res.json({ player: sanitizePlayer(player) });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get profile', code: 'SERVER_ERROR' });
  }
});

// ─── PUT /settings ──────────────────────────────────────────────
router.put('/settings', authenticate, async (req, res) => {
  try {
    const allowed = ['masterVolume', 'musicVolume', 'sfxVolume', 'sensitivity', 'invertY', 'colorblindMode', 'crosshair'];
    const updates = {};
    
    for (const key of allowed) {
      if (req.body[key] !== undefined) {
        updates[`settings.${key}`] = req.body[key];
      }
    }

    const player = await Player.findOneAndUpdate(
      { playerId: req.player.playerId },
      { $set: updates },
      { new: true }
    );

    if (!player) {
      return res.status(404).json({ error: 'Player not found', code: 'NOT_FOUND' });
    }

    res.json({ message: 'Settings updated', settings: player.settings });
  } catch (err) {
    res.status(500).json({ error: 'Failed to update settings', code: 'SERVER_ERROR' });
  }
});

// ─── POST /logout ───────────────────────────────────────────────
router.post('/logout', authenticate, (req, res) => {
  res.json({ message: 'Logged out successfully' });
});

// ─── DELETE /account ────────────────────────────────────────────
router.delete('/account', authenticate, async (req, res) => {
  try {
    await Player.findOneAndDelete({ playerId: req.player.playerId });
    logger.info(`🗑️ Account deleted: ${req.player.username}`);
    res.json({ message: 'Account deleted' });
  } catch (err) {
    res.status(500).json({ error: 'Failed to delete account', code: 'SERVER_ERROR' });
  }
});

module.exports = router;
