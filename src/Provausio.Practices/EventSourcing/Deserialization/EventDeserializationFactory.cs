using System;
using System.Collections.Generic;
using Provausio.Core.Logging;

namespace Provausio.Practices.EventSourcing.Deserialization
{
    public class EventDeserializationFactory : IEventDeserializationFactory
    {
        private readonly List<IEventDeserializationStrategy> _deserializationStrategies = new List<IEventDeserializationStrategy>();

        private EventDeserializationFactory(
            ByAttributeStrategy byAttributeStrategy, 
            AttributeAsAssemblyQualifiedNameStrategy attributeAsAssemblyQualifiedNameStrategy)
        {
            // in order of likeliness
            _deserializationStrategies.Add(byAttributeStrategy);
            _deserializationStrategies.Add(new ByAssemblyQualifiedTypeNameStrategy());
            _deserializationStrategies.Add(new ByJsonTypeStrategy());
            _deserializationStrategies.Add(attributeAsAssemblyQualifiedNameStrategy);
        }

        public EventDeserializationFactory(string eventLibraryPath)
            : this(new ByAttributeStrategy(eventLibraryPath), 
                   new AttributeAsAssemblyQualifiedNameStrategy(eventLibraryPath)) { }

        public EventDeserializationFactory(params Type[] typeRefs)
            : this(new ByAttributeStrategy(typeRefs), 
                   new AttributeAsAssemblyQualifiedNameStrategy(typeRefs)) { }

        public bool TryDeserialize(byte[] eventData, byte[] eventMetaData, out EventInfo e)
        {
            e = null;
            foreach (var strategy in _deserializationStrategies)
            {
                try
                {
                    e = strategy.DeserializeEvent(eventData, eventMetaData);
                    if (e != null)
                    {
                        //Logger.Debug($"Deserialization succeeded using {strategy} ({e.GetType()})", this);
                        return true;
                    }
                }
                catch { }
            }

            Logger.Error("All deserialization strategies failed.", this);
            return false;
        }
    }
}