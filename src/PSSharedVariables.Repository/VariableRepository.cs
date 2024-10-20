using System.Collections.Concurrent;
using System.Data;

namespace PSSharedVariables.Repository;

public sealed class VariableRepository
{
    private readonly ConcurrentDictionary<string, string?> scopeVariable = new(StringComparer.InvariantCultureIgnoreCase);

    public List<(string Key, string? Value)> Get() => scopeVariable.Select(kvp => (kvp.Key, kvp.Value)).ToList();
    public string? Get(string name) => scopeVariable.TryGetValue(name, out var value) ? value : null;
    public void Set(string name, string? value) => scopeVariable[name] = value;
    public void Remove(string name) => scopeVariable.TryRemove(name, out _);
    public bool Exists(string name) => scopeVariable.ContainsKey(name);
}

