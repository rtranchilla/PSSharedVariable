using MediatR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PSSharedVariables
{
    public abstract class SharedHashtableUpdateEvent : INotification
    {
        protected SharedHashtableUpdateEvent(Guid hashtableId) => HashtableId = hashtableId;
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid HashtableId { get; private set; }
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
    }

    public sealed class SharedHashtableSetEvent : SharedHashtableUpdateEvent
    {
        public SharedHashtableSetEvent(Guid hashtableId, string key, object value) : base(hashtableId)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value;
        }
        public string Key { get; }
        public object Value { get; }
    }

    public sealed class SharedHashtableRemovedEvent : SharedHashtableUpdateEvent
    {
        public SharedHashtableRemovedEvent(Guid hashtableId, string key) : base(hashtableId) => Key = key ?? throw new ArgumentNullException(nameof(key));
        public string Key { get; }
    }

    public sealed class SharedHashtableClearedEvent : SharedHashtableUpdateEvent
    {
        public SharedHashtableClearedEvent(Guid hashtableId) : base(hashtableId) { }
    }

    public sealed class SharedHashtableUpdateConsolidationEvent : SharedHashtableUpdateEvent
    {
        public SharedHashtableUpdateConsolidationEvent(Guid hashtableId, IDictionary<string, object> items) : base(hashtableId) => Items = items.ToImmutableDictionary();
        public IReadOnlyDictionary<string, object> Items { get; private set; }
    }
}
