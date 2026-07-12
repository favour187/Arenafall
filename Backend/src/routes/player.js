const express = require('express');
const { authenticate } = require('../middleware/auth');
const Player = require('../models/Player');
const { logger } = require('../server');

const router = express.Router();
global.memoryStore = global.memoryStore || { players: new Map() };

// ─── GET /profile (Authenticated Player) ────────────────────────
router.get('/profile', authenticate, async (req, res) => {
  try {
    if (require('mongoose').connection.readyState !== 1) {
      let memPlayer = global.memoryStore?.players.get(req.player?.email) || global.memoryStore?.players.get(req.player?.username);
      if (!memPlayer) {
        memPlayer = {
          playerId: req.player?.playerId || 'player_1',
          playerName: req.player?.username || 'Vanguard_Soldier',
          username: req.player?.username || 'Vanguard_Soldier',
          level: req.player?.level || 15,
          credits: 2450,
          xp: 850
        };
      }
      return res.json({ success: true, data: memPlayer });
    }

    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) return res.status(404).json({ success: false, error: 'Player not found' });
    return res.json({ success: true, data: player });
  } catch (err) {
    return res.status(500).json({ success: false, error: err.message });
  }
});

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

// ─── POST /stats (Match Completion Sync) ────────────────────────
router.post('/stats', authenticate, async (req, res) => {
  try {
    const { kills = 0, damageDealt = 0, placement = 1, survivalTime = 0 } = req.body;
    const earnedXP = (kills * 150) + (placement === 1 ? 500 : 200);
    const earnedCredits = (kills * 50) + (placement === 1 ? 250 : 100);

    if (require('mongoose').connection.readyState !== 1) {
      let memPlayer = global.memoryStore?.players.get(req.player?.email) || global.memoryStore?.players.get(req.player?.username);
      if (memPlayer) {
        memPlayer.xp = (memPlayer.xp || 0) + earnedXP;
        memPlayer.credits = (memPlayer.credits || 0) + earnedCredits;
        if (memPlayer.xp >= memPlayer.level * 1000) {
          memPlayer.level += 1;
          memPlayer.xp -= (memPlayer.level - 1) * 1000;
        }
      }
      return res.json({ success: true, message: 'Stats synced (In-Memory)', earnedXP, earnedCredits });
    }

    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) return res.status(404).json({ success: false, error: 'Player not found' });

    player.stats.kills = (player.stats.kills || 0) + kills;
    player.stats.damageDealt = (player.stats.damageDealt || 0) + damageDealt;
    player.stats.matchesPlayed = (player.stats.matchesPlayed || 0) + 1;
    if (placement === 1) player.stats.wins = (player.stats.wins || 0) + 1;

    player.xp = (player.xp || 0) + earnedXP;
    player.credits = (player.credits || 0) + earnedCredits;
    if (player.xp >= player.level * 1000) {
      player.level += 1;
      player.xp -= (player.level - 1) * 1000;
    }
    await player.save();

    return res.json({ success: true, message: 'Stats synced to DB', earnedXP, earnedCredits });
  } catch (err) {
    return res.status(500).json({ success: false, error: err.message });
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
