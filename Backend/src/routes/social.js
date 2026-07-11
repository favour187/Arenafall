const express = require('express');
const { authenticate } = require('../middleware/auth');
const Player = require('../models/Player');
const { logger } = require('../server');

const router = express.Router();

// ─── Friend System ──────────────────────────────────────────────
// Stored in player document as a simple array for now (scales with Redis later)

// ─── POST /friends/add ──────────────────────────────────────────
router.post('/friends/add', authenticate, async (req, res) => {
  try {
    const { friendId } = req.body;
    if (!friendId || friendId === req.player.playerId) {
      return res.status(400).json({ error: 'Invalid friend ID', code: 'INVALID_ID' });
    }

    const friend = await Player.findOne({ playerId: friendId });
    if (!friend) {
      return res.status(404).json({ error: 'Player not found', code: 'NOT_FOUND' });
    }

    const player = await Player.findOne({ playerId: req.player.playerId });
    
    // Simple: store as comma-separated or array (in production use a friends collection)
    if (!player.friends) player.friends = [];
    if (!player.friends.includes(friendId)) {
      player.friends.push(friendId);
      await player.save();
    }

    res.json({ message: `Added ${friend.username} as friend` });
  } catch (err) {
    res.status(500).json({ error: 'Failed to add friend', code: 'SERVER_ERROR' });
  }
});

// ─── POST /friends/remove ───────────────────────────────────────
router.post('/friends/remove', authenticate, async (req, res) => {
  try {
    const { friendId } = req.body;
    const player = await Player.findOne({ playerId: req.player.playerId });
    if (player.friends) {
      player.friends = player.friends.filter(f => f !== friendId);
      await player.save();
    }
    res.json({ message: 'Friend removed' });
  } catch (err) {
    res.status(500).json({ error: 'Failed to remove friend', code: 'SERVER_ERROR' });
  }
});

// ─── GET /friends ───────────────────────────────────────────────
router.get('/friends', authenticate, async (req, res) => {
  try {
    const player = await Player.findOne({ playerId: req.player.playerId })
      .populate('friends', 'username displayName level title isOnline');

    const friends = (player.friends || []).map(f => ({
      playerId: f.playerId || f,
      username: f.username || f,
      displayName: f.displayName || f,
      level: f.level || 0,
      isOnline: false // Would come from Redis
    }));

    res.json({ friends });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get friends', code: 'SERVER_ERROR' });
  }
});

// ─── Party System ───────────────────────────────────────────────
const parties = new Map(); // In production: Redis

router.post('/party/create', authenticate, (req, res) => {
  const partyId = `party_${Date.now()}_${Math.random().toString(36).substr(2, 5)}`;
  parties.set(partyId, {
    id: partyId,
    leader: req.player.playerId,
    members: [{ playerId: req.player.playerId, username: req.player.username }],
    maxSize: req.body.mode === 'duos' ? 2 : 4,
    createdAt: new Date()
  });
  res.json({ party: parties.get(partyId) });
});

router.post('/party/join', authenticate, (req, res) => {
  const party = parties.get(req.body.partyId);
  if (!party) return res.status(404).json({ error: 'Party not found' });
  if (party.members.length >= party.maxSize) {
    return res.status(400).json({ error: 'Party full' });
  }
  party.members.push({ playerId: req.player.playerId, username: req.player.username });
  res.json({ party });
});

router.post('/party/leave', authenticate, (req, res) => {
  for (const [id, party] of parties) {
    party.members = party.members.filter(m => m.playerId !== req.player.playerId);
    if (party.members.length === 0) parties.delete(id);
  }
  res.json({ message: 'Left party' });
});

// ─── POST /report ───────────────────────────────────────────────
router.post('/report', authenticate, async (req, res) => {
  try {
    const { playerId, reason, details } = req.body;
    logger.warn(`🚨 Player reported: ${playerId} by ${req.player.playerId} - ${reason}`);
    
    // In production: save to reports collection
    res.json({ message: 'Report submitted', caseId: `case_${Date.now()}` });
  } catch (err) {
    res.status(500).json({ error: 'Failed to submit report', code: 'SERVER_ERROR' });
  }
});

module.exports = router;
