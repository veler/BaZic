// CSharp code generated automatically

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaZic.StandaloneRuntime;

[CSharpCodeGenerator_AssemblyInformation]
[assembly: System.Resources.NeutralResourcesLanguage("en")]

namespace BaZic.StandaloneRuntime
{
    public static class EntryProgram
    {
        /// <summary>
        /// Entry point of the entire application.
        /// </summary>
        /// <param name="args"></param>
        [STAThread()]
        public static void Main(string[] args)
        {
            var instance = new Program();
            instance.Main(args);
        }
    }

    [Serializable]
    public class Program : IBaZicProgram
    {
        private ProgramHelper _programHelperInstance;

        public ProgramHelper ProgramHelperInstance => _programHelperInstance;

        public Program()
        {
            var CSharpCodeGenerator_xamlCode = string.Empty;
            _programHelperInstance = new ProgramHelper(CSharpCodeGenerator_xamlCode);
        }

[CSharpCodeGenerator_GlobalVariablesString]
[CSharpCodeGenerator_BindingsString]
[CSharpCodeGenerator_MethodsString]
    }
}
