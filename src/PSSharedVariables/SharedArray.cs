using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PSSharedVariables
{
    public sealed class SharedArray : IList
    {
        private readonly IObjectConverter converter;
        private readonly SharedArrayList items;
        private readonly object syncRoot;

        public SharedArray(IPublisher publisher, IObjectConverter converter, object[] initalValue) : base()
        {
            items = new SharedArrayList(publisher, this, initalValue);
            syncRoot = items.SyncRoot;
            this.converter = converter;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public DateTime LastUpdateTime => items.LastUpdateTime;

        public object this[int index]
        {
            get
            {
                lock (syncRoot)
                    return items[index];
            }
            set
            {
                var convertValue = converter.Convert(value);
                lock (syncRoot)
                    items[index] = convertValue;
            }
        }

        public bool IsFixedSize => items.IsFixedSize;
        public bool IsReadOnly => items.IsReadOnly;
        public bool IsSynchronized => true;
        public object SyncRoot => syncRoot;

        public int Count
        {
            get
            {
                lock (syncRoot)
                    return items.Count;
            }
        }

        public int Add(object value)
        {
            var convertValue = converter.Convert(value);
            lock (syncRoot)
                return items.Add(convertValue);
        }

        public void Clear()
        {
            lock (syncRoot)
                items.Clear();
        }

        public bool Contains(object item)
        {
            lock (syncRoot)
                return items.Contains(item);
        }

        public void CopyTo(Array array, int index)
        {
            lock (syncRoot)
                items.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            lock (syncRoot)
                return items.ToArray()
                            .GetEnumerator();
        }

        public IEnumerator GetEnumerator(int index, int count)
        {
            lock (syncRoot)
                return items.GetRange(index, count)
                            .ToArray()
                            .GetEnumerator();
        }

        public int IndexOf(object value)
        {
            lock (syncRoot)
                return items.IndexOf(value);
        }
        public void Insert(int index, object value)
        {
            var convertValue = converter.Convert(value);
            lock (syncRoot)
                items.Insert(index, convertValue);
        }

        public void Remove(object value)
        {
            lock (syncRoot)
                items.Remove(value);
        }

        public void RemoveAt(int index)
        {
            lock (syncRoot)
                items.RemoveAt(index);
        }

        internal void ProcessEvent(SharedArrayUpdateEvent updateEvent)
        {
            lock (syncRoot)
                items.ProcessEvent(updateEvent);
        }

        private class SharedArrayList : ArrayList
        {
            private readonly ConcurrentDictionary<Guid, SharedArrayUpdateEvent> updateEvents = new ConcurrentDictionary<Guid, SharedArrayUpdateEvent>();
            private readonly IPublisher publisher;
            private readonly SharedArray array;

            public SharedArrayList(IPublisher publisher, SharedArray array, object[] initalValue) : base(initalValue)
            {
                this.publisher = publisher;
                this.array = array;
            }

            public DateTime LastUpdateTime { get; private set; }
            private bool CheckOrUpdateUpdateTime(DateTime eventTime)
            {
                if (eventTime > LastUpdateTime)
                {
                    LastUpdateTime = eventTime;
                    return true;
                }

                return false;
            }

            public override object this[int index]
            {
                get => base[index];
                set
                {
                    var setEvent = new SharedArraySetEvent(array.Id, index, value);
                    var task = publisher.Publish(setEvent);
                    ProcessEvent(setEvent);
                    task.Wait();
                }
            }

            public override void Insert(int index, object value)
            {
                var setEvent = new SharedArraySetEvent(array.Id, index, value);
                var task = publisher.Publish(setEvent);
                ProcessEvent(setEvent);
                task.Wait();
            }

            public override int Add(object value)
            {
                var index = base.Count;
                var setEvent = new SharedArraySetEvent(array.Id, index, value);
                var task = publisher.Publish(setEvent);
                ProcessEvent(setEvent);
                task.Wait();
                return index;
            }

            public override void RemoveAt(int index)
            {
                var removeEvent = new SharedArrayRemovedEvent(array.Id, index);
                var task = publisher.Publish(removeEvent);
                ProcessEvent(removeEvent);
                task.Wait();
            }

            public override void Clear()
            {
                var clearEvent = new SharedArrayClearedEvent(array.Id);
                var task = publisher.Publish(clearEvent);
                ProcessEvent(clearEvent);
                task.Wait();
            }

            internal void ProcessEvent(SharedArrayUpdateEvent updateEvent)
            {
                if (updateEvent is SharedArraySetEvent setEvent)
                    ProcessEvent(setEvent);
                else if (updateEvent is SharedArrayRemovedEvent removeEvent)
                    ProcessEvent(removeEvent);
                else if (updateEvent is SharedArrayClearedEvent clearEvent)
                    ProcessEvent(clearEvent);
                else if (updateEvent is SharedArrayAddEvent addEvent)
                    ProcessEvent(addEvent);
            }

            internal void ProcessEvent(SharedArraySetEvent setEvent)
            {
                if (updateEvents.TryAdd(setEvent.Id, setEvent))
                    if (CheckOrUpdateUpdateTime(setEvent.Timestamp))
                        base[setEvent.Index] = setEvent.Item;
                    else
                        ReprocessEvents(setEvent.Timestamp);
            }

            internal void ProcessEvent(SharedArrayRemovedEvent removeEvent)
            {
                if (updateEvents.TryAdd(removeEvent.Id, removeEvent))
                    if (CheckOrUpdateUpdateTime(removeEvent.Timestamp))
                        base.RemoveAt(removeEvent.Index);
                    else
                        ReprocessEvents(removeEvent.Timestamp);
            }

            internal void ProcessEvent(SharedArrayClearedEvent clearEvent)
            {
                if (updateEvents.TryAdd(clearEvent.Id, clearEvent))
                    if (CheckOrUpdateUpdateTime(clearEvent.Timestamp))
                        base.Clear();
                    else
                        ReprocessEvents(clearEvent.Timestamp);
            }

            internal void ProcessEvent(SharedArrayAddEvent addEvent)
            {
                if (updateEvents.TryAdd(addEvent.Id, addEvent))
                    if (CheckOrUpdateUpdateTime(addEvent.Timestamp))
                        base.Clear();
                    else
                        ReprocessEvents(addEvent.Timestamp);
            }

            private void ReprocessEvents(DateTime startTime)
            {
                foreach (var udEvent in updateEvents.Values.Where(e => e.Timestamp >= startTime).OrderBy(e => e.Timestamp))
                    if (udEvent is SharedArraySetEvent setEvent)
                        base[setEvent.Index] = setEvent.Item;
                    else if (udEvent is SharedArrayRemovedEvent removeEvent)
                        base.RemoveAt(removeEvent.Index);
                    else if (udEvent is SharedArrayClearedEvent clearEvent)
                        base.Clear();
                    else if (udEvent is SharedArrayAddEvent addEvent)
                    {
                        if (addEvent.Index == base.Count)
                            base.Add(addEvent.Item);
                        else if (addEvent.Index > base.Count)
                        {
                            while (addEvent.Index > base.Count)
                                base.Add(null);
                            base.Insert(addEvent.Index, addEvent.Item);
                        }
                        else 
                        {
                            if (base[addEvent.Index] == null)
                                base.Insert(addEvent.Index, addEvent.Item);
                            else
                                // TODO: Implement add conflict resolution
                                throw new NotImplementedException("Add conflicts do not yet have a resolution.");
                        }
                    }
            }
        }
    }
}
