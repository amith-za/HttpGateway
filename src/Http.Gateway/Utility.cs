using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Gateway
{
    public class Utility
    {
        public static string ReadScript(string scriptName)
        {
            return File.ReadAllText($"{System.AppDomain.CurrentDomain.BaseDirectory}\\scripts\\{scriptName}");
        }
    }
}
