using System.Management.Automation;

namespace PSSharedVariables.PsModule;

public sealed class PsObjectHandler : IPsObjectHandler
{
    public object Extract(object obj)
    {
        if (obj is PSObject pso)
            obj = pso.BaseObject;

        return obj;
    }

    public object Encapsulate(object obj)
    {
        if (obj is not PSObject)
            obj = new PSObject(obj);

        return obj;
    }

    public IEnumerable<PsPropertyValue> ExtractPropterties(object obj)
    {
        if (obj is PSObject pso)
            foreach (PSPropertyInfo prop in pso.Properties)
            {
                object? value = null;
                try { value = prop.Value; } catch { }
                yield return new PsPropertyValue(prop.Name, value);
            }
    }
}
