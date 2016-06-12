using System;
using System.Reflection;

using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace HyperCard.Scripting
{
    public class CSharpScripting
    {
        /// <summary>
        /// Get a compiled Type from a script.  This Type can contain multiple methods which will be
        /// called at runtime by their message handlers.
        /// </summary>
        /// <param name="script">The script to compile (1 script per part/card/background/etc).</param>
        /// <returns>The fully compiled type (if successful) or null if not successful.</returns>
        public static Module CompileScript(string script)
        {
            try
            {
                // TODO:  Load each script into a unique AppDomain and dispose of it when it is no longer in use.
                // Get rid of this unique name shenanigans.
                string uniqueName = Environment.TickCount + script.GetHashCode() + ".dll";

                var csc = new CSharpCodeProvider();
                var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "HyperCard.dll" }, uniqueName, true);
                parameters.GenerateInMemory = true;

                CompilerResults results = csc.CompileAssemblyFromSource(parameters, script);

                foreach (var error in results.Errors) Console.WriteLine(error);

                if (results.Errors.Count == 0)
                {
                    Module module = results.CompiledAssembly.GetModules()[0];
                    return module;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error compiling the script:");
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static bool InvokeCompiledMethod(Type type, string methodName, object sender)
        {
            if (type == null) return false;

            MethodInfo mouseDown = type.GetMethod(methodName);
            if (mouseDown == null) return false;

            mouseDown.Invoke(null, new object[] { sender });

            return true;
        }
    }
}
