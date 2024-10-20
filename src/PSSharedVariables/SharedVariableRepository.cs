using MediatR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSSharedVariables
{
    public sealed class SharedVariable
    {
        public SharedVariable(string Name, object Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public string Name { get; }
        public object Value { get; internal set; }
    }

    public sealed class SharedVariableRepository
    {
        private readonly ConcurrentDictionary<string, SharedVariable> variables = new ConcurrentDictionary<string, SharedVariable>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ConcurrentDictionary<Guid, SharedVariableUpdateEvent> updateEvents = new ConcurrentDictionary<Guid, SharedVariableUpdateEvent>();
        private readonly IPublisher publisher;
        private readonly IObjectConverter converter;

        public SharedVariableRepository(IPublisher publisher, IObjectConverter converter, string root)
        {
            this.publisher = publisher;
            this.converter = converter;
            Root = root;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public string Root { get; }
        public DateTime LastUpdateTime { get; private set; }
        private readonly object lastUpdateTimeLock = new object();

        private bool CheckOrUpdateUpdateTime(DateTime eventTime)
        {
            if (eventTime > LastUpdateTime)
                lock (lastUpdateTimeLock)
                    if (eventTime > LastUpdateTime)
                    {
                        LastUpdateTime = eventTime;
                        return true;
                    }

            return false;
        }

        public SharedVariable[] Get() => variables.Select(i => i.Value).ToArray();
        public object Get(string name)
        {
            if (variables.TryGetValue(name, out var variable))
                return variable.Value;

            return null;
        }
        public bool Contains(string name) => variables.ContainsKey(name);

        public void Set(string name, object value)
        {
            var setEvent = new SharedVariableSetEvent(name, converter.Convert(value));
            var task = publisher.Publish(setEvent);
            ProcessEvent(setEvent);
            task.Wait();
        }
        public void Remove(string name)
        {
            var removeEvent = new SharedVariableRemoveEvent(name);
            var task = publisher.Publish(removeEvent);
            ProcessEvent(removeEvent);
            task.Wait();
        }

        internal void ProcessEvent(SharedVariableUpdateEvent updateEvent)
        {
            if (updateEvent is SharedVariableSetEvent setEvent)
                ProcessEvent(setEvent);
            else if (updateEvent is SharedVariableRemoveEvent removeEvent)
                ProcessEvent(removeEvent);
        }

        internal void ProcessEvent(SharedVariableSetEvent setEvent)
        {
            if (updateEvents.TryAdd(setEvent.Id, setEvent))
                if (CheckOrUpdateUpdateTime(setEvent.Timestamp))
                    variables.AddOrUpdate(setEvent.Name, new SharedVariable(setEvent.Name, setEvent.Value), (key, variable) =>
                    {
                        variable.Value = setEvent.Value;
                        return variable;
                    });
                else
                    ReprocessEvents(setEvent.Timestamp);
        }

        internal void ProcessEvent(SharedVariableRemoveEvent removeEvent)
        {
            if (updateEvents.TryAdd(removeEvent.Id, removeEvent))
                if (CheckOrUpdateUpdateTime(removeEvent.Timestamp))
                    variables.TryRemove(removeEvent.Name, out _);
                else
                    ReprocessEvents(removeEvent.Timestamp);
        }

        private void ReprocessEvents(DateTime startTime)
        {
            foreach (var udEvent in updateEvents.Values.Where(e => e.Timestamp >= startTime).OrderBy(e => e.Timestamp))
                if (udEvent is SharedVariableSetEvent setEvent)
                    variables.AddOrUpdate(setEvent.Name, new SharedVariable(setEvent.Name, setEvent.Value), (key, variable) =>
                    {
                        variable.Value = setEvent.Value;
                        return variable;
                    });
                else if (udEvent is SharedVariableRemoveEvent removeEvent)
                    variables.TryRemove(removeEvent.Name, out _);
        }
    }
}
