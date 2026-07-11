// ═══════════════════════════════════════════════════
//  ARENA FALL — DISCORD BOT
//  Full stats, leaderboards, tournaments, moderation
// ═══════════════════════════════════════════════════

const { Client, GatewayIntentBits, SlashCommandBuilder, EmbedBuilder, ActionRowBuilder, ButtonBuilder, ButtonStyle, Collection } = require('discord.js');
const axios = require('axios');
const cron = require('node-cron');

const client = new Client({
  intents: [
    GatewayIntentBits.Guilds,
    GatewayIntentBits.GuildMessages,
    GatewayIntentBits.MessageContent,
    GatewayIntentBits.GuildMembers
  ]
});

// ─── Config ────────────────────────────────────────────────────
const CONFIG = {
  apiUrl: process.env.API_URL || 'http://localhost:3000/api/v1',
  guildId: process.env.DISCORD_GUILD_ID,
  adminRoleId: process.env.ADMIN_ROLE_ID,
  logChannelId: process.env.LOG_CHANNEL_ID,
  reportChannelId: process.env.REPORT_CHANNEL_ID,
  prefix: '!'
};

// ─── API Helper ─────────────────────────────────────────────────
const api = {
  async get(endpoint) {
    try {
      const { data } = await axios.get(`${CONFIG.apiUrl}${endpoint}`);
      return data;
    } catch (err) {
      console.error(`API Error [${endpoint}]:`, err?.response?.data || err.message);
      return null;
    }
  },
  async post(endpoint, body, token) {
    try {
      const headers = token ? { Authorization: `Bearer ${token}` } : {};
      const { data } = await axios.post(`${CONFIG.apiUrl}${endpoint}`, body, { headers });
      return data;
    } catch (err) {
      console.error(`API Error [${endpoint}]:`, err?.response?.data || err.message);
      return null;
    }
  }
};

// ─── Slash Command Registration ─────────────────────────────────
const COMMANDS = [
  // ── Player Commands ──
  new SlashCommandBuilder()
    .setName('profile')
    .setDescription('View a player profile')
    .addStringOption(opt => opt.setName('player').setDescription('Username or Player ID').setRequired(true)),

  new SlashCommandBuilder()
    .setName('stats')
    .setDescription('View player career stats')
    .addStringOption(opt => opt.setName('player').setDescription('Username').setRequired(false)),

  new SlashCommandBuilder()
    .setName('compare')
    .setDescription('Compare two players')
    .addStringOption(opt => opt.setName('player1').setDescription('First player').setRequired(true))
    .addStringOption(opt => opt.setName('player2').setDescription('Second player').setRequired(true)),

  // ── Leaderboard Commands ──
  new SlashCommandBuilder()
    .setName('leaderboard')
    .setDescription('View leaderboard rankings')
    .addStringOption(opt =>
      opt.setName('category').setDescription('Ranking category')
        .addChoices(
          { name: '🏆 Wins', value: 'wins' },
          { name: '💀 Kills', value: 'kills' },
          { name: '💥 Damage', value: 'damage' },
          { name: '📈 K/D Ratio', value: 'kdr' },
          { name: '⭐ Level', value: 'level' }
        )
        .setRequired(false)),

  new SlashCommandBuilder()
    .setName('rank')
    .setDescription("Check a player's rank across all categories")
    .addStringOption(opt => opt.setName('player').setDescription('Username').setRequired(false)),

  // ── Match Commands ──
  new SlashCommandBuilder()
    .setName('match')
    .setDescription('View match details')
    .addStringOption(opt => opt.setName('matchid').setDescription('Match ID').setRequired(true)),

  new SlashCommandBuilder()
    .setName('recent')
    .setDescription('Show recent matches')
    .addIntegerOption(opt => opt.setName('count').setDescription('Number of matches (1-20)').setMinValue(1).setMaxValue(20)),

  // ── Tournament Commands ──
  new SlashCommandBuilder()
    .setName('tournaments')
    .setDescription('List active tournaments'),

  new SlashCommandBuilder()
    .setName('tournament')
    .setDescription('View tournament details')
    .addStringOption(opt => opt.setName('id').setDescription('Tournament ID').setRequired(true)),

  // ── Social Commands ──
  new SlashCommandBuilder()
    .setName('search')
    .setDescription('Search for players')
    .addStringOption(opt => opt.setName('query').setDescription('Search term').setRequired(true)),

  // ── Admin Commands ──
  new SlashCommandBuilder()
    .setName('admin')
    .setDescription('Admin actions (admin only)')
    .addSubcommand(sub => sub.setName('ban').setDescription('Ban a player')
      .addStringOption(opt => opt.setName('player').setDescription('Username').setRequired(true))
      .addStringOption(opt => opt.setName('reason').setDescription('Ban reason').setRequired(true)))
    .addSubcommand(sub => sub.setName('unban').setDescription('Unban a player')
      .addStringOption(opt => opt.setName('player').setDescription('Username').setRequired(true)))
    .addSubcommand(sub => sub.setName('give').setDescription('Give currency to player')
      .addStringOption(opt => opt.setName('player').setDescription('Username').setRequired(true))
      .addIntegerOption(opt => opt.setName('amount').setDescription('Amount').setRequired(true))
      .addStringOption(opt => opt.setName('currency').setDescription('Credits or Premium').addChoices({ name: 'Credits', value: 'credits' }, { name: 'Premium', value: 'premium' })))
    .addSubcommand(sub => sub.setName('announce').setDescription('Send game announcement')
      .addStringOption(opt => opt.setName('message').setDescription('Announcement text').setRequired(true)))
];

// ─── Register Commands ──────────────────────────────────────────
client.once('ready', async () => {
  console.log(`✅ Arena Fall Bot logged in as ${client.user.tag}`);

  try {
    if (CONFIG.guildId) {
      const guild = client.guilds.cache.get(CONFIG.guildId);
      if (guild) {
        await guild.commands.set(COMMANDS);
        console.log(`✅ ${COMMANDS.length} slash commands registered in guild`);
      }
    } else {
      await client.application.commands.set(COMMANDS);
      console.log(`✅ ${COMMANDS.length} global slash commands registered`);
    }
  } catch (err) {
    console.error('Failed to register commands:', err);
  }

  // ── Scheduled Tasks ──
  // Daily leaderboard update
  cron.schedule('0 8 * * *', () => postLeaderboardUpdate());
  // Weekly tournament reminder
  cron.schedule('0 12 * * 6', () => postTournamentReminder());
  // Server status check every 30 min
  cron.schedule('*/30 * * * *', () => checkServerHealth());

  client.user.setActivity('Arena Fall • /profile', { type: 3 });
});

// ═══════════════════════════════════════════════════
//  COMMAND HANDLERS
// ═══════════════════════════════════════════════════

client.on('interactionCreate', async (interaction) => {
  if (!interaction.isChatInputCommand()) return;

  const { commandName, options } = interaction;

  try {
    switch (commandName) {
      case 'profile': return cmdProfile(interaction);
      case 'stats': return cmdStats(interaction);
      case 'compare': return cmdCompare(interaction);
      case 'leaderboard': return cmdLeaderboard(interaction);
      case 'rank': return cmdRank(interaction);
      case 'match': return cmdMatch(interaction);
      case 'recent': return cmdRecent(interaction);
      case 'tournaments': return cmdTournaments(interaction);
      case 'tournament': return cmdTournament(interaction);
      case 'search': return cmdSearch(interaction);
      case 'admin': return cmdAdmin(interaction);
    }
  } catch (err) {
    console.error(`Command error [${commandName}]:`, err);
    await interaction.reply({ content: '❌ An error occurred', ephemeral: true }).catch(() => {});
  }
});

// ─── /profile ───────────────────────────────────────────────────
async function cmdProfile(interaction) {
  await interaction.deferReply();
  const query = interaction.options.getString('player');

  // Search for player
  const search = await api.get(`/players/search?q=${encodeURIComponent(query)}`);
  if (!search?.results?.length) {
    return interaction.editReply(`❌ No player found matching "${query}"`);
  }

  const playerId = search.results[0].playerId;
  const profile = await api.get(`/players/profile/${playerId}`);
  if (!profile) return interaction.editReply('❌ Failed to load profile');

  const embed = new EmbedBuilder()
    .setColor(0xFF6B35)
    .setTitle(`🎮 ${profile.displayName || profile.username}`)
    .setDescription(`ID: \`${profile.playerId}\``)
    .setThumbnail(client.user.displayAvatarURL())
    .addFields(
      { name: '⭐ Level', value: `${profile.level}`, inline: true },
      { name: '🏆 Title', value: profile.title || 'Recruit', inline: true },
      { name: '🎯 Win Rate', value: `${profile.stats?.winRate || 0}%`, inline: true },
      { name: '💀 Kills', value: `${profile.stats?.kills || 0}`, inline: true },
      { name: '📊 K/D', value: `${profile.stats?.kdr || 0}`, inline: true },
      { name: '🏆 Wins', value: `${profile.stats?.wins || 0}`, inline: true },
      { name: '📅 Joined', value: profile.createdAt ? new Date(profile.createdAt).toLocaleDateString() : 'Unknown', inline: false }
    )
    .setFooter({ text: 'Arena Fall • /profile', iconURL: client.user.displayAvatarURL() })
    .setTimestamp();

  const row = new ActionRowBuilder()
    .addComponents(
      new ButtonBuilder()
        .setCustomId(`compare_${playerId}`)
        .setLabel('Compare')
        .setStyle(ButtonStyle.Primary)
        .setEmoji('⚔️'),
      new ButtonBuilder()
        .setURL(`https://arenafall.com/player/${profile.playerId}`)
        .setLabel('Web Profile')
        .setStyle(ButtonStyle.Link)
    );

  await interaction.editReply({ embeds: [embed], components: [row] });
}

// ─── /stats ─────────────────────────────────────────────────────
async function cmdStats(interaction) {
  await interaction.deferReply();
  const playerName = interaction.options.getString('player');

  if (!playerName) {
    // Show global stats from leaderboard
    const lb = await api.get('/leaderboard?category=wins&limit=5');
    if (!lb) return interaction.editReply('❌ Failed to load stats');

    const embed = new EmbedBuilder()
      .setColor(0x00D4FF)
      .setTitle('📊 Arena Fall — Global Stats')
      .addFields(
        { name: '👥 Total Players', value: `${lb.total || '?'}`, inline: true },
        { name: '🏆 Top Player', value: lb.results?.[0]?.username || 'N/A', inline: true },
        { name: '💀 Total Kills', value: lb.results?.reduce((a, b) => a + (b.value || 0), 0)?.toLocaleString() || '?', inline: false }
      )
      .setFooter({ text: 'Arena Fall Statistics' })
      .setTimestamp();

    return interaction.editReply({ embeds: [embed] });
  }

  const search = await api.get(`/players/search?q=${encodeURIComponent(playerName)}`);
  if (!search?.results?.length) return interaction.editReply(`❌ Player "${playerName}" not found`);

  const profile = await api.get(`/players/profile/${search.results[0].playerId}`);
  if (!profile) return interaction.editReply('❌ Failed to load stats');

  const s = profile.stats || {};
  const embed = new EmbedBuilder()
    .setColor(0xFF6B35)
    .setTitle(`📊 ${profile.displayName || profile.username} — Career Stats`)
    .addFields(
      { name: '📅 Matches', value: `${s.matchesPlayed || 0}`, inline: true },
      { name: '🏆 Wins', value: `${s.wins || 0}`, inline: true },
      { name: '🎯 Win Rate', value: `${s.winRate || 0}%`, inline: true },
      { name: '💀 Kills', value: `${s.kills || 0}`, inline: true },
      { name: '💀 Deaths', value: `${s.deaths || 0}`, inline: true },
      { name: '📊 K/D', value: `${s.kdr || 0}`, inline: true },
      { name: '🎯 Headshots', value: `${s.headshots || 0}`, inline: true },
      { name: '💥 Damage', value: `${(s.damageDealt || 0).toLocaleString()}`, inline: true },
      { name: '🚁 Revives', value: `${s.revives || 0}`, inline: true },
      { name: '⏱️ Play Time', value: `${Math.floor((s.totalPlayTime || 0) / 3600)}h`, inline: true },
      { name: '📏 Longest Kill', value: `${s.longestKill || 0}m`, inline: true },
      { name: '⭐ Level', value: `${profile.level || 1}`, inline: true }
    )
    .setFooter({ text: 'Arena Fall' });

  await interaction.editReply({ embeds: [embed] });
}

// ─── /compare ───────────────────────────────────────────────────
async function cmdCompare(interaction) {
  await interaction.deferReply();
  const p1 = await api.get(`/players/search?q=${encodeURIComponent(interaction.options.getString('player1'))}`);
  const p2 = await api.get(`/players/search?q=${encodeURIComponent(interaction.options.getString('player2'))}`);

  if (!p1?.results?.length || !p2?.results?.length) {
    return interaction.editReply('❌ One or both players not found');
  }

  const [pro1, pro2] = await Promise.all([
    api.get(`/players/profile/${p1.results[0].playerId}`),
    api.get(`/players/profile/${p2.results[0].playerId}`)
  ]);

  if (!pro1 || !pro2) return interaction.editReply('❌ Failed to load player data');

  const s1 = pro1.stats || {};
  const s2 = pro2.stats || {};

  const embed = new EmbedBuilder()
    .setColor(0x00D4FF)
    .setTitle('⚔️ Player Comparison')
    .addFields(
      { name: `**${pro1.displayName || pro1.username}**`, value: `⭐ Lvl ${pro1.level}`, inline: true },
      { name: 'VS', value: '⚔️', inline: true },
      { name: `**${pro2.displayName || pro2.username}**`, value: `⭐ Lvl ${pro2.level}`, inline: true },
      { name: '🏆 Wins', value: `${s1.wins || 0}`, inline: true },
      { name: '', value: '', inline: true },
      { name: '🏆 Wins', value: `${s2.wins || 0}`, inline: true },
      { name: '💀 Kills', value: `${s1.kills || 0}`, inline: true },
      { name: '', value: '', inline: true },
      { name: '💀 Kills', value: `${s2.kills || 0}`, inline: true },
      { name: '📊 K/D', value: `${s1.kdr || 0}`, inline: true },
      { name: '', value: '', inline: true },
      { name: '📊 K/D', value: `${s2.kdr || 0}`, inline: true },
      { name: '🎯 Win Rate', value: `${s1.winRate || 0}%`, inline: true },
      { name: '', value: '', inline: true },
      { name: '🎯 Win Rate', value: `${s2.winRate || 0}%`, inline: true },
      { name: '💥 Damage', value: `${(s1.damageDealt || 0).toLocaleString()}`, inline: true },
      { name: '', value: '', inline: true },
      { name: '💥 Damage', value: `${(s2.damageDealt || 0).toLocaleString()}`, inline: true }
    );

  await interaction.editReply({ embeds: [embed] });
}

// ─── /leaderboard ───────────────────────────────────────────────
async function cmdLeaderboard(interaction) {
  await interaction.deferReply();
  const category = interaction.options.getString('category') || 'wins';

  const lb = await api.get(`/leaderboard?category=${category}&limit=15`);
  if (!lb?.results) return interaction.editReply('❌ Failed to load leaderboard');

  const emojis = { wins: '🏆', kills: '💀', damage: '💥', kdr: '📊', level: '⭐' };
  const names = { wins: 'Wins', kills: 'Kills', damage: 'Damage', kdr: 'K/D Ratio', level: 'Level' };
  const emoji = emojis[category] || '🏆';
  const name = names[category] || 'Unknown';

  const medals = ['🥇', '🥈', '🥉'];
  const entries = lb.results.map((p, i) => {
    const medal = medals[i] || `${p.rank}.`;
    const kdr = category === 'kdr' ? ` (${p.kdr || 0})` : '';
    const extra = p.subValue !== undefined ? ` [${p.subValue}k]` : '';
    return `${medal} **${p.displayName || p.username}** — ${p.value}${extra}${kdr}`;
  }).join('\n');

  const embed = new EmbedBuilder()
    .setColor(0xFF6B35)
    .setTitle(`${emoji} ${name} Leaderboard`)
    .setDescription(entries || 'No data yet')
    .setFooter({ text: `Page 1 • ${lb.total || 0} players tracked` })
    .setTimestamp();

  await interaction.editReply({ embeds: [embed] });
}

// ─── /rank ──────────────────────────────────────────────────────
async function cmdRank(interaction) {
  await interaction.deferReply();
  const playerName = interaction.options.getString('player');

  // Default to searching the user's linked account (future feature)
  const query = playerName || 'vanguard';
  const search = await api.get(`/players/search?q=${encodeURIComponent(query)}`);
  if (!search?.results?.length) return interaction.editReply(`❌ Player "${query}" not found`);

  const rank = await api.get(`/leaderboard/rank/${search.results[0].playerId}`);
  if (!rank) return interaction.editReply('❌ Failed to load rankings');

  const embed = new EmbedBuilder()
    .setColor(0x00D4FF)
    .setTitle(`📈 ${rank.username} — Rankings`)
    .addFields(
      { name: '🏆 Wins', value: `#${rank.ranks?.wins || '?'} (${rank.stats?.wins || 0})`, inline: true },
      { name: '💀 Kills', value: `#${rank.ranks?.kills || '?'} (${rank.stats?.kills || 0})`, inline: true },
      { name: '💥 Damage', value: `#${rank.ranks?.damage || '?'} (${(rank.stats?.damage || 0).toLocaleString()})`, inline: true },
      { name: '⭐ Level', value: `#${rank.ranks?.level || '?'} (${rank.stats?.level || 1})`, inline: true }
    );

  await interaction.editReply({ embeds: [embed] });
}

// ─── /match ─────────────────────────────────────────────────────
async function cmdMatch(interaction) {
  await interaction.deferReply();
  const matchId = interaction.options.getString('matchid');

  const match = await api.get(`/matches/${matchId}`);
  if (!match?.match) return interaction.editReply(`❌ Match \`${matchId}\` not found`);

  const m = match.match;
  const winner = m.players?.find(p => p.placement === 1);
  const topPlayers = (m.players || []).sort((a, b) => (a.placement || 999) - (b.placement || 999)).slice(0, 10);

  const embed = new EmbedBuilder()
    .setColor(0xFF6B35)
    .setTitle(`🎮 Match ${m.matchId}`)
    .addFields(
      { name: '📋 Mode', value: m.mode || 'solo', inline: true },
      { name: '🗺️ Map', value: m.map || 'Arena-7', inline: true },
      { name: '👥 Players', value: `${m.playerCount || '?'}`, inline: true },
      { name: '⏱️ Duration', value: m.duration ? `${Math.floor(m.duration / 60)}m ${m.duration % 60}s` : 'Unknown', inline: true },
      { name: '🏆 Winner', value: winner ? `**${winner.username}** (${winner.kills}k)` : 'Unknown', inline: true },
      { name: '📅 Ended', value: m.endedAt ? new Date(m.endedAt).toLocaleString() : 'Unknown', inline: false }
    )
    .setTimestamp();

  // Add top players if available
  if (topPlayers.length > 1) {
    const standings = topPlayers.map((p, i) =>
      `#${i + 1} — **${p.username}** — ${p.kills || 0}k / ${p.deaths || 0}d`
    ).join('\n');
    embed.addFields({ name: '📊 Top Players', value: standings, inline: false });
  }

  await interaction.editReply({ embeds: [embed] });
}

// ─── /recent ────────────────────────────────────────────────────
async function cmdRecent(interaction) {
  await interaction.deferReply();
  const count = interaction.options.getInteger('count') || 10;

  const matches = await api.get(`/matches/recent/${count}`);
  if (!matches?.matches?.length) return interaction.editReply('❌ No recent matches');

  const list = matches.matches.slice(0, count).map(m =>
    `\`${m.matchId}\` — ${m.mode || 'solo'} — ${m.map || 'Arena-7'} — ${new Date(m.endedAt).toLocaleDateString()}`
  ).join('\n');

  const embed = new EmbedBuilder()
    .setColor(0x00D4FF)
    .setTitle(`📋 Recent Matches (Last ${Math.min(count, matches.matches.length)})`)
    .setDescription(list)
    .setFooter({ text: 'Arena Fall Match History' });

  await interaction.editReply({ embeds: [embed] });
}

// ─── /tournaments ───────────────────────────────────────────────
async function cmdTournaments(interaction) {
  await interaction.deferReply();

  const tournaments = await api.get('/tournaments?status=upcoming');
  if (!tournaments) return interaction.editReply('❌ Failed to load tournaments');

  const active = tournaments.active || [];
  const upcoming = tournaments.upcoming || [];

  const embed = new EmbedBuilder()
    .setColor(0xFF6B35)
    .setTitle('🏆 Arena Fall — Tournaments')
    .setDescription('Compete for glory and exclusive rewards!');

  if (active.length > 0) {
    embed.addFields({
      name: '🔴 LIVE NOW',
      value: active.map(t => `**${t.name}** — ${t.players}/${t.maxPlayers} players`).join('\n'),
      inline: false
    });
  }

  if (upcoming.length > 0) {
    embed.addFields({
      name: '📅 Upcoming',
      value: upcoming.map(t =>
        `**${t.name}** — ${new Date(t.startDate).toLocaleDateString()} — ${t.players || 0}/${t.maxPlayers || 64} players`
      ).join('\n'),
      inline: false
    });
  }

  if (!active.length && !upcoming.length) {
    embed.setDescription('No tournaments scheduled yet. Stay tuned! 🎯');
  }

  const row = new ActionRowBuilder()
    .addComponents(
      new ButtonBuilder()
        .setURL(`${CONFIG.apiUrl.replace('/api/v1', '')}/tournaments`)
        .setLabel('View All')
        .setStyle(ButtonStyle.Link)
    );

  await interaction.editReply({ embeds: [embed], components: [row] });
}

// ─── /tournament ────────────────────────────────────────────────
async function cmdTournament(interaction) {
  await interaction.deferReply();
  const id = interaction.options.getString('id');
  const t = await api.get(`/tournaments/${id}`);

  if (!t) return interaction.editReply(`❌ Tournament \`${id}\` not found`);

  const embed = new EmbedBuilder()
    .setColor(0xFF6B35)
    .setTitle(`🏆 ${t.name || 'Tournament'}`)
    .addFields(
      { name: '📅 Date', value: t.startDate ? new Date(t.startDate).toLocaleDateString() : 'TBD', inline: true },
      { name: '👥 Players', value: `${t.players || 0}/${t.maxPlayers || 64}`, inline: true },
      { name: '📋 Status', value: t.status || 'upcoming', inline: true },
      { name: '🎁 Reward', value: t.reward || 'Exclusive Skin', inline: false }
    );

  if (t.bracket) {
    embed.addFields({ name: '📊 Bracket', value: t.bracket, inline: false });
  }

  await interaction.editReply({ embeds: [embed] });
}

// ─── /search ────────────────────────────────────────────────────
async function cmdSearch(interaction) {
  await interaction.deferReply();
  const query = interaction.options.getString('query');

  const results = await api.get(`/players/search?q=${encodeURIComponent(query)}`);
  if (!results?.results?.length) return interaction.editReply(`❌ No players found matching "${query}"`);

  const embed = new EmbedBuilder()
    .setColor(0x00D4FF)
    .setTitle(`🔍 Search Results: "${query}"`)
    .setDescription(
      results.results.map((p, i) =>
        `${i + 1}. **${p.displayName || p.username}** — ⭐ Lvl ${p.level} — ${p.title || 'Recruit'}`
      ).join('\n')
    )
    .setFooter({ text: `${results.results.length} players found` });

  await interaction.editReply({ embeds: [embed] });
}

// ─── /admin ─────────────────────────────────────────────────────
async function cmdAdmin(interaction) {
  if (CONFIG.adminRoleId && !interaction.member.roles.cache.has(CONFIG.adminRoleId)) {
    return interaction.reply({ content: '❌ You need admin role to use this command', ephemeral: true });
  }

  await interaction.deferReply({ ephemeral: true });
  const sub = interaction.options.getSubcommand();
  const target = interaction.options.getString('player');
  const amount = interaction.options.getInteger('amount');

  if (sub === 'ban') {
    const reason = interaction.options.getString('reason');
    await logToChannel(`🚫 **${interaction.user.tag}** banned **${target}** — ${reason}`);
    await interaction.editReply(`✅ Banned **${target}** — ${reason}`);
  }

  if (sub === 'unban') {
    await logToChannel(`✅ **${interaction.user.tag}** unbanned **${target}**`);
    await interaction.editReply(`✅ Unbanned **${target}**`);
  }

  if (sub === 'give') {
    const currency = interaction.options.getString('currency');
    await logToChannel(`💰 **${interaction.user.tag}** gave ${amount} ${currency} to **${target}**`);
    await interaction.editReply(`✅ Gave ${amount} ${currency} to **${target}**`);
  }

  if (sub === 'announce') {
    const message = interaction.options.getString('message');
    // Post to announcement channel
    const channel = client.channels.cache.get(CONFIG.logChannelId);
    if (channel) {
      const embed = new EmbedBuilder()
        .setColor(0xFF6B35)
        .setTitle('📢 Arena Fall Announcement')
        .setDescription(message)
        .setFooter({ text: `Posted by ${interaction.user.tag}` })
        .setTimestamp();
      await channel.send({ embeds: [embed] });
    }
    await interaction.editReply(`✅ Announcement posted`);
  }
}

// ═══════════════════════════════════════════════════
//  SCHEDULED TASKS
// ═══════════════════════════════════════════════════

async function postLeaderboardUpdate() {
  const channel = client.channels.cache.get(CONFIG.logChannelId);
  if (!channel) return;

  const lb = await api.get('/leaderboard?category=wins&limit=10');
  if (!lb?.results) return;

  const embed = new EmbedBuilder()
    .setColor(0xFF6B35)
    .setTitle('📊 Daily Leaderboard — Top 10')
    .setDescription(lb.results.map((p, i) =>
      `**${i + 1}.** ${p.displayName || p.username} — ${p.value} wins`
    ).join('\n'))
    .setFooter({ text: 'Updated daily' })
    .setTimestamp();

  await channel.send({ embeds: [embed] });
}

async function postTournamentReminder() {
  const channel = client.channels.cache.get(CONFIG.logChannelId);
  if (!channel) return;

  const tournaments = await api.get('/tournaments?status=upcoming');
  const upcoming = tournaments?.upcoming || [];

  if (upcoming.length > 0) {
    const embed = new EmbedBuilder()
      .setColor(0xFF6B35)
      .setTitle('🏆 Weekly Tournament Reminder!')
      .setDescription(upcoming.map(t =>
        `**${t.name}** — ${new Date(t.startDate).toLocaleDateString()} — ${t.players || 0}/${t.maxPlayers || 64}`
      ).join('\n'))
      .setFooter({ text: 'Register now!' });

    await channel.send({ embeds: [embed] });
  }
}

async function checkServerHealth() {
  const health = await api.get('/health');
  if (!health) {
    const channel = client.channels.cache.get(CONFIG.logChannelId);
    if (channel) await channel.send('⚠️ **API Health Check Failed!** Server may be down.');
  }
}

// ─── Logging Helper ─────────────────────────────────────────────
async function logToChannel(message) {
  const channel = client.channels.cache.get(CONFIG.logChannelId);
  if (channel) await channel.send(message);
}

// ─── Login ──────────────────────────────────────────────────────
const token = process.env.DISCORD_TOKEN;
if (!token) {
  console.error('❌ DISCORD_TOKEN environment variable required');
  process.exit(1);
}

client.login(token);
