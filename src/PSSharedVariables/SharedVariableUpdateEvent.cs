using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PSSharedVariables
{
    public abstract class SharedVariableUpdateEvent : INotification
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
    }

    public sealed class SharedVariableSetEvent : SharedVariableUpdateEvent
    {
        public SharedVariableSetEvent(string name, object value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }

        public string Name { get; }
        public object Value { get; }
    }
    public sealed class SharedVariableRemoveEvent : SharedVariableUpdateEvent
    {
        public SharedVariableRemoveEvent(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
        public string Name { get; }
    }

    public sealed class SharedVariableUpdateConsolidationEvent : SharedVariableUpdateEvent
    {
        public SharedVariableUpdateConsolidationEvent(IDictionary<string, object> items) : base() => Items = items.ToImmutableDictionary();
        public IReadOnlyDictionary<string, object> Items { get; private set; }
    }
}
