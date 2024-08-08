using DenisKudelin.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DenisKudelin.Test
{
    internal class Program
    {
        public static void Main()
        {
            var cfg = new ConfigInitializer(typeof(CFG), autoSaveInterval: 1000);
            Thread.Sleep(5000);
            CFG.Value1 = 10;
            CFG.Value2 = new Dictionary<string, string>() { { "1", "2" } };
            CFG.Value3 = "test123";
            Thread.Sleep(5000);
        }
    }

    public static class CFG
    {
        public static int Value1 { get; set; }
        public static Dictionary<string, string> Value2 { get; set; }
        public static string Value3 { get; set; }
    }
}
