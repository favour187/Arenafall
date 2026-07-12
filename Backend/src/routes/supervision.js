const express = require('express');
const router = express.Router();
const { authenticate, adminOnly } = require('../middleware/auth');

let supervisionInstance = null;

function setSupervisionService(instance) {
  supervisionInstance = instance;
}

// GET /api/v1/supervision/status
router.get('/status', [authenticate, adminOnly], (req, res) => {
  if (!supervisionInstance) {
    return res.status(503).json({
      success: false,
      message: 'Supervision service not initialized'
    });
  }
  return res.json({
    success: true,
    data: supervisionInstance.getSupervisionSnapshot()
  });
});

// POST /api/v1/supervision/action
router.post('/action', [authenticate, adminOnly], async (req, res) => {
  if (!supervisionInstance) {
    return res.status(503).json({
      success: false,
      message: 'Supervision service not initialized'
    });
  }
  const { action, payload } = req.body;
  if (!action) {
    return res.status(400).json({ success: false, message: 'Action parameter is required.' });
  }

  const result = await supervisionInstance.performAdminAction(action, payload);
  return res.json(result);
});

module.exports = router;
module.exports.setSupervisionService = setSupervisionService;
