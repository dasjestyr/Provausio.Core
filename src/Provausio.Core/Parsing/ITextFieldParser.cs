using System;
using System.Collections.Generic;
using System.IO;

namespace Provausio.Core.Parsing
{
    public interface ITextFieldParser : IDisposable
    {
        Stream BaseStream { get; }

        /// <summary>
        /// Gets a value indicating whether [end of data].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [end of data]; otherwise, <c>false</c>.
        /// </value>
        bool EndOfData { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has quoted fields.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has quoted fields; otherwise, <c>false</c>.
        /// </value>
        bool HasQuotedFields { get; set; }

        /// <summary>
        /// Sets the delimiters.
        /// </summary>
        /// <param name="delimiters">The delimiters.</param>
        void SetDelimiters(params string[] delimiters);

        /// <summary>
        /// Reads the fields.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> ReadFields();
    }
}
