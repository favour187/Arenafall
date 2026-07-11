const express = require('express');
const { authenticate } = require('../middleware/auth');
const Player = require('../models/Player');
const { logger } = require('../server');

const router = express.Router();

// ─── Shop Catalog (static for now, in production from DB) ──────
const SHOP_ITEMS = [
  // Characters
  { id: 'phantom', name: 'Phantom Operative', type: 'character', price: 500, currency: 'credits', rarity: 'rare' },
  { id: 'reaper', name: 'Reaper Unit', type: 'character', price: 800, currency: 'credits', rarity: 'epic' },
  { id: 'sentinel', name: 'Sentinel Guard', type: 'character', price: 1200, currency: 'credits', rarity: 'legendary' },
  // Weapon Skins
  { id: 'skin_striker_carbon', name: 'Carbon Fiber A-17', type: 'weapon_skin', price: 300, currency: 'credits', rarity: 'rare' },
  { id: 'skin_phantom_gold', name: 'Gold A-23', type: 'weapon_skin', price: 600, currency: 'credits', rarity: 'epic' },
  { id: 'skin_viper_neon', name: 'Neon S-9', type: 'weapon_skin', price: 450, currency: 'credits', rarity: 'epic' },
  // Emotes
  { id: 'emote_victory', name: 'Victory Dance', type: 'emote', price: 200, currency: 'credits', rarity: 'uncommon' },
  { id: 'emote_floss', name: 'Floss', type: 'emote', price: 350, currency: 'credits', rarity: 'rare' },
  { id: 'emote_airguitar', name: 'Air Guitar', type: 'emote', price: 500, currency: 'credits', rarity: 'epic' },
  // Premium
  { id: 'battlepass_premium', name: 'Battle Pass Premium', type: 'battlepass', price: 950, currency: 'premium', rarity: 'legendary' },
  { id: 'credits_pack_small', name: '500 Credits', type: 'currency', price: 99, currency: 'premium', rarity: 'common' },
  { id: 'credits_pack_large', name: '2000 Credits', type: 'currency', price: 349, currency: 'premium', rarity: 'uncommon' },
];

// ─── GET /catalog ──────────────────────────────────────────────
router.get('/catalog', (req, res) => {
  const type = req.query.type;
  const items = type ? SHOP_ITEMS.filter(i => i.type === type) : SHOP_ITEMS;
  res.json({ items, total: items.length });
});

// ─── POST /purchase ────────────────────────────────────────────
router.post('/purchase', authenticate, async (req, res) => {
  try {
    const { itemId } = req.body;
    if (!itemId) {
      return res.status(400).json({ error: 'Item ID required', code: 'MISSING_FIELDS' });
    }

    const item = SHOP_ITEMS.find(i => i.id === itemId);
    if (!item) {
      return res.status(404).json({ error: 'Item not found', code: 'NOT_FOUND' });
    }

    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) {
      return res.status(404).json({ error: 'Player not found', code: 'NOT_FOUND' });
    }

    // Check if already owned
    const ownedField = item.type === 'character' ? 'ownedCharacters'
      : item.type === 'weapon_skin' ? 'ownedWeaponSkins'
      : item.type === 'emote' ? 'ownedEmotes'
      : null;

    if (ownedField && player[ownedField]?.includes(item.id)) {
      return res.status(409).json({ error: 'Already owned', code: 'ALREADY_OWNED' });
    }

    // Check currency
    if (item.currency === 'credits') {
      if (player.credits < item.price) {
        return res.status(402).json({ error: 'Insufficient credits', code: 'INSUFFICIENT_FUNDS' });
      }
      player.credits -= item.price;
    } else if (item.currency === 'premium') {
      if (player.premiumCurrency < item.price) {
        return res.status(402).json({ error: 'Insufficient premium currency', code: 'INSUFFICIENT_FUNDS' });
      }
      player.premiumCurrency -= item.price;
    }

    // Grant item
    if (ownedField) {
      if (!player[ownedField]) player[ownedField] = [];
      player[ownedField].push(item.id);
    }

    // Handle special items
    if (item.type === 'battlepass') {
      player.battlePass.premium = true;
    } else if (item.id === 'credits_pack_small') {
      player.credits += 500;
    } else if (item.id === 'credits_pack_large') {
      player.credits += 2000;
    }

    await player.save();
    logger.info(`🛒 Purchase: ${req.player.username} bought ${item.name} for ${item.price} ${item.currency}`);

    res.json({
      message: 'Purchase successful',
      item: item.name,
      credits: player.credits,
      premiumCurrency: player.premiumCurrency
    });
  } catch (err) {
    logger.error('Purchase error:', err);
    res.status(500).json({ error: 'Purchase failed', code: 'SERVER_ERROR' });
  }
});

// ─── Daily Login Reward ─────────────────────────────────────────
router.post('/daily-reward', authenticate, async (req, res) => {
  try {
    const player = await Player.findOne({ playerId: req.player.playerId });
    if (!player) return res.status(404).json({ error: 'Not found', code: 'NOT_FOUND' });

    // Check if already claimed today (in production: use timestamp tracking)
    const now = new Date();
    const lastClaim = player.lastLogin || new Date(0);
    const dayDiff = Math.floor((now - lastClaim) / (1000 * 60 * 60 * 24));

    if (dayDiff < 1) {
      return res.status(409).json({ error: 'Already claimed today', code: 'ALREADY_CLAIMED' });
    }

    const rewards = [
      { day: 1, credits: 100, xp: 200 },
      { day: 2, credits: 150, xp: 300 },
      { day: 3, credits: 200, xp: 400, item: 'emote_victory' },
      { day: 4, credits: 250, xp: 500 },
      { day: 5, credits: 300, xp: 600 },
      { day: 6, credits: 400, xp: 800 },
      { day: 7, credits: 500, xp: 1000, item: 'skin_striker_carbon' },
    ];

    const streak = Math.min(dayDiff, 7);
    const reward = rewards[streak - 1] || rewards[0];

    player.credits += reward.credits;
    player.addXp(reward.xp);
    if (reward.item) {
      if (!player.ownedSkins) player.ownedSkins = [];
      player.ownedSkins.push(reward.item);
    }
    player.lastLogin = now;
    await player.save();

    res.json({
      message: 'Daily reward claimed',
      day: streak,
      credits: reward.credits,
      xp: reward.xp,
      item: reward.item || null,
      streak
    });
  } catch (err) {
    res.status(500).json({ error: 'Failed to claim reward', code: 'SERVER_ERROR' });
  }
});

module.exports = router;
