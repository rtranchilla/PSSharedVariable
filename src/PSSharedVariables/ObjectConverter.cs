using MediatR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PSSharedVariables
{
    public sealed class ObjectConverter : IObjectConverter
    {
        public ObjectConverter(IPublisher publisher, IPsObjectHandler psObjectHandler)
        {
            this.publisher = publisher;
            this.psObjectHandler = psObjectHandler;
        }

        private int maxDepth = 10;
        private readonly IPublisher publisher;
        private readonly IPsObjectHandler psObjectHandler;

        public object Convert(object value) => ConvertObject(value);

        //public object Convert(SharedHashtable parentHashtable, object key, object value) => ConvertObject(value);

        private object ConvertObject(object value, int depth = 0)
        {
            if (value == null || value == DBNull.Value)
                return null;

            var originalValue = value;
            PsPropertyValue[] additionalProps = Array.Empty<PsPropertyValue>();
            value = psObjectHandler.Extract(value);

            if (value is string || value is char || value is bool || value is DateTime || value is DateTimeOffset
                || value is Guid || value is Uri || value is double || value is float || value is decimal || value is BigInteger)
                return FinalizeObject(value);

            additionalProps = psObjectHandler.ExtractPropterties(originalValue).ToArray();

            Type type = value.GetType();

            if (type.IsPrimitive || type.IsEnum)
                return FinalizeObject(value);

            if (depth <= maxDepth)
            {
                if (value is IDictionary dict)
                {
                    var dictResult = new SharedHashtable(publisher, this);
                    foreach (DictionaryEntry kvp in dict)
                    {
                        string key;
                        if (kvp.Key is string keyStr)
                            key = keyStr;
                        else
                            key = kvp.Key.ToString();

                        if (!dictResult.ContainsKey(key))
                            dictResult.SetDirect(key, ConvertObject(kvp.Value, depth + 1));
                    }

                    return FinalizeHashtable(dictResult);
                }

                if (value is IEnumerable enumerable)
                {
                    List<object> arrayResult = new List<object>();
                    foreach (object item in enumerable)
                        arrayResult.Add(ConvertObject(item, depth + 1));
                    return FinalizeObject(new SharedArray(publisher, this, arrayResult.ToArray()));
                }

                var result = new SharedHashtable(publisher, this);
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    result.SetDirect(field.Name, ConvertObject(field.GetValue(value), depth + 1));

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo property in properties)
                    result.SetDirect(property.Name, ConvertObject(property.GetValue(value, null), depth + 1));

                return FinalizeHashtable(result);
            }

            return null;

            object FinalizeObject(object obj)
            {
                if (additionalProps.Length > 0)
                {
                    var result = FinalizeHashtable(new SharedHashtable(publisher, this));
                    result.SetDirect("value", obj);
                    return result;
                }
                return psObjectHandler.Encapsulate(obj);
            }

            SharedHashtable FinalizeHashtable(SharedHashtable ht)
            {
                foreach (PsPropertyValue property in additionalProps)
                    ht.SetDirect(property.Name, ConvertObject(property.Value, depth + 1));

                return ht;
            }
        }
    }

    public interface IObjectConverter
    {
        object Convert(object value);
        //object Convert(SharedHashtable parentHashtable, object key, object value);
    }
}