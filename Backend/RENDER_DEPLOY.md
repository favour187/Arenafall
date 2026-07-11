# 🚀 Arena Fall — Render Deployment Guide

## Option 1: One-Click Deploy (Easiest)
[![Deploy to Render](https://render.com/images/deploy-to-render-button.svg)](https://render.com/deploy?blueprint-url=https://raw.githubusercontent.com/favour187/Arenafall/main/Backend/render.yaml)

This will auto-create:
- ✅ Web Service (Node.js) — your game API
- ✅ MongoDB — your database

Just connect your repo and click Deploy. No Docker needed.

## Option 2: Manual Deploy (More Control)

### Step 1: Create a Web Service
1. Go to [dashboard.render.com](https://dashboard.render.com)
2. Click **New + → Web Service**
3. Connect your GitHub repo: `favour187/Arenafall`
4. **IMPORTANT:** Do NOT select Docker. Render will auto-detect Node.js.
5. Fill in:

| Field | Value |
|-------|-------|
| **Name** | `arenafall-api` |
| **Region** | Oregon (US) / Frankfurt (EU) / Singapore (Asia) |
| **Branch** | `main` |
| **Root Directory** | `Backend` ← **CRITICAL** |
| **Runtime** | `Node` (auto-detected, NOT Docker) |
| **Build Command** | `npm install` |
| **Start Command** | `node src/server.js` |
| **Plan** | Starter ($7/mo) or Free

### Step 2: Add Environment Variables
Under **Environment Variables**, add these:

```env
NODE_ENV=production
PORT=3000
JWT_SECRET=[click "Generate Value"]
JWT_REFRESH_SECRET=[click "Generate Value"]
JWT_EXPIRY=24h
JWT_REFRESH_EXPIRY=7d
RATE_LIMIT_MAX=100
ANTICHEAT_ENABLED=true
MAX_PLAYERS_PER_MATCH=60
ENCRYPTION_KEY=[generate]
ENCRYPTION_IV=[generate]
LOG_LEVEL=info
```

### Step 3: Add MongoDB
**Option A — Render MongoDB (easiest):**
1. Click **New + → MongoDB**
2. Name: `arenafall-mongodb`
3. Plan: Free (512MB) or Starter (1GB)
4. After creation, copy the **Internal Connection String**
5. Add it as env var in your Web Service:
   ```
   MONGODB_URI=render-mongodb://internal-connection-string
   ```

**Option B — MongoDB Atlas (recommended for production):**
1. Go to [mongodb.com/atlas](https://mongodb.com/atlas) → Create free cluster
2. Get connection string: `mongodb+srv://user:pass@cluster.mongodb.net/arenafall`
3. Add to env vars:
   ```
   MONGODB_URI=mongodb+srv://youruser:yourpass@cluster.mongodb.net/arenafall
   ```

### Step 4: Deploy 🚀
Click **Create Web Service**. Render will:
- Clone your repo
- Install dependencies
- Start the server
- Your API is live at: `https://arenafall-api.onrender.com`

### Step 5: Verify
Visit: `https://arenafall-api.onrender.com/health`

You should see:
```json
{
  "status": "online",
  "version": "v1",
  "services": {
    "database": "connected",
    "redis": "disconnected"
  },
  "game": "Arena Fall Battle Royale"
}
```

> Redis is optional — the server runs fine without it (just disables caching)

---

## 📡 Connecting Unity to Your Backend

In Unity, create a file `Assets/Scripts/Networking/ServerConfig.cs`:

```csharp
public static class ServerConfig
{
    // After deploying, update this URL
    public static string API_URL = "https://arenafall-api.onrender.com/api/v1";
    public static string WS_URL = "wss://arenafall-api.onrender.com";
    
    // For local testing:
    // public static string API_URL = "http://localhost:3000/api/v1";
    // public static string WS_URL = "ws://localhost:3000";
}
```

Then in your game's login screen:
```csharp
// POST /api/v1/auth/login
POST https://arenafall-api.onrender.com/api/v1/auth/login
Body: { "username": "player1", "password": "password123" }
Response: { "accessToken": "...", "player": {...} }

// POST /api/v1/matches/submit (send match results)
POST https://arenafall-api.onrender.com/api/v1/matches/submit
Headers: { "Authorization": "Bearer <token>" }
Body: { "matchId": "...", "mode": "solo", "player": { "placement": 1, "kills": 5 } }
```

---

## 📊 Render Dashboard Features

| Feature | How to Access |
|---------|--------------|
| **Logs** | Render Dashboard → Your Service → Logs tab |
| **Metrics** | Render Dashboard → Your Service → Metrics (CPU, RAM, Network) |
| **Env Vars** | Render Dashboard → Your Service → Environment |
| **Deploy History** | Render Dashboard → Your Service → Events |
| **Custom Domain** | Render Dashboard → Your Service → Settings → Domains |
| **Auto-scaling** | Render Dashboard → Your Service → Settings → Scaling (paid plans) |

---

## 💰 Render Pricing for Arena Fall

| Plan | Price | What You Get | Players Supported |
|-----|-------|-------------|-------------------|
| **Free** (Web) | $0/mo | Sleeps after 15min idle | Testing only |
| **Starter** | $7/mo | 0.5 CPU, 512MB RAM, always on | ~50 concurrent |
| **Professional** | $20/mo | 1 CPU, 1GB RAM | ~200 concurrent |
| **Advanced** | $40/mo | 2 CPU, 2GB RAM | ~500 concurrent |
| **MongoDB Free** | $0/mo | 512MB storage | Testing |
| **MongoDB Starter** | $7/mo | 1GB storage | Production |

**Recommended setup for launch:** Web Service Starter ($7) + MongoDB Free ($0) = **$7/month**
