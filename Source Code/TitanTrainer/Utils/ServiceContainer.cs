using System;
using System.Collections.Concurrent;

namespace TitanTrainer.Utils
{
    /// <summary>
    /// Lightweight service container used for simple DI inside the trainer.
    /// Not a general-purpose DI framework.
    /// </summary>
    public sealed class ServiceContainer
    {
        private readonly ConcurrentDictionary<Type, object> _services = new ConcurrentDictionary<Type, object>();

        public void Register<TService>(TService instance) where TService : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _services[typeof(TService)] = instance;
        }

        public void Register(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _services[instance.GetType()] = instance;
        }

        public TService Resolve<TService>() where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out var svc)) return svc as TService;
            return null;
        }

        public object Resolve(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (_services.TryGetValue(type, out var svc)) return svc;
            return null;
        }
    }
}