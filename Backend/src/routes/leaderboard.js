const express = require('express');
const { optionalAuth } = require('../middleware/auth');
const Player = require('../models/Player');

const router = express.Router();

// Redis client — will be assigned by server.js after initialization
let redisClient = null;
function setRedis(client) { redisClient = client; }

// ─── Cache keys ─────────────────────────────────────────────────
const LEADERBOARD_CACHE_KEY = 'arenafall:leaderboard';
const CACHE_TTL = 120; // 2 minutes

// ─── GET / ──────────────────────────────────────────────────────
router.get('/', optionalAuth, async (req, res) => {
  try {
    const category = req.query.category || 'wins';
    const limit = Math.min(parseInt(req.query.limit) || 100, 500);
    const offset = parseInt(req.query.offset) || 0;

    // Try cache first
    if (redisClient?.status === 'ready') {
      const cacheKey = `${LEADERBOARD_CACHE_KEY}:${category}:${limit}:${offset}`;
      const cached = await redisClient.get(cacheKey);
      if (cached) {
        const data = JSON.parse(cached);
        return res.json({ ...data, cached: true });
      }
    }

    // Build query based on category
    let sort = {};
    let projection = 'username displayName level title playerId';

    switch (category) {
      case 'wins':
        sort = { 'stats.wins': -1, 'stats.kills': -1 };
        projection += ' stats.wins stats.kills stats.matchesPlayed';
        break;
      case 'kills':
        sort = { 'stats.kills': -1, 'stats.matchesPlayed': 1 };
        projection += ' stats.kills stats.deaths stats.matchesPlayed';
        break;
      case 'damage':
        sort = { 'stats.damageDealt': -1 };
        projection += ' stats.damageDealt';
        break;
      case 'kdr':
        sort = { 'stats.kills': -1, 'stats.deaths': 1 };
        projection += ' stats.kills stats.deaths';
        break;
      case 'level':
        sort = { level: -1, totalXp: -1 };
        projection += ' level';
        break;
      default:
        sort = { 'stats.wins': -1 };
        projection += ' stats.wins stats.kills stats.matchesPlayed';
    }

    const players = await Player.find({ isBanned: false })
      .sort(sort)
      .skip(offset)
      .limit(limit)
      .select(projection)
      .lean();

    const total = await Player.countDocuments({ isBanned: false });

    // Format results with rank
    const results = players.map((player, index) => {
      const rank = offset + index + 1;
      const entry = {
        rank,
        playerId: player.playerId,
        username: player.username,
        displayName: player.displayName,
        level: player.level,
        title: player.title
      };

      switch (category) {
        case 'wins':
          entry.value = player.stats?.wins || 0;
          entry.subValue = player.stats?.kills || 0;
          entry.matchesPlayed = player.stats?.matchesPlayed || 0;
          break;
        case 'kills':
          entry.value = player.stats?.kills || 0;
          entry.deaths = player.stats?.deaths || 0;
          entry.kdr = player.stats?.deaths > 0
            ? (player.stats.kills / player.stats.deaths).toFixed(2)
            : player.stats?.kills || 0;
          break;
        case 'level':
          entry.value = player.level || 1;
          break;
        default:
          entry.value = player.stats?.[category] || 0;
      }

      return entry;
    });

    const response = {
      category,
      total,
      limit,
      offset,
      results,
      cached: false,
      generatedAt: new Date().toISOString()
    };

    // Cache the result
    if (redisClient?.status === 'ready') {
      const cacheKey = `${LEADERBOARD_CACHE_KEY}:${category}:${limit}:${offset}`;
      redisClient.setex(cacheKey, CACHE_TTL, JSON.stringify(response));
    }

    res.json(response);
  } catch (err) {
    console.error('Leaderboard error:', err);
    res.status(500).json({ error: 'Failed to get leaderboard', code: 'SERVER_ERROR' });
  }
});

// ─── GET /rank/:playerId ────────────────────────────────────────
router.get('/rank/:playerId', optionalAuth, async (req, res) => {
  try {
    const player = await Player.findOne({ playerId: req.params.playerId });
    if (!player) {
      return res.status(404).json({ error: 'Player not found', code: 'NOT_FOUND' });
    }

    // Calculate rank across categories
    const categories = ['wins', 'kills', 'damage', 'level'];
    const ranks = {};

    for (const category of categories) {
      let sort = {};
      switch (category) {
        case 'wins': sort = { 'stats.wins': -1 }; break;
        case 'kills': sort = { 'stats.kills': -1 }; break;
        case 'damage': sort = { 'stats.damageDealt': -1 }; break;
        case 'level': sort = { level: -1 }; break;
      }

      const count = await Player.countDocuments({
        isBanned: false,
        ...(category !== 'level'
          ? { [`stats.${category}`]: { $gt: player.stats?.[category] || 0 } }
          : { level: { $gt: player.level } })
      });

      ranks[category] = count + 1;
    }

    res.json({
      playerId: player.playerId,
      username: player.username,
      ranks,
      stats: {
        wins: player.stats?.wins || 0,
        kills: player.stats?.kills || 0,
        damage: player.stats?.damageDealt || 0,
        level: player.level
      }
    });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get rank', code: 'SERVER_ERROR' });
  }
});

router.setRedis = setRedis;
module.exports = router;
