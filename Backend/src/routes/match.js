const express = require('express');
const { authenticate } = require('../middleware/auth');
const Match = require('../models/Match');
const Player = require('../models/Player');
const { logger, config } = require('../server');

const router = express.Router();

// ─── POST /submit ───────────────────────────────────────────────
router.post('/submit', authenticate, async (req, res) => {
  try {
    const matchData = req.body;

    // Validate match data
    if (!matchData.matchId) {
      return res.status(400).json({ error: 'Match ID required', code: 'MISSING_FIELDS' });
    }

    // Find the player and update stats
    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) {
      return res.status(404).json({ error: 'Player not found', code: 'NOT_FOUND' });
    }

    // Find or create match record
    let match = await Match.findOne({ matchId: matchData.matchId });
    const isNew = !match;

    if (isNew) {
      match = new Match({
        matchId: matchData.matchId,
        mode: matchData.mode || 'solo',
        map: matchData.map || 'arena7',
        maxPlayers: matchData.maxPlayers || config.maxPlayersPerMatch
      });
    }

    // Update match info
    match.status = 'finished';
    match.endedAt = new Date();
    match.duration = matchData.duration || 0;

    // Add player result
    const playerResult = matchData.player;
    if (playerResult) {
      const existingPlayer = match.players.find(p => p.playerId === req.player.playerId);
      if (!existingPlayer) {
        match.players.push({
          playerId: req.player.playerId,
          username: req.player.username,
          ...playerResult
        });
      }
    }

    // Record kills
    if (matchData.killFeed) {
      for (const kill of matchData.killFeed) {
        match.recordKill(kill.killerId, kill.victimId, kill.weaponId, kill.headshot, kill.distance);
      }
    }

    await match.save();

    // Update player stats
    if (playerResult) {
      player.addStats(playerResult);

      // Calculate XP and credits
      const xpReward = calculateXpReward(playerResult);
      const creditReward = calculateCreditReward(playerResult);
      player.addXp(xpReward);
      player.credits += creditReward;

      await player.save();

      logger.info(`🏆 Match submitted: ${req.player.username} - P${playerResult.placement} ${playerResult.kills}k`);

      return res.json({
        message: 'Match recorded',
        matchId: match.matchId,
        xpEarned: xpReward,
        creditsEarned: creditReward,
        level: player.level,
        leveledUp: false, // Check from addXp
        stats: player.stats
      });
    }

    res.json({ message: 'Match recorded', matchId: match.matchId });
  } catch (err) {
    logger.error('Match submit error:', err);
    res.status(500).json({ error: 'Failed to submit match', code: 'SERVER_ERROR' });
  }
});

// ─── GET /history ───────────────────────────────────────────────
router.get('/history', authenticate, async (req, res) => {
  try {
    const page = parseInt(req.query.page) || 1;
    const limit = parseInt(req.query.limit) || 20;
    const skip = (page - 1) * limit;

    const matches = await Match.find({
      'players.playerId': req.player.playerId,
      status: 'finished'
    })
    .sort({ endedAt: -1 })
    .skip(skip)
    .limit(limit)
    .select('matchId mode map duration playerCount players killFeed endedAt');

    const total = await Match.countDocuments({
      'players.playerId': req.player.playerId,
      status: 'finished'
    });

    // Extract this player's data from each match
    const history = matches.map(match => {
      const myData = match.players.find(p => p.playerId === req.player.playerId);
      return {
        matchId: match.matchId,
        mode: match.mode,
        map: match.map,
        duration: match.duration,
        playerCount: match.playerCount,
        placement: myData?.placement,
        kills: myData?.kills,
        deaths: myData?.deaths,
        assists: myData?.assists,
        damageDealt: myData?.damageDealt,
        survivalTime: myData?.survivalTime,
        xpEarned: myData?.xpEarned,
        creditsEarned: myData?.creditsEarned,
        endedAt: match.endedAt
      };
    });

    res.json({
      matches: history,
      pagination: {
        page,
        limit,
        total,
        pages: Math.ceil(total / limit)
      }
    });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get match history', code: 'SERVER_ERROR' });
  }
});

// ─── GET /recent/:count ─────────────────────────────────────────
router.get('/recent/:count?', async (req, res) => {
  try {
    if (require('mongoose').connection.readyState !== 1) {
      return res.json({ matches: global.memoryStore?.matches || [] });
    }
    const count = Math.min(parseInt(req.params.count) || 10, 50);
    const matches = await Match.find({ status: 'finished' })
      .sort({ endedAt: -1 })
      .limit(count)
      .select('matchId mode map duration playerCount endedAt');

    res.json({ matches });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get recent matches', code: 'SERVER_ERROR' });
  }
});

// ─── GET /:matchId ──────────────────────────────────────────────
router.get('/:matchId', async (req, res) => {
  try {
    const match = await Match.findOne({ matchId: req.params.matchId });
    if (!match) {
      return res.status(404).json({ error: 'Match not found', code: 'NOT_FOUND' });
    }
    res.json({ match });
  } catch (err) {
    res.status(500).json({ error: 'Failed to get match', code: 'SERVER_ERROR' });
  }
});

// ─── Helper Functions ───────────────────────────────────────────
function calculateXpReward(playerResult) {
  let xp = 50; // Base XP
  if (playerResult.placement === 1) xp += 500;
  else if (playerResult.placement <= 5) xp += 300;
  else if (playerResult.placement <= 10) xp += 200;
  else if (playerResult.placement <= 25) xp += 100;
  xp += (playerResult.kills || 0) * 100;
  xp += Math.floor((playerResult.survivalTime || 0) / 60) * 10;
  xp += (playerResult.revives || 0) * 50;
  return Math.min(xp, 5000);
}

function calculateCreditReward(playerResult) {
  let credits = 10; // Base
  if (playerResult.placement === 1) credits += 200;
  else if (playerResult.placement <= 5) credits += 100;
  else if (playerResult.placement <= 10) credits += 50;
  credits += (playerResult.kills || 0) * 25;
  return Math.min(credits, 2000);
}

module.exports = router;
