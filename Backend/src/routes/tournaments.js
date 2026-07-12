const express = require('express');
const { authenticate } = require('../middleware/auth');

const router = express.Router();

// ─── In-memory tournament store ────────────────────────────────
let tournaments = global.memoryStore?.tournaments || [];
let autoId = 1;

// ─── GET /tournaments ───────────────────────────────────────────
router.get('/', (req, res) => {
  const { status, mode } = req.query;
  let filtered = tournaments;

  if (status) filtered = filtered.filter(t => t.status === status);
  if (mode) filtered = filtered.filter(t => t.mode === mode);

  // Auto-update statuses
  const now = new Date();
  filtered.forEach(t => {
    if (new Date(t.startDate) < now && t.status === 'upcoming') t.status = 'live';
  });

  const active = filtered.filter(t => t.status === 'live');
  const upcoming = filtered.filter(t => t.status === 'upcoming');
  const completed = filtered.filter(t => t.status === 'completed');

  res.json({
    active,
    upcoming,
    completed,
    total: filtered.length
  });
});

// ─── GET /tournaments/:id ──────────────────────────────────────
router.get('/:id', (req, res) => {
  const t = tournaments.find(t => t.id === req.params.id);
  if (!t) return res.status(404).json({ error: 'Tournament not found', code: 'NOT_FOUND' });
  res.json(t);
});

// ─── POST /tournaments/register ─────────────────────────────────
router.post('/register', authenticate, async (req, res) => {
  const { tournamentId } = req.body;
  if (!tournamentId) return res.status(400).json({ error: 'Tournament ID required', code: 'MISSING_FIELDS' });

  const t = tournaments.find(t => t.id === tournamentId);
  if (!t) return res.status(404).json({ error: 'Tournament not found', code: 'NOT_FOUND' });
  if (t.status !== 'upcoming') return res.status(400).json({ error: 'Tournament not open for registration', code: 'NOT_OPEN' });
  if (t.registered.includes(req.player.playerId)) return res.status(409).json({ error: 'Already registered', code: 'ALREADY_REGISTERED' });
  if (t.registered.length >= t.maxPlayers) return res.status(400).json({ error: 'Tournament full', code: 'FULL' });

  t.registered.push(req.player.playerId);
  t.players = t.registered.length;

  console.log(`🏆 ${req.player.username} registered for ${t.name}`);
  res.json({ message: `Registered for ${t.name}`, tournament: t });
});

// ─── POST /tournaments/:id/start ────────────────────────────────
router.post('/:id/start', authenticate, async (req, res) => {
  const t = tournaments.find(t => t.id === req.params.id);
  if (!t) return res.status(404).json({ error: 'Not found', code: 'NOT_FOUND' });

  t.status = 'live';
  t.bracket = generateBracket(t.registered, t.mode);
  
  console.log(`🏆 Tournament started: ${t.name} (${t.registered.length} players)`);
  res.json({ message: 'Tournament started', tournament: t });
});

// ─── POST /tournaments/:id/complete ─────────────────────────────
router.post('/:id/complete', authenticate, async (req, res) => {
  const t = tournaments.find(t => t.id === req.params.id);
  if (!t) return res.status(404).json({ error: 'Not found', code: 'NOT_FOUND' });

  const { winner } = req.body;
  t.status = 'completed';
  t.winner = winner || 'Unknown';

  console.log(`🏆 Tournament completed: ${t.name} — Winner: ${t.winner}`);
  res.json({ message: 'Tournament completed', winner: t.winner });
});

// ─── Helper: Generate bracket ───────────────────────────────────
function generateBracket(players, mode) {
  if (!players || players.length === 0) return null;

  const teamSize = mode === 'duos' ? 2 : mode === 'squads' ? 4 : 1;
  const shuffled = [...players].sort(() => Math.random() - 0.5);
  const teams = [];
  
  for (let i = 0; i < shuffled.length; i += teamSize) {
    teams.push(shuffled.slice(i, i + teamSize));
  }

  // Generate bracket rounds
  const rounds = [];
  let roundTeams = teams;
  let roundNum = 1;

  while (roundTeams.length > 1) {
    const matches = [];
    for (let i = 0; i < roundTeams.length; i += 2) {
      if (i + 1 < roundTeams.length) {
        matches.push({
          id: `round${roundNum}_match${Math.floor(i / 2) + 1}`,
          team1: roundTeams[i],
          team2: roundTeams[i + 1],
          winner: null,
          score: null
        });
      } else {
        // Bye — auto-advance
        matches.push({
          id: `round${roundNum}_match${Math.floor(i / 2) + 1}`,
          team1: roundTeams[i],
          team2: null,
          winner: roundTeams[i],
          score: 'BYE'
        });
      }
    }
    rounds.push({ round: roundNum, matches });
    
    // Next round would be winners — simulated
    roundTeams = roundTeams.slice(0, Math.ceil(roundTeams.length / 2));
    roundNum++;
  }

  return { teams, rounds, format: mode };
}

module.exports = router;
