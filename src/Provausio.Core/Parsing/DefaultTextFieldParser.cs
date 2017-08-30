using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Provausio.Core.Parsing
{
    public class DefaultTextFieldParser : ITextFieldParser
    {
        private readonly StreamReader _reader;
        private readonly IFieldLexer _lexer;

        public Stream BaseStream => _reader.BaseStream;

        public bool EndOfData => _reader.EndOfStream;

        public bool HasQuotedFields { get; set; }

        public DefaultTextFieldParser(StreamReader reader)
            : this(reader, new DelimitedFieldLexer(reader))
        {
        }

        public DefaultTextFieldParser(StreamReader reader, IFieldLexer lexer)
        {
            _reader = reader;
            _lexer = lexer;
        }

        /// <summary>
        /// Sets the delimiters
        /// </summary>
        /// <param name="delimiters"></param>
        public void SetDelimiters(params string[] delimiters)
        {
            if(delimiters.Any(character => character.Length > 1))
                throw new ArgumentException("Delimiters can only be 1 character long.");

            _lexer.SetDelimiters(delimiters.Select(delimiter => delimiter[0]).ToArray());
        }

        /// <summary>
        /// Gets the fields from the next line in the stream
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReadFields()
        {
            if(EndOfData)
                throw new InvalidOperationException("Reached end of stream.");
            
            while (_lexer.HasMoreFields || !EndOfData)
            {
                var field = _lexer.GetNextField();
                yield return field;

                if (!_lexer.HasMoreFields) // this is to make up for the awkward loop condition
                    break;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _reader.Dispose();
        }
    }
}
