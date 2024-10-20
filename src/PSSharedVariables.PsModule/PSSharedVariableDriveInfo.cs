using System.Management.Automation;

namespace PSSharedVariables.PsModule;

public class PSSharedVariableDriveInfo : PSDriveInfo
{
    public PSSharedVariableDriveInfo(PSDriveInfo driveInfo, SharedVariableRepository variableRepository) : base(driveInfo) => 
        VariableRepository = variableRepository ?? throw new ArgumentNullException(nameof(variableRepository));

    public PSSharedVariableDriveInfo(string name, ProviderInfo provider, string root, string description, PSCredential credential, SharedVariableRepository variableRepository) : base(name, provider, root, description, credential) => 
        VariableRepository = variableRepository ?? throw new ArgumentNullException(nameof(variableRepository));

    internal SharedVariableRepository VariableRepository { get; }
}
