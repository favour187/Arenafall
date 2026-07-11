const express = require('express');
const { authenticate } = require('../middleware/auth');
const Player = require('../models/Player');
const { logger } = require('../server');

const router = express.Router();

// ─── GET /profile/:playerId ─────────────────────────────────────
router.get('/profile/:playerId', async (req, res) => {
  try {
    const player = await Player.findOne({ playerId: req.params.playerId });
    if (!player) {
      return res.status(404).json({ error: 'Player not found', code: 'NOT_FOUND' });
    }
    res.json({
      playerId: player.playerId,
      username: player.username,
      displayName: player.displayName,
      level: player.level,
      title: player.title,
      selectedCharacter: player.selectedCharacter,
      banner: player.banner,
      stats: {
        matchesPlayed: player.stats.matchesPlayed,
        wins: player.stats.wins,
        kills: player.stats.kills,
        deaths: player.stats.deaths,
        winRate: player.winRate,
        kdr: player.kdr
      },
      createdAt: player.createdAt
    });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get profile', code: 'SERVER_ERROR' });
  }
});

// ─── PUT /profile ───────────────────────────────────────────────
router.put('/profile', authenticate, async (req, res) => {
  try {
    const updates = {};
    if (req.body.displayName) updates.displayName = req.body.displayName;
    if (req.body.selectedCharacter) updates.selectedCharacter = req.body.selectedCharacter;
    if (req.body.title) updates.title = req.body.title;
    if (req.body.banner) updates.banner = req.body.banner;

    const player = await Player.findOneAndUpdate(
      { playerId: req.player.playerId },
      { $set: updates },
      { new: true }
    );

    res.json({ message: 'Profile updated', player });
  } catch (err) {
    res.status(500).json({ error: 'Failed to update profile', code: 'SERVER_ERROR' });
  }
});

// ─── GET /stats ─────────────────────────────────────────────────
router.get('/stats', authenticate, async (req, res) => {
  try {
    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) return res.status(404).json({ error: 'Not found', code: 'NOT_FOUND' });

    res.json({
      career: player.stats,
      winRate: player.winRate,
      kdr: player.kdr,
      level: player.level,
      xp: player.xp,
      totalXp: player.totalXp,
      prestige: player.prestige
    });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get stats', code: 'SERVER_ERROR' });
  }
});

// ─── GET /inventory ─────────────────────────────────────────────
router.get('/inventory', authenticate, async (req, res) => {
  try {
    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) return res.status(404).json({ error: 'Not found', code: 'NOT_FOUND' });

    res.json({
      characters: player.ownedCharacters,
      weaponSkins: player.ownedWeaponSkins,
      emotes: player.ownedEmotes,
      skins: player.ownedSkins,
      banners: player.ownedBanners,
      credits: player.credits,
      premiumCurrency: player.premiumCurrency
    });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get inventory', code: 'SERVER_ERROR' });
  }
});

// ─── GET /loadouts ──────────────────────────────────────────────
router.get('/loadouts', authenticate, async (req, res) => {
  try {
    const player = await Player.findOne({ playerId: req.player.playerId });
    res.json({ loadouts: player?.loadouts || [] });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get loadouts', code: 'SERVER_ERROR' });
  }
});

// ─── PUT /loadouts ──────────────────────────────────────────────
router.put('/loadouts', authenticate, async (req, res) => {
  try {
    const { loadoutId, loadout } = req.body;
    const player = await Player.findOne({ playerId: req.player.playerId });

    if (loadoutId !== undefined && player.loadouts[loadoutId]) {
      player.loadouts[loadoutId] = { ...player.loadouts[loadoutId].toObject(), ...loadout };
    } else {
      player.loadouts.push(loadout);
    }

    await player.save();
    res.json({ loadouts: player.loadouts });
  } catch (err) {
    res.status(500).json({ error: 'Failed to update loadout', code: 'SERVER_ERROR' });
  }
});

// ─── POST /xp/add ───────────────────────────────────────────────
router.post('/xp/add', authenticate, async (req, res) => {
  try {
    const { amount, source } = req.body;
    if (!amount || amount < 0 || amount > 10000) {
      return res.status(400).json({ error: 'Invalid XP amount', code: 'INVALID_AMOUNT' });
    }

    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) return res.status(404).json({ error: 'Not found', code: 'NOT_FOUND' });

    const oldLevel = player.level;
    player.addXp(amount);
    await player.save();

    res.json({
      xpAdded: amount,
      totalXp: player.totalXp,
      level: player.level,
      leveledUp: player.level > oldLevel
    });
  } catch (err) {
    res.status(500).json({ error: 'Failed to add XP', code: 'SERVER_ERROR' });
  }
});

// ─── POST /save ─────────────────────────────────────────────────
router.post('/save', authenticate, async (req, res) => {
  try {
    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) return res.status(404).json({ error: 'Not found', code: 'NOT_FOUND' });

    // Allow saving specific fields
    if (req.body.settings) Object.assign(player.settings, req.body.settings);
    if (req.body.loadouts) player.loadouts = req.body.loadouts;

    await player.save();
    res.json({ message: 'Save data stored', timestamp: new Date().toISOString() });
  } catch (err) {
    res.status(500).json({ error: 'Failed to save', code: 'SERVER_ERROR' });
  }
});

// ─── GET /search ────────────────────────────────────────────────
router.get('/search', authenticate, async (req, res) => {
  try {
    const { q } = req.query;
    if (!q || q.length < 2) {
      return res.status(400).json({ error: 'Search query too short', code: 'INVALID_QUERY' });
    }

    const players = await Player.find({
      $or: [
        { username: { $regex: q, $options: 'i' } },
        { displayName: { $regex: q, $options: 'i' } }
      ]
    })
    .limit(20)
    .select('username displayName level title selectedCharacter');

    res.json({ results: players });
  } catch (err) {
    res.status(500).json({ error: 'Search failed', code: 'SERVER_ERROR' });
  }
});

module.exports = router;
