using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TauCode.Extensions.Lab
{
    public class MultiTextWriterLab : TextWriter
    {
        private readonly List<TextWriter> _innerWriters;

        public MultiTextWriterLab(Encoding encoding, IEnumerable<TextWriter> innerWriters)
        {
            this.Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            if (innerWriters == null)
            {
                throw new ArgumentNullException(nameof(innerWriters));
            }

            _innerWriters = innerWriters.ToList();
            if (_innerWriters.Any(x => x == null))
            {
                throw new ArgumentException($"'{nameof(innerWriters)}' cannot contain nulls.");
            }
        }

        public override Encoding Encoding { get; }

        public IReadOnlyList<TextWriter> InnerWriters => _innerWriters;

        // todo: check not disposed; locks?
        public override void Write(char value)
        {
            foreach (var innerWriter in _innerWriters)
            {
                innerWriter.Write(value);
            }
        }

        // todo: check not disposed; locks?
        public override void Write(string s)
        {
            foreach (var innerWriter in _innerWriters)
            {
                innerWriter.Write(s);
            }
        }

        public override async Task WriteAsync(char value)
        {
            foreach (var innerWriter in _innerWriters)
            {
                await innerWriter.WriteAsync(value);
            }
        }

        public override async Task WriteAsync(string value)
        {
            foreach (var innerWriter in _innerWriters)
            {
                await innerWriter.WriteAsync(value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            // todo: checks, locks?
            _innerWriters.Clear();
        }
    }
}
