using System.Collections.Generic;
using UnityEngine;
using ArenaFall.Core;

namespace ArenaFall.Managers
{
    /// <summary>
    /// Manages game analytics events for monitoring player behavior and game performance.
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private bool _logToConsole = false;

        private Queue<AnalyticsEvent> _eventQueue = new();
        private float _flushInterval = 30f;
        private float _flushTimer;

        public static AnalyticsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register<AnalyticsManager>(this);
        }

        private void Update()
        {
            if (!_enableAnalytics) return;

            _flushTimer += Time.deltaTime;
            if (_flushTimer >= _flushInterval)
            {
                _flushTimer = 0;
                FlushEvents();
            }
        }

        /// <summary>
        /// Track a game start event.
        /// </summary>
        public void TrackGameStart()
        {
            LogEvent("game_start", new Dictionary<string, object>
            {
                { "platform", Application.platform.ToString() },
                { "version", Application.version },
                { "timestamp", Time.time }
            });
        }

        /// <summary>
        /// Track match start.
        /// </summary>
        public void TrackMatchStart(string mode, int playerCount)
        {
            LogEvent("match_start", new Dictionary<string, object>
            {
                { "mode", mode },
                { "player_count", playerCount },
                { "timestamp", Time.time }
            });
        }

        /// <summary>
        /// Track match end.
        /// </summary>
        public void TrackMatchEnd(int placement, int kills, float survivalTime)
        {
            LogEvent("match_end", new Dictionary<string, object>
            {
                { "placement", placement },
                { "kills", kills },
                { "survival_time", survivalTime },
                { "timestamp", Time.time }
            });
        }

        /// <summary>
        /// Track player death.
        /// </summary>
        public void TrackPlayerDeath(string weaponId, string killedBy)
        {
            LogEvent("player_death", new Dictionary<string, object>
            {
                { "weapon", weaponId },
                { "killed_by", killedBy },
                { "timestamp", Time.time }
            });
        }

        /// <summary>
        /// Track item pickups.
        /// </summary>
        public void TrackItemPickup(string itemId, string category)
        {
            LogEvent("item_pickup", new Dictionary<string, object>
            {
                { "item_id", itemId },
                { "category", category }
            });
        }

        /// <summary>
        /// Track performance metrics.
        /// </summary>
        public void TrackPerformance(float fps, float frameTime, int drawCalls, int triangleCount)
        {
            LogEvent("performance", new Dictionary<string, object>
            {
                { "fps", Mathf.RoundToInt(fps) },
                { "frame_time_ms", Mathf.RoundToInt(frameTime * 1000) },
                { "draw_calls", drawCalls },
                { "triangles", triangleCount }
            });
        }

        /// <summary>
        /// Track an error event.
        /// </summary>
        public void TrackError(string errorType, string message)
        {
            LogEvent("error", new Dictionary<string, object>
            {
                { "type", errorType },
                { "message", message },
                { "timestamp", Time.time }
            });
        }

        /// <summary>
        /// Track a custom event.
        /// </summary>
        public void TrackCustomEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            LogEvent(eventName, parameters);
        }

        private void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (!_enableAnalytics) return;

            var analyticsEvent = new AnalyticsEvent
            {
                eventName = eventName,
                parameters = parameters ?? new Dictionary<string, object>(),
                timestamp = System.DateTime.UtcNow
            };

            _eventQueue.Enqueue(analyticsEvent);

            if (_logToConsole)
            {
                Debug.Log($"[Analytics] {eventName}: {string.Join(", ", parameters)}");
            }
        }

        private void FlushEvents()
        {
            if (_eventQueue.Count == 0) return;

            var events = new List<AnalyticsEvent>();
            while (_eventQueue.Count > 0)
            {
                events.Add(_eventQueue.Dequeue());
            }

            // In production, send to analytics service
            if (_logToConsole)
            {
                Debug.Log($"[Analytics] Flushed {events.Count} events");
            }
        }

        private class AnalyticsEvent
        {
            public string eventName;
            public Dictionary<string, object> parameters;
            public System.DateTime timestamp;
        }
    }
}
