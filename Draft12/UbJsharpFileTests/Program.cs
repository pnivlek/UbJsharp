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
            var file = File.Open("./Rsrc/slp_test.slp", FileMode.Open);
            st.Start();
            var val = new UbJsonReaderDeserializer().Deserialize(file);
            st.Stop();
            file.Close();
            Console.WriteLine(st.ElapsedMilliseconds);
            byte[] allBytes = File.ReadAllBytes("./Rsrc/slp_test.slp");
            st.Restart();
            val = new UbJsonArrayDeserializer().Deserialize(allBytes);
            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);
            Console.WriteLine($"{val.ToString()}");
        }
    }
}
