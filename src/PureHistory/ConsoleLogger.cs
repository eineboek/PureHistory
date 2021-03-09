using System;
using System.IO;
using System.Text;

namespace PureHistory
{
    internal class ConsoleLogger : TextWriter, IDisposable
    {
        private TextWriter stdOutWriter;
        public TextWriter Log { get; private set; }
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public ConsoleLogger()
        {
            Console.OutputEncoding = Encoding;
            stdOutWriter = Console.Out;
            Console.SetOut(stdOutWriter);
            Log = new StringWriter();
        }

        override public void WriteLine(string output)
        {
            Log.WriteLine(output);
            stdOutWriter.WriteLine(output);
        }

        public void Reset() => Log = new StringWriter();

        public string GetLog() => Log.ToString();
    }
}