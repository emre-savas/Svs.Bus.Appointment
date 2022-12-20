using EventBus.Base.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Base.Events;

namespace EventBus.Base.SubManagers
{
    public class InMemoryEventBusSubscriptionManager: IEventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handler;
        private readonly List<Type> _evenTypes;

        public event EventHandler<string> OnEventRemoved;
        public Func<string, string> eventNameGetter;

        public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGetter)
        {
            _handler = new Dictionary<string, List<SubscriptionInfo>>();
            _evenTypes= new List<Type>();
            this.eventNameGetter = eventNameGetter;
        }

        public bool IsEmpty => !_handler.Keys.Any();
        public void Clear() => _handler.Clear();

        public void AddSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            AddSubscription(typeof(TH),eventName);
            if (!_evenTypes.Contains(typeof(T)))
            {
                _evenTypes.Add(typeof(T));
            }
        }

        private void AddSubscription(Type handlerType, string eventName)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handler.Add(eventName,new List<SubscriptionInfo>());
            }

            if (_handler[eventName].Any(s=>s.HandlerType==handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'",
                    nameof(handlerType));
            }

            _handler[eventName].Add(SubscriptionInfo.Typed(handlerType));
        }

        public void RemoveSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            RemoveHandler(eventName,handlerToRemove);
        }

        private void RemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove!=null)
            {
                _handler[eventName].Remove(subsToRemove);
                if (!_handler[eventName].Any())
                {
                    _handler.Remove(eventName);
                    var eventType = _evenTypes.SingleOrDefault(w => w.Name == eventName);
                    if (eventType != null)
                    {
                        _evenTypes.Remove(eventType);
                    }

                    RaiseOnEventRemoved(eventName);
                }
            }
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName)
            => _handler[eventName];

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this,eventName);
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, TH>() where TH : IIntegrationEventHandler<T> where T : IntegrationEvent
        {
            var eventName = GetEventKey<T>();
            return FindSubscriptionToRemove(eventName,typeof(TH));
        }

        private SubscriptionInfo FindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return _handler[eventName].SingleOrDefault(e => e.HandlerType == handlerType);
        }

        public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
        {
            var key=GetEventKey<T>();

            return HasSubscriptionsForEvent(key);
        }

        public bool HasSubscriptionsForEvent(string eventName)
            => _handler.ContainsKey(eventName);

        public Type GetEventTypeByName(string eventName)
            => _evenTypes.SingleOrDefault(e => e.Name == eventName);

        public string GetEventKey<T>()
        {
            string eventName = typeof(T).Name;
            return eventNameGetter(eventName);
        }
    }
}
