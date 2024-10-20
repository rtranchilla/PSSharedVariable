using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace PSSharedVariables.PsModule;

[CmdletProvider("SharedVariable", ProviderCapabilities.ShouldProcess)]
public sealed class SharedVariableProvider : ContainerCmdletProvider, IContentCmdletProvider
{
    #region PSDrive
    protected override Collection<PSDriveInfo> InitializeDefaultDrives()
    {
        var drives = base.InitializeDefaultDrives();
        var scopeId = "TestPipe"; //SessionState.PSVariable.GetValue("ClientToolsScopeId");
        if (scopeId != null)
        {
            var info = new PSSharedVariableDriveInfo(
                "SVar",
                ProviderInfo,
                scopeId.ToString(),
                "Client Tools drive - session scoped", null,
                ActivatorUtilities.CreateInstance<SharedVariableRepository>(DependencyManager.ServiceProvider, scopeId));

            drives.Add(info);
        }
        return drives;
    }

    protected override PSDriveInfo? NewDrive(PSDriveInfo drive)
    {
        if (drive == null)
        {
            WriteError(new ErrorRecord(
                new ArgumentNullException(nameof(drive)),
                "NullDrive",
                ErrorCategory.InvalidArgument,
                null)
            );

            return null;
        }

        if (string.IsNullOrWhiteSpace(drive.Root))
        {
            WriteError(new ErrorRecord(
                new ArgumentException(nameof(drive.Root)),
                "NoRoot",
                ErrorCategory.InvalidArgument,
                drive)
            );

            return null;
        }

        return new PSSharedVariableDriveInfo(drive, ActivatorUtilities.CreateInstance<SharedVariableRepository>(DependencyManager.ServiceProvider, drive.Root));
    }
    protected override PSDriveInfo? RemoveDrive(PSDriveInfo drive)
    {
        if (drive == null)
        {
            WriteError(new ErrorRecord(
                new ArgumentNullException(nameof(drive)),
                "NullDrive",
                ErrorCategory.InvalidArgument,
                null)
            );

            return null;
        }

        return base.RemoveDrive(drive);
    }
    #endregion

    #region ContainerCmdletProvider
    protected override void GetItem(string path) => WriteItemObject(Get(path), path, false);
    protected override void SetItem(string path, object? value) => Set(path, value);
    protected override void NewItem(string path, string itemTypeName, object newItemValue) => Set(path, newItemValue);
    protected override void ClearItem(string path) => Clear(path);

    protected override bool IsValidPath(string path) => !string.IsNullOrWhiteSpace(path);
    protected override bool ItemExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return true;

        return Check(path);
    }
    protected override bool HasChildItems(string path) => false;

    protected override void GetChildItems(string path, bool recurse)
    {
        if (string.IsNullOrEmpty(path))
            foreach (var variable in Get().OrderBy(i => i.Name))
                WriteItemObject(variable, variable.Name, false);
        else
            WriteItemObject(new SharedVariable(path, Get(path)), path, false);
    }
    protected override void GetChildNames(string path, ReturnContainers returnContainers)
    {
        if (string.IsNullOrEmpty(path))
            foreach (var variable in Get().OrderBy(i => i.Name))
                WriteItemObject(variable.Name, variable.Name, false);
        else
            WriteItemObject(path, path, false);
    }
    #endregion

    #region IContentCmdletProvider
    public IContentReader? GetContentReader(string path) => new SharedVariableProviderContentReaderWriter(path, this);
    public IContentWriter? GetContentWriter(string path) => new SharedVariableProviderContentReaderWriter(path, this);

    public object? GetContentWriterDynamicParameters(string path) => null;
    public object? GetContentReaderDynamicParameters(string path) => null;
    public object? ClearContentDynamicParameters(string path) => null;

    public void ClearContent(string path) => Clear(path);
    #endregion

    #region SharedVariableRepository
    internal void Clear(string path)
    {
        if (PSDriveInfo is PSSharedVariableDriveInfo driveInfo)
            driveInfo.VariableRepository.Remove(path);
    }

    internal SharedVariable[] Get()
    {
        if (PSDriveInfo is PSSharedVariableDriveInfo driveInfo)
            return driveInfo.VariableRepository.Get();

        return [];
    }

    internal object? Get(string path)
    {
        if (PSDriveInfo is PSSharedVariableDriveInfo driveInfo)
            return driveInfo.VariableRepository.Get(path);

        return null;
    }

    internal void Set(string path, object? value)
    {
        if (PSDriveInfo is PSSharedVariableDriveInfo driveInfo)
            driveInfo.VariableRepository.Set(path, value);
    }

    internal bool Check(string path)
    {
        if (PSDriveInfo is PSSharedVariableDriveInfo driveInfo)
            return driveInfo.VariableRepository.Contains(path);
        return false;
    }

    //try
    //{
    //}
    //catch (TimeoutException ex)
    //{
    //    WriteError(new ErrorRecord(
    //        ex,
    //        "Timeout",
    //        ErrorCategory.ConnectionError,
    //        null)
    //    );
    //}
    //catch (Exception ex)
    //{
    //    WriteError(new ErrorRecord(
    //        ex,
    //        "Unknown",
    //        ErrorCategory.InvalidResult,
    //        null)
    //    );
    //}
    #endregion
}
