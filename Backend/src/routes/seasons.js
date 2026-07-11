const express = require('express');
const { authenticate, adminOnly } = require('../middleware/auth');

const router = express.Router();

// ─── Season System ──────────────────────────────────────────────
// In production: store in MongoDB

let seasons = [];
let currentSeasonId = null;

// ─── Season Template ────────────────────────────────────────────
const SEASON_CONFIGS = [
  {
    id: 'season_1',
    name: 'Neon Dawn',
    theme: 'Cyberpunk Neon',
    durationDays: 60,
    battlePassTiers: 100,
    xpPerTier: 1000,
    premiumCost: 950,
    startDate: new Date('2024-06-01'),
    endDate: new Date('2024-07-31'),
    weapons: [
      { id: 'a51_eclipse', name: 'A-51 Eclipse AR', tier: 1, isFree: true },
      { id: 's15_viper_mk2', name: 'S-15 Viper MK2 SMG', tier: 30, isFree: false }
    ],
    rewards: [
      { tier: 1, type: 'character', id: 'neon_vanguard', name: 'Neon Vanguard Skin', premium: false },
      { tier: 10, type: 'emote', id: 'emote_neon_dance', name: 'Neon Dance', premium: false },
      { tier: 25, type: 'weapon_skin', id: 'skin_striker_neon', name: 'Neon A-17 Striker', premium: true },
      { tier: 50, type: 'character', id: 'neon_phantom', name: 'Neon Phantom Operative', premium: false },
      { tier: 75, type: 'weapon_skin', id: 'skin_longshot_neon', name: 'Neon SR-25 Longshot', premium: true },
      { tier: 100, type: 'character', id: 'neon_reaper', name: 'Neon Reaper (Legendary)', premium: false }
    ]
  },
  {
    id: 'season_2',
    name: 'Frost Protocol',
    theme: 'Arctic Military',
    durationDays: 60,
    battlePassTiers: 100,
    xpPerTier: 1000,
    premiumCost: 950,
    startDate: new Date('2024-08-01'),
    endDate: new Date('2024-09-30'),
    weapons: [
      { id: 'sr50_frost', name: 'SR-50 Frost Sniper', tier: 1, isFree: true }
    ],
    rewards: [
      { tier: 1, type: 'character', id: 'frost_operative', name: 'Frost Operative', premium: false },
      { tier: 100, type: 'character', id: 'frost_commander', name: 'Frost Commander (Legendary)', premium: false }
    ]
  }
];

// ─── Initialize ─────────────────────────────────────────────────
function initSeasons() {
  if (seasons.length > 0) return;
  seasons = SEASON_CONFIGS;

  // Find current season
  const now = new Date();
  const current = seasons.find(s => now >= s.startDate && now <= s.endDate);
  if (current) currentSeasonId = current.id;
  
  console.log(`📅 Seasons loaded: ${seasons.length} (current: ${currentSeasonId || 'none'})`);
}

initSeasons();

// ─── GET /seasons ───────────────────────────────────────────────
router.get('/', (req, res) => {
  res.json({
    seasons: seasons.map(s => ({
      id: s.id,
      name: s.name,
      theme: s.theme,
      startDate: s.startDate,
      endDate: s.endDate,
      battlePassTiers: s.battlePassTiers,
      premiumCost: s.premiumCost,
      weapons: s.weapons,
      isCurrent: s.id === currentSeasonId
    }))
  });
});

// ─── GET /seasons/current ───────────────────────────────────────
router.get('/current', (req, res) => {
  const current = seasons.find(s => s.id === currentSeasonId);
  if (!current) return res.status(404).json({ error: 'No active season', code: 'NO_SEASON' });
  res.json(current);
});

// ─── GET /seasons/:id ───────────────────────────────────────────
router.get('/:id', (req, res) => {
  const season = seasons.find(s => s.id === req.params.id);
  if (!season) return res.status(404).json({ error: 'Season not found', code: 'NOT_FOUND' });
  res.json(season);
});

// ─── POST /seasons/claim-tier ───────────────────────────────────
router.post('/claim-tier', authenticate, async (req, res) => {
  const { seasonId, tier } = req.body;
  if (!seasonId || !tier) return res.status(400).json({ error: 'Season ID and tier required', code: 'MISSING_FIELDS' });

  const season = seasons.find(s => s.id === seasonId);
  if (!season) return res.status(404).json({ error: 'Season not found', code: 'NOT_FOUND' });

  const reward = season.rewards.find(r => r.tier === tier);
  if (!reward) return res.status(404).json({ error: 'No reward at this tier', code: 'NO_REWARD' });

  // In production: check if player has unlocked the tier and hasn't claimed it yet
  // Then grant the reward item to the player's inventory

  console.log(`🎁 ${req.player.username} claimed tier ${tier} reward: ${reward.name}`);
  res.json({ message: `Claimed ${reward.name}`, reward });
});

// ─── POST /seasons/calculate ────────────────────────────────────
router.post('/calculate', authenticate, async (req, res) => {
  // Calculate season XP from match performance
  const { placement, kills, damage, survivalTime } = req.body;
  
  let seasonXp = 0;
  seasonXp += placement === 1 ? 500 : Math.max(0, 200 - placement * 5);
  seasonXp += kills * 50;
  seasonXp += Math.floor(damage / 10);
  seasonXp += Math.floor((survivalTime || 0) / 60) * 10;

  res.json({
    seasonXpEarned: seasonXp,
    currentSeason: currentSeasonId
  });
});

module.exports = router;
