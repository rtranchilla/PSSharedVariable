using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PSSharedVariables
{
    public sealed class SharedHashtable : Hashtable, IEnumerable
    {
        private readonly ConcurrentDictionary<Guid, SharedHashtableUpdateEvent> updateEvents = new ConcurrentDictionary<Guid, SharedHashtableUpdateEvent>();
        private readonly ConcurrentDictionary<string, object> dictionary = new ConcurrentDictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        private readonly IPublisher publisher;
        private readonly IObjectConverter converter;

        internal SharedHashtable(IPublisher publisher, IObjectConverter converter) : base()
        {
            this.publisher = publisher;
            this.converter = converter;
        }

        public Guid Id { get; } = Guid.NewGuid();
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

        public override void Add(object key, object value)
        {
            var keyStr = ToKeyString(key);

            if (!dictionary.ContainsKey(keyStr))
            {
                var setEvent = new SharedHashtableSetEvent(Id, keyStr, converter.Convert(value));
                var task = publisher.Publish(setEvent);
                ProcessEvent(setEvent);
                task.Wait();
            }
            else
                throw new ArgumentException($"Item has already been added. Key being added: '{keyStr}'", nameof(keyStr));
        }

        public override void Clear()
        {
            var clearEvent = new SharedHashtableClearedEvent(Id);
            var task = publisher.Publish(clearEvent);
            ProcessEvent(clearEvent);
            task.Wait();
        }

        public override void Remove(object key)
        {
            var removeEvent = new SharedHashtableRemovedEvent(Id, ToKeyString(key));
            var task = publisher.Publish(removeEvent);
            ProcessEvent(removeEvent);
            task.Wait();
        }

        public override object this[object key]
        {
            get => dictionary[ToKeyString(key)];

            set
            {
                var setEvent = new SharedHashtableSetEvent(Id, ToKeyString(key), converter.Convert(value));
                var task = publisher.Publish(setEvent);
                ProcessEvent(setEvent);
                task.Wait();
            }
        }

        internal void SetDirect(object key, object value) => dictionary[ToKeyString(key)] = value;

        public override bool Contains(object key) => dictionary.ContainsKey(ToKeyString(key));
        public override bool ContainsKey(object key) => dictionary.ContainsKey(ToKeyString(key));
        public override bool ContainsValue(object value)
        {
            foreach (KeyValuePair<string, object> entry in dictionary)
                if (Equals(entry.Value, value))
                    return true;

            return false;
        }
        public override IDictionaryEnumerator GetEnumerator() => new SharedHashtableEnumerator(dictionary);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override void CopyTo(Array array, int arrayIndex) => throw new NotSupportedException();
        public override object Clone() => throw new NotSupportedException();

        public override int Count => dictionary.Count;
        public override bool IsFixedSize => false;
        public override bool IsReadOnly => false;
        public override bool IsSynchronized => false;
        public override ICollection Keys => (ICollection)dictionary.Keys;
        public override ICollection Values => (ICollection)dictionary.Values;

        private string ToKeyString(object key)
        {
            if (!(key is string keyStr))
                keyStr = key.ToString();

            return keyStr;
        }

        internal void ProcessEvent(SharedHashtableUpdateEvent updateEvent)
        {
            if (updateEvent is SharedHashtableSetEvent setEvent)
                ProcessEvent(setEvent);
            else if (updateEvent is SharedHashtableRemovedEvent removeEvent)
                ProcessEvent(removeEvent);
            else if (updateEvent is SharedHashtableClearedEvent clearEvent)
                ProcessEvent(clearEvent);
        }

        internal void ProcessEvent(SharedHashtableSetEvent setEvent)
        {
            if (updateEvents.TryAdd(setEvent.Id, setEvent))
                if (CheckOrUpdateUpdateTime(setEvent.Timestamp))
                    dictionary.AddOrUpdate(setEvent.Key, setEvent.Value, (key, oldValue) => setEvent.Value);
                else
                    ReprocessEvents(setEvent.Timestamp);
        }

        internal void ProcessEvent(SharedHashtableRemovedEvent removeEvent)
        {
            if (updateEvents.TryAdd(removeEvent.Id, removeEvent))
                if (CheckOrUpdateUpdateTime(removeEvent.Timestamp))
                    dictionary.TryRemove(removeEvent.Key, out _);
                else
                    ReprocessEvents(removeEvent.Timestamp);
        }

        internal void ProcessEvent(SharedHashtableClearedEvent clearEvent)
        {
            if (updateEvents.TryAdd(clearEvent.Id, clearEvent))
                if (CheckOrUpdateUpdateTime(clearEvent.Timestamp))
                    dictionary.Clear();
                else
                    ReprocessEvents(clearEvent.Timestamp);
        }

        private void ReprocessEvents(DateTime startTime)
        {
            foreach (var udEvent in updateEvents.Values.Where(e => e.Timestamp >= startTime).OrderBy(e => e.Timestamp))
                if (udEvent is SharedHashtableSetEvent setEvent)
                    dictionary.AddOrUpdate(setEvent.Key, setEvent.Value, (key, oldValue) => setEvent.Value);
                else if (udEvent is SharedHashtableRemovedEvent removeEvent)
                    dictionary.TryRemove(removeEvent.Key, out _);
                else if (udEvent is SharedHashtableClearedEvent clearEvent)
                    dictionary.Clear();
        }


        public sealed class SharedHashtableEnumerator : IDictionaryEnumerator
        {
            internal SharedHashtableEnumerator(ConcurrentDictionary<string, object> dictionary) =>
                enumerator = dictionary.OrderBy(x => x.Key).GetEnumerator();

            readonly IEnumerator<KeyValuePair<string, object>> enumerator;

            public DictionaryEntry Entry => new DictionaryEntry(Key, Value);

            public object Key => enumerator.Current.Key;
            public object Value => enumerator.Current.Value;
            public object Current => Entry;

            public bool MoveNext() => enumerator.MoveNext();
            public void Reset() => enumerator.Reset();
        }
    }
}