using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Provausio.Core.Parsing.Csv.Mappers;

namespace Provausio.Core.Parsing
{
    public abstract class ParsedObjectReader<T> : IDisposable
    {
        private readonly List<ParseError> _errors = new List<ParseError>();
        private bool _isDisposed;

        /// <summary>
        /// Last read line number
        /// </summary>
        public long LineNumber { get; protected set; }

        /// <summary>
        /// Gets or sets the last parsed line.
        /// </summary>
        /// <value>
        /// The current line.
        /// </value>
        public T CurrentLine { get; protected set; }

        /// <summary>
        /// The fields contained in the current line.
        /// </summary>
        public string[] RawFields { get; set; }

        /// <summary>
        /// Specifies whether or  not the reader should throw when it encounters an error. If set to false, it will continue, but collect a list of errors in the 'ParseErrors' property
        /// </summary>
        public bool BreakOnError { get; set; }

        /// <summary>
        /// A collection of errors that were encountered during the read
        /// </summary>
        public IEnumerable<ParseError> ParseErrors => _errors;

        /// <summary>
        /// Reads the next line.
        /// </summary>
        /// <returns></returns>
        public abstract bool ReadNext();

        /// <summary>
        /// Retrieves the value in the current line at the specified index. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string GetAtIndex(int index)
        {
            if(RawFields == null)
                throw new InvalidOperationException("Nothing to read. Did you forget to call ReadNext()?");

            if(RawFields.Length - 1 < index)
                throw new IndexOutOfRangeException($"Attempted to retrieve value at index {index} but the array ranges from 0 to {RawFields.Length - 1}");

            return RawFields[index];
        }

        /// <summary>
        /// Retrieves the value in the current line at the specified index. Returns true if validation passes, otherwise false.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="validationPattern"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool TryGetAtIndex(int index, string validationPattern, out string value)
        {
            if (RawFields == null)
                throw new InvalidOperationException("Nothing to read. Did you forget to call ReadNext()?");

            if (RawFields.Length - 1 < index)
                throw new IndexOutOfRangeException($"Attempted to retrieve value at index {index} but the array ranges from 0 to {RawFields.Length - 1}.");

            value = RawFields[index];
            return Regex.IsMatch(value, validationPattern);
        }

        protected void AddError(ParseError error)
        {
            _errors.Add(error);
        }

        public void ClearErrors()
        {
            _errors.Clear();
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            Dispose(true);
            _isDisposed = true;

            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        internal abstract void SetMapper(IStringArrayMapper<T> mapper);

        public TMapperType UseMapper<TMapperType>()
            where TMapperType : IStringArrayMapper<T>, new()
        {
            var mapper = (TMapperType)Activator.CreateInstance(typeof(TMapperType));
            SetMapper(mapper);
            return mapper;
        }
    }

    public class ParseError
    {
        /// <summary>
        /// The approximate line number that was read which caused the error
        /// </summary>
        public long LineNumber { get; set; }

        /// <summary>
        /// Message of the error
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The stack trace, if any.
        /// </summary>
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return $"Line {LineNumber}: {Message}";
        }
    }
}
