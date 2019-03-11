using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FrameWork;
using Microsoft.CSharp;

namespace WorldServer.Managers
{
    public class CSharpScriptCompiler
    {
        private IEnumerable<string> GetScriptFiles()
        {
            return Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Scripts"), "*", SearchOption.AllDirectories).Where(f => f.ToLower().EndsWith(".cs"));
        }

        private CompilerResults CompileScript(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;

            var language = CodeDomProvider.GetLanguageFromExtension(Path.GetExtension(filePath));
            //var codeDomProvider = CodeDomProvider.CreateProvider(language);
            var codeDomProvider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });

            var compilerParams = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = false,
                CompilerOptions = "/optimize"
            };

            Log.Info("Scripting", "Compiling :" + filePath);
            foreach (AssemblyName an in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                compilerParams.ReferencedAssemblies.Add(an.Name + ".dll");
            compilerParams.ReferencedAssemblies.Add("WorldServer.exe");

            return codeDomProvider.CompileAssemblyFromFile(compilerParams, filePath);
        }

        public void LoadScripts()
        {
            //get all script files from file system
            var scriptFiles = GetScriptFiles();

            //how many files were successfully loaded
            var loaded = 0;

            //loop through each script file and attempt to compile
            foreach (var filePath in scriptFiles)
            {
                //compile the script to an in-memory assembly
                var result = CompileScript(filePath);

                //if there were errors compiling the script, report them to the logger
                if (!result.Errors.HasErrors)
                {
                    ++loaded;
                    continue;
                }
                var errors = new StringBuilder();
                var filename = Path.GetFileName(filePath);
                foreach (CompilerError err in result.Errors)
                {
                    errors.AppendLine(string.Format("{0}({1},{2}): {3}: {4}",
                        filename, err.Line, err.Column,
                        err.ErrorNumber, err.ErrorText));
                }
                var errorOutput = string.Format("Error loading script\r\n{0}", errors);
                
                Log.Error("Scripting", errorOutput);
            }

            Log.Info("Scripting", string.Format("C# Script Compiler Loaded {0} script{1}.", loaded, loaded != 1 ? "s" : ""));
        }
    }
}
