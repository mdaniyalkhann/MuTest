using System;
using MuTest.Core.Common;
using MuTest.Core.Utility;
using static MuTest.Core.Common.Constants;

namespace MuTest.Core.Testing
{
    public class ChalkHtml : IChalk
    {
        public event EventHandler<string> OutputDataReceived;

        public void Red(string text)
        {
            Write(text, Colors.Red);
        }

        public void Green(string text)
        {
            Write(text, Colors.Green);
        }

        public void Cyan(string text)
        {
            Write(text, Colors.Blue);
        }

        public void Yellow(string text)
        {
            Write(text, Colors.BlueViolet);
        }

        public void DarkGray(string text)
        {
            Write(text, Colors.Brown);
        }

        public void Magenta(string text)
        {
            Write(text, Colors.Brown);
        }

        public void Default(string text)
        {
            Write(text, DefaultColor);
        }

        protected virtual void OnOutputDataReceived(string arg)
        {
            OutputDataReceived?.Invoke(this, arg);
        }

        private void Write(string text, string color)
        {
            OnOutputDataReceived(text.Encode().PrintWithPreTag(color: color));
        }
    }
}
