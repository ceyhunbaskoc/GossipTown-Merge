using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Architecture
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            Type type = typeof(T);
            if (!_services.ContainsKey(type))
            {
                _services.Add(type, service);
            }
            else
            {
                _services[type] = service;
            }
        }
        public static T Get<T>()
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object service))
            {
                return (T)service;
            }
            return default;
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }
}