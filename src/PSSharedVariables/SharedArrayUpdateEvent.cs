using MediatR;
using System;

namespace PSSharedVariables
{
    public abstract class SharedArrayUpdateEvent : INotification
    {
        protected SharedArrayUpdateEvent(Guid arrayId) => ArrayId = arrayId;
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid ArrayId { get; private set; }
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
    }

    public sealed class SharedArrayClearedEvent : SharedArrayUpdateEvent
    {
        public SharedArrayClearedEvent(Guid arrayId) : base(arrayId) { }
    }

    public sealed class SharedArrayRemovedEvent : SharedArrayUpdateEvent
    {
        public SharedArrayRemovedEvent(Guid arrayId, int index) : base(arrayId) => Index = index;
        public int Index { get; private set; }
    }

    public sealed class SharedArraySetEvent : SharedArrayUpdateEvent
    {
        public SharedArraySetEvent(Guid arrayId, int index, object item) : base(arrayId)
        {
            Index = index;
            Item = item;
        }
        public int Index { get; private set; }
        public object Item { get; private set; }
    }

    public sealed class SharedArrayAddEvent : SharedArrayUpdateEvent
    {
        public SharedArrayAddEvent(Guid arrayId, int index, object item) : base(arrayId)
        {
            Index = index;
            Item = item;
        }
        public int Index { get; private set; }
        public object Item { get; private set; }
    }

    public sealed class SharedArrayUpdateConsolidationEvent : SharedArrayUpdateEvent
    {
        public SharedArrayUpdateConsolidationEvent(Guid arrayId, object[] items) : base(arrayId) => Items = items;
        public object[] Items { get; private set; }
    }
}
