param([Parameter(Mandatory = $true)][string]$BuildRoot)
$ErrorActionPreference = 'Stop'

# Windows PowerShell does not use WorldServer.exe.config's binding redirects.
# Resolve dependencies from this build, as the compiled validation harnesses do.
# A C# callback is required: assembly resolution can run without a PowerShell runspace.
if (-not ('ProjectWarValidation.BuildAssemblyResolver' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.Reflection;

namespace ProjectWarValidation
{
    public sealed class BuildAssemblyResolver : IDisposable
    {
        private readonly string libraries;

        public BuildAssemblyResolver(string buildRoot)
        {
            libraries = Path.Combine(Path.GetFullPath(buildRoot), "libs");
            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        private Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;
            if (name != Path.GetFileName(name))
                return null;
            string path = Path.Combine(libraries, name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
        }
    }
}
'@
}

New-Object ProjectWarValidation.BuildAssemblyResolver -ArgumentList (Resolve-Path -LiteralPath $BuildRoot).Path
