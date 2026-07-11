const express = require('express');
const { authenticate } = require('../middleware/auth');

const router = express.Router();

// ─── Replay System ──────────────────────────────────────────────
// Stores match action data for playback

let replays = [];
const MAX_REPLAYS = 1000;

// ─── POST /replays/record ───────────────────────────────────────
router.post('/record', authenticate, async (req, res) => {
  try {
    const { matchId, actions, metadata } = req.body;
    
    if (!matchId || !actions) {
      return res.status(400).json({ error: 'Match ID and actions required', code: 'MISSING_FIELDS' });
    }

    // Validate actions array
    if (!Array.isArray(actions) || actions.length > 50000) {
      return res.status(400).json({ error: 'Invalid actions data', code: 'INVALID_ACTIONS' });
    }

    const replay = {
      id: `replay_${matchId}`,
      matchId,
      recordPlayerId: req.player.playerId,
      actions: actions.slice(0, 50000), // Cap at 50k actions
      metadata: {
        mode: metadata?.mode || 'solo',
        map: metadata?.map || 'arena7',
        duration: metadata?.duration || 0,
        playerCount: metadata?.playerCount || 0,
        recordedAt: new Date().toISOString(),
        gameVersion: metadata?.gameVersion || '1.0',
        tickRate: metadata?.tickRate || 20
      },
      compressed: false,
      size: JSON.stringify(actions).length,
      createdAt: new Date(),
      downloads: 0
    };

    // Manage storage limit
    if (replays.length >= MAX_REPLAYS) {
      // Remove oldest replay
      replays.sort((a, b) => a.createdAt - b.createdAt);
      replays.shift();
    }

    replays.push(replay);

    console.log(`🎥 Replay recorded: ${replay.id} (${actions.length} actions, ${(replay.size / 1024).toFixed(1)}KB)`);
    res.json({
      message: 'Replay recorded',
      replayId: replay.id,
      actionCount: actions.length,
      sizeKB: (replay.size / 1024).toFixed(1)
    });
  } catch (err) {
    console.error('Replay record error:', err);
    res.status(500).json({ error: 'Failed to record replay', code: 'SERVER_ERROR' });
  }
});

// ─── GET /replays/:matchId ──────────────────────────────────────
router.get('/:matchId', (req, res) => {
  const replay = replays.find(r => r.matchId === req.params.matchId || r.id === `replay_${req.params.matchId}`);
  if (!replay) return res.status(404).json({ error: 'Replay not found', code: 'NOT_FOUND' });

  replay.downloads++;
  res.json({
    id: replay.id,
    matchId: replay.matchId,
    metadata: replay.metadata,
    actions: replay.actions,
    actionCount: replay.actions.length,
    sizeKB: (replay.size / 1024).toFixed(1)
  });
});

// ─── GET /replays ───────────────────────────────────────────────
router.get('/', (req, res) => {
  const page = parseInt(req.query.page) || 1;
  const limit = Math.min(parseInt(req.query.limit) || 20, 100);
  const start = (page - 1) * limit;

  const sorted = [...replays].sort((a, b) => b.createdAt - a.createdAt);
  const pageReplays = sorted.slice(start, start + limit);

  res.json({
    replays: pageReplays.map(r => ({
      id: r.id,
      matchId: r.matchId,
      metadata: r.metadata,
      actionCount: r.actions.length,
      sizeKB: (r.size / 1024).toFixed(1),
      recordedAt: r.createdAt,
      downloads: r.downloads
    })),
    pagination: {
      page,
      limit,
      total: replays.length,
      totalSizeKB: (replays.reduce((a, r) => a + r.size, 0) / 1024).toFixed(1)
    }
  });
});

// ─── GET /replays/:matchId/data ─────────────────────────────────
router.get('/:matchId/data', (req, res) => {
  const replay = replays.find(r => r.matchId === req.params.matchId);
  if (!replay) return res.status(404).json({ error: 'Replay not found', code: 'NOT_FOUND' });

  // Return replay data for Unity to playback
  res.json({
    formatVersion: '1.0',
    tickRate: replay.metadata.tickRate,
    duration: replay.metadata.duration,
    actions: replay.actions,
    metadata: replay.metadata,
    // Include player lookup table
    playerLookup: extractPlayers(replay.actions)
  });
});

// ─── Helper ─────────────────────────────────────────────────────
function extractPlayers(actions) {
  const players = new Map();
  for (const action of actions) {
    if (action.playerId && !players.has(action.playerId)) {
      players.set(action.playerId, {
        playerId: action.playerId,
        playerName: action.playerName || `Player ${players.size + 1}`
      });
    }
  }
  return Array.from(players.values());
}

module.exports = router;
