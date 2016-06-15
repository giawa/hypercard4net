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
                var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll", "System.Drawing.dll", "HyperCard.dll" }, uniqueName, true);
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

        /// <summary>
        /// Tries to invoke a method with the required name.  Returns true
        /// if the method was found and run successfully.  Returns false
        /// if the message should be escalated thru the message passing stack.
        /// </summary>
        /// <param name="type">The class that contains the method to call.</param>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="sender">The object that is calling this method.
        /// This is a button for a button script, card for card script, etc.</param>
        /// <returns>True if the method was run successfully, false if the message should be escalated.</returns>
        public static bool InvokeCompiledMethod(Type type, string methodName, object sender)
        {
            if (type == null) return false;

            MethodInfo method = type.GetMethod(methodName);
            if (method == null) return false;

            method.Invoke(null, new object[] { sender });

            return true;
        }
    }
}
