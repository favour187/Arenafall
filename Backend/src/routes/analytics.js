const express = require('express');
const { authenticate } = require('../middleware/auth');

const router = express.Router();

// ─── In-memory analytics store ─────────────────────────────────
const analyticsEvents = [];
const ANALYTICS_FLUSH_INTERVAL = parseInt(process.env.ANALYTICS_FLUSH_INTERVAL) || 30000;
let lastFlush = Date.now();

// ─── POST /track ───────────────────────────────────────────────
router.post('/track', authenticate, (req, res) => {
  try {
    const { event, properties } = req.body;
    
    if (!event) {
      return res.status(400).json({ error: 'Event name required', code: 'MISSING_EVENT' });
    }

    analyticsEvents.push({
      event,
      playerId: req.player.playerId,
      properties: properties || {},
      timestamp: new Date().toISOString(),
      platform: req.headers['x-platform'] || 'unknown',
      version: req.headers['x-game-version'] || '1.0.0'
    });

    // Auto-flush if batch size reached
    if (analyticsEvents.length >= (parseInt(process.env.ANALYTICS_BATCH_SIZE) || 100)) {
      flushAnalytics();
    }

    res.json({ status: 'tracked' });
  } catch (err) {
    // Don't fail the game for analytics
    res.json({ status: 'tracked' });
  }
});

// ─── POST /track/batch ──────────────────────────────────────────
router.post('/track/batch', authenticate, (req, res) => {
  try {
    const { events } = req.body;
    if (!Array.isArray(events)) {
      return res.status(400).json({ error: 'Events must be an array', code: 'INVALID_FORMAT' });
    }

    for (const evt of events) {
      analyticsEvents.push({
        event: evt.event,
        playerId: req.player.playerId,
        properties: evt.properties || {},
        timestamp: new Date().toISOString(),
        platform: req.headers['x-platform'] || 'unknown'
      });
    }

    if (analyticsEvents.length >= (parseInt(process.env.ANALYTICS_BATCH_SIZE) || 100)) {
      flushAnalytics();
    }

    res.json({ status: 'tracked', count: events.length });
  } catch (err) {
    res.json({ status: 'tracked' });
  }
});

// ─── POST /performance ─────────────────────────────────────────
router.post('/performance', authenticate, (req, res) => {
  try {
    const { fps, frameTime, ping, platform } = req.body;
    
    analyticsEvents.push({
      event: 'performance',
      playerId: req.player.playerId,
      properties: { fps, frameTime, ping, platform },
      timestamp: new Date().toISOString()
    });

    res.json({ status: 'tracked' });
  } catch (err) {
    res.json({ status: 'tracked' });
  }
});

// ─── GET /stats ─────────────────────────────────────────────────
router.get('/stats', (req, res) => {
  // Aggregate basic stats
  const eventCounts = {};
  for (const evt of analyticsEvents) {
    eventCounts[evt.event] = (eventCounts[evt.event] || 0) + 1;
  }

  res.json({
    totalEvents: analyticsEvents.length,
    pendingFlush: analyticsEvents.length,
    lastFlush: new Date(lastFlush).toISOString(),
    events: eventCounts
  });
});

// ─── Flush to storage ───────────────────────────────────────────
function flushAnalytics() {
  if (analyticsEvents.length === 0) return;

  const batch = analyticsEvents.splice(0, analyticsEvents.length);
  lastFlush = Date.now();

  // In production: write to MongoDB / file / external service
  if (batch.length > 10) {
    console.log(`📊 Analytics flushed: ${batch.length} events`);
  }
}

// Periodic flush
setInterval(flushAnalytics, ANALYTICS_FLUSH_INTERVAL);

module.exports = router;
