using System.Configuration;
using System.IO.Pipes;
using System.Management.Automation;
using System.Threading;

namespace PSSharedVariables.PsModule;

[Cmdlet(VerbsDiagnostic.Test, "PipeConnection")]
public sealed class TestConnection : PSCmdlet
{
    protected override void ProcessRecord()
    {
        var client = new NamedPipeClientStream("TestPipe");
        client.Connect(100);
        using var reader = new StreamReader(client);
        using var writer = new StreamWriter(client)
        {
            AutoFlush = true
        };

        writer.Write("Test");
        base.ProcessRecord();
    }
}
