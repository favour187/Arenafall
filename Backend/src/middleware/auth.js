const jwt = require('jsonwebtoken');
const { logger } = require('../server');

/**
 * JWT Authentication Middleware
 * Verifies Bearer token from Authorization header
 */
const authenticate = (req, res, next) => {
  const authHeader = req.headers.authorization;
  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    return res.status(401).json({
      error: 'Authentication required',
      code: 'AUTH_REQUIRED'
    });
  }

  const token = authHeader.split(' ')[1];
  if (!token) {
    return res.status(401).json({
      error: 'Invalid token format',
      code: 'INVALID_TOKEN'
    });
  }

  try {
    const secret = process.env.JWT_SECRET || 'arenafall-dev-secret-key-2024';
    const decoded = jwt.verify(token, secret);
    
    req.player = {
      playerId: decoded.playerId,
      username: decoded.username,
      email: decoded.email,
      level: decoded.level
    };
    req.token = token;
    next();
  } catch (err) {
    if (err.name === 'TokenExpiredError') {
      return res.status(401).json({
        error: 'Token expired',
        code: 'TOKEN_EXPIRED',
        refreshToken: true
      });
    }
    return res.status(401).json({
      error: 'Invalid token',
      code: 'INVALID_TOKEN'
    });
  }
};

/**
 * Optional Authentication — doesn't fail if no token
 */
const optionalAuth = (req, res, next) => {
  const authHeader = req.headers.authorization;
  if (!authHeader) return next();

  const token = authHeader.split(' ')[1];
  if (!token) return next();

  try {
    const secret = process.env.JWT_SECRET || 'arenafall-dev-secret-key-2024';
    const decoded = jwt.verify(token, secret);
    req.player = {
      playerId: decoded.playerId,
      username: decoded.username
    };
  } catch (err) {
    // Silently ignore invalid tokens for optional auth
  }
  next();
};

/**
 * Rate limiting by player ID
 */
const playerRateLimit = (maxRequests = 60, windowMs = 60000) => {
  const requests = new Map();

  return (req, res, next) => {
    const playerId = req.player?.playerId || req.ip;
    const now = Date.now();

    if (!requests.has(playerId)) {
      requests.set(playerId, []);
    }

    const timestamps = requests.get(playerId).filter(t => now - t < windowMs);
    timestamps.push(now);
    requests.set(playerId, timestamps);

    if (timestamps.length > maxRequests) {
      return res.status(429).json({
        error: 'Too many requests',
        code: 'PLAYER_RATE_LIMIT',
        retryAfter: Math.ceil((timestamps[0] + windowMs - now) / 1000)
      });
    }

    next();
  };
};

/**
 * Validate request body against a schema
 */
const validate = (schema) => {
  return (req, res, next) => {
    const errors = [];
    
    for (const [field, rules] of Object.entries(schema)) {
      const value = req.body[field];
      
      for (const rule of rules) {
        if (rule.required && (value === undefined || value === null || value === '')) {
          errors.push({ field, message: `${field} is required` });
          break;
        }
        if (value !== undefined && value !== null) {
          if (rule.minLength && String(value).length < rule.minLength) {
            errors.push({ field, message: `${field} must be at least ${rule.minLength} characters` });
          }
          if (rule.maxLength && String(value).length > rule.maxLength) {
            errors.push({ field, message: `${field} must be at most ${rule.maxLength} characters` });
          }
          if (rule.pattern && !rule.pattern.test(value)) {
            errors.push({ field, message: `${field} format is invalid` });
          }
          if (rule.min !== undefined && value < rule.min) {
            errors.push({ field, message: `${field} must be at least ${rule.min}` });
          }
          if (rule.max !== undefined && value > rule.max) {
            errors.push({ field, message: `${field} must be at most ${rule.max}` });
          }
          if (rule.oneOf && !rule.oneOf.includes(value)) {
            errors.push({ field, message: `${field} must be one of: ${rule.oneOf.join(', ')}` });
          }
        }
      }
    }

    if (errors.length > 0) {
      return res.status(400).json({ error: 'Validation failed', code: 'VALIDATION_ERROR', errors });
    }
    next();
  };
};

/**
 * Admin-only middleware
 */
const adminOnly = (req, res, next) => {
  if (!req.player?.isAdmin) {
    return res.status(403).json({
      error: 'Admin access required',
      code: 'FORBIDDEN'
    });
  }
  next();
};

module.exports = { authenticate, optionalAuth, playerRateLimit, validate, adminOnly };
