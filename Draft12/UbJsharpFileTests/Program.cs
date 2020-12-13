using System;
using System.Diagnostics;
using System.IO;
using UbJsharp;

namespace UbJsharpFileTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var val = new UbJsonDeserializer().Deserialize(File.ReadAllBytes("./Rsrc/slp_test.slp"));
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);
            Console.WriteLine($"{val.ToString()}");
        }
    }
}
