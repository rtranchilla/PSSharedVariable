using System;
using System.Collections.Generic;
using System.Text;

namespace PSSharedVariables
{
    public interface IPsObjectHandler
    {
        object Extract(object obj);
        object Encapsulate(object obj);
        IEnumerable<PsPropertyValue> ExtractPropterties(object obj);
    }

    public sealed class PsPropertyValue
    {
        public PsPropertyValue(string name, object value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }

        public string Name { get; }
        public object Value { get; }
    }
}
