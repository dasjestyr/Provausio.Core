using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Provausio.Core.Parsing
{
    /// <summary>
    /// When implemented, provides a service for reading a single field out of a delimited text stream.
    /// </summary>
    public interface IFieldLexer
    {
        /// <summary>
        /// Get whether or not there are more fields on the current line.
        /// </summary>
        bool HasMoreFields { get; }

        /// <summary>
        /// Sets the characters that will act as markers between each field.
        /// </summary>
        /// <param name="delimiters"></param>
        void SetDelimiters(params char[] delimiters);

        /// <summary>
        /// Parses the next field. Returns true/false indicating whether or not there is more 
        /// </summary>
        /// <returns></returns>
        string GetNextField();
    }

    internal class DelimitedFieldLexer : IFieldLexer
    {
        private const int NoData = -1;
        private const int Delimiter = -2;
        private const int Initialize = -3;
        private const int EndOfLine = 0x0A;
        private const int WindowsEndOfLine = 0x0D;
        private const int Quote = 0x22;
        private const LexingState BreakCondition = LexingState.NoData | LexingState.EndOfLine | LexingState.StartingNextField;

        private readonly StringBuilder _buffer;
        private readonly TextReader _reader;

        private int[] _delimiters;
        private LexingState _state;

        public bool HasMoreFields => _state != LexingState.EndOfLine && _reader.Peek() != NoData;

        public DelimitedFieldLexer(TextReader reader)
        {
            _reader = reader;
            _buffer = new StringBuilder();
        }

        public DelimitedFieldLexer(TextReader reader, IEnumerable<char> delimiters)
            : this(reader)
        {
            SetDelimiters(delimiters.ToArray());
        }

        public void SetDelimiters(params char[] delimiters)
        {
            _delimiters = delimiters
                .Select(delimiter => (int) delimiter)
                .ToArray();
        }

        public string GetNextField()
        {
            if (_delimiters == null || _delimiters.Length == 0)
                throw new InvalidOperationException("No delimiters were set.");

            UpdateState(Initialize);
            
            while ((_state & BreakCondition) != _state)
            {
                var currentValue = _reader.Read();
                UpdateState(currentValue);

                switch (_state)
                {
                    case LexingState.WindowsEndOfLine:
                        // just skip because the next character is probably going to be \n
                        continue;
                    case LexingState.ReadingField:
                        _buffer.Append((char)currentValue);
                        break;
                    case LexingState.StartingNextField:
                    case LexingState.EndOfLine:
                    case LexingState.NoData:
                        break;
                    case LexingState.ReadingQuoted:
                        currentValue = _reader.Read();
                        while (currentValue != Quote)
                        {
                            _buffer.Append((char)currentValue);
                            currentValue = _reader.Read();
                        }
                        _state = LexingState.ReadingField;
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var field = _buffer.ToString();
            _buffer.Clear();

            return field;
        }

        private void UpdateState(int value)
        {
            // This switch is marginally faster than a dictionary and significantly faster than Hashtable

            value = _delimiters.Contains(value) ? Delimiter : value;
            
            switch (value)
            {
                case NoData:
                    _state = LexingState.NoData;
                    break;
                case Delimiter:
                    _state = LexingState.StartingNextField;
                    break;
                case EndOfLine:
                    _state = LexingState.EndOfLine;
                    break;
                case WindowsEndOfLine:
                    _state = LexingState.WindowsEndOfLine;
                    break;
                case Quote:
                    _state = LexingState.ReadingQuoted;
                    break;
                default:
                    _state = LexingState.ReadingField;
                    break;
            }
        }

        [Flags]
        private enum LexingState
        {
            ReadingField = 1,
            ReadingQuoted = 1 << 1,
            StartingNextField = 1 << 2,
            WindowsEndOfLine = 1 << 3,
            EndOfLine = 1 << 4,
            NoData = 1 << 5
        }
    }
}