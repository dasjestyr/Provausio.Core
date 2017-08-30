using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Provausio.Core.Parsing.Csv.Mappers;

namespace Provausio.Core.Parsing.Csv
{
    public class DelimitedStringReader<T> : ParsedObjectReader<T>
        where T : class, new()
    {
        private readonly ITextFieldParser _fieldParser;
        private IStringArrayMapper<T> _mapper;
        private string[] _columnHeaders;
        
        /// <summary>
        /// Gets the base stream.
        /// </summary>
        /// <value>
        /// The base stream.
        /// </value>
        public Stream BaseStream => _fieldParser.BaseStream;
        
        /// <summary>
        /// Specifies whether or not the first row in the source file contains column names.
        /// </summary>
        public bool FirstRowHeaders { get; set; }

        public DelimitedStringReader(
            ITextFieldParser fieldParser,
            IStringArrayMapper<T> mapper,
            bool hasQuotedFields,
            bool firstLineIsHeader,
            params string[] delimiters)
        {
            if(delimiters == null)
                throw new ArgumentException("Must specify at least one delimiter");

            FirstRowHeaders = firstLineIsHeader;

            fieldParser.SetDelimiters(delimiters);
            fieldParser.HasQuotedFields = hasQuotedFields;
            _fieldParser = fieldParser ?? throw new ArgumentNullException(nameof(fieldParser));

            _mapper = mapper;
            if(_mapper != null)
                _mapper.AttachedReader = this;
            
        }
        
        public DelimitedStringReader(
            ITextFieldParser fieldParser,
            bool hasQuotedFields,
            bool firstLineIsHeader,
            params string[] delimiters)
                : this(fieldParser, null, hasQuotedFields, firstLineIsHeader, delimiters)
        {
        }

        /// <summary>
        /// Reads all lines in a single run.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<T>> ReadAll()
        {
            var result = await Task.Run(() =>
            {
                var items = new List<T>();
                while (ReadNext())
                {
                    items.Add(CurrentLine);
                }
                return items;
            });

            return result;
        }

        /// <summary>
        /// Reads the next line. The result will be placed in the CurrentLine property. Returns false if there are no more values that can be read.
        /// </summary>
        /// <returns></returns>
        public override bool ReadNext()
        {
            LineNumber++;

            try
            {
                if (FirstRowHeaders && _columnHeaders == null)
                {
                    _columnHeaders = _fieldParser.ReadFields().ToArray();
                    TrySetHeaderPositions(_columnHeaders);
                }

                CurrentLine = null;

                if (_fieldParser.EndOfData)
                    return false;

                RawFields = _fieldParser.ReadFields().ToArray();

                if(_mapper != null)
                    CurrentLine = MapLine(RawFields);
            }
            catch (Exception ex)
            {
                var error = new ParseError
                {
                    LineNumber = LineNumber,
                    Exception = ex,
                    Message = ex.Message
                };
                AddError(error);

                if (BreakOnError)
                    throw;

                return true;
            }

            return _mapper == null || CurrentLine != null;
        }

        private void TrySetHeaderPositions(string[] headers)
        {
            // handles mappers that care about headers
            var headerMapper = _mapper as IHeaderMapper;
            headerMapper?.SetHeaderPositions(headers);
        }

        /// <summary>
        /// Reads the delimited string and attempts to map it using the previously set mapped.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        protected virtual T MapLine(string[] fields)
        {
            var target = new T();
            var obj = _mapper.Map(fields, target);
            return obj;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _fieldParser.Dispose();
        }

        internal override void SetMapper(IStringArrayMapper<T> mapper)
        {
            _mapper = mapper;
            _mapper.AttachedReader = this;
        }
    }
}
