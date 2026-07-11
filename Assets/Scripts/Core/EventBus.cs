using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArenaFall.Core
{
    /// <summary>
    /// Central event bus for decoupled communication between systems.
    /// Implements a publish/subscribe pattern with type-safe events.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private static readonly Dictionary<Type, List<Delegate>> _pendingRemovals = new();
        private static bool _isRaising;

        /// <summary>
        /// Subscribe to an event type. Handler will be called when the event is raised.
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : GameEvent
        {
            Type eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }
            _handlers[eventType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from an event type.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : GameEvent
        {
            Type eventType = typeof(T);
            if (_isRaising)
            {
                if (!_pendingRemovals.ContainsKey(eventType))
                {
                    _pendingRemovals[eventType] = new List<Delegate>();
                }
                _pendingRemovals[eventType].Add(handler);
                return;
            }

            if (_handlers.ContainsKey(eventType))
            {
                _handlers[eventType].Remove(handler);
            }
        }

        /// <summary>
        /// Raise an event, notifying all subscribers.
        /// </summary>
        public static void Raise<T>(T eventData) where T : GameEvent
        {
            Type eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType)) return;

            _isRaising = true;
            var handlers = _handlers[eventType].ToArray();
            foreach (var handler in handlers)
            {
                try
                {
                    (handler as Action<T>)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] Error handling {eventType.Name}: {ex.Message}");
                }
            }
            _isRaising = false;

            ProcessPendingRemovals(eventType);
        }

        private static void ProcessPendingRemovals(Type eventType)
        {
            if (_pendingRemovals.ContainsKey(eventType))
            {
                foreach (var handler in _pendingRemovals[eventType])
                {
                    if (_handlers.ContainsKey(eventType))
                    {
                        _handlers[eventType].Remove(handler);
                    }
                }
                _pendingRemovals.Remove(eventType);
            }
        }

        /// <summary>
        /// Clear all subscribers (use when loading scenes).
        /// </summary>
        public static void Clear()
        {
            _handlers.Clear();
            _pendingRemovals.Clear();
            _isRaising = false;
        }
    }

    /// <summary>
    /// Base class for all game events.
    /// </summary>
    public abstract class GameEvent
    {
        public float Timestamp { get; } = Time.time;
    }
}
