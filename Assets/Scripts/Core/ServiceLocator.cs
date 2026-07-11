using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArenaFall.Core
{
    /// <summary>
    /// Service locator pattern for providing global access to services.
    /// Services are registered and accessed via interfaces.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static readonly Dictionary<Type, Stack<object>> _servicePools = new();
        private static bool _initialized;

        /// <summary>
        /// Initialize the service locator.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            Debug.Log("[ServiceLocator] Initialized");
        }

        /// <summary>
        /// Register a service instance.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered, overwriting");
                _services[type] = service;
            }
            else
            {
                _services[type] = service;
            }
            Debug.Log($"[ServiceLocator] Registered {type.Name}");
        }

        /// <summary>
        /// Unregister a service.
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
                Debug.Log($"[ServiceLocator] Unregistered {type.Name}");
            }
        }

        /// <summary>
        /// Get a registered service.
        /// </summary>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            Debug.LogError($"[ServiceLocator] Service {type.Name} not registered!");
            return null;
        }

        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Clear all services (use when unloading scene).
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _servicePools.Clear();
            _initialized = false;
            Debug.Log("[ServiceLocator] Cleared all services");
        }
    }
}
