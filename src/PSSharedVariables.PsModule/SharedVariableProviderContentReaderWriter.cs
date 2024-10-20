using System.Collections;
using System.Management.Automation.Provider;

namespace PSSharedVariables.PsModule;

public sealed class SharedVariableProviderContentReaderWriter(string path, SharedVariableProvider provider) : IContentReader, IContentWriter
{
    public IList Read(long readCount)
    {
        var result = provider.Get(path);

        if (result == null)
            return Array.Empty<object>();

        return new object[] { result };
    }

    public IList Write(IList content)
    {
        ArgumentNullException.ThrowIfNull(content);

        object? valueToSet = content;
        if (content.Count == 1)
            valueToSet = content[0];

        provider.Set(path, valueToSet);

        return content;
    }

    public void Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public void Dispose() { }
    public void Close() { }
}
