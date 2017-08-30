using System;

namespace Provausio.Core.Parsing
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArrayPropertyAttribute : Attribute
    {

        /// <summary>
        /// Gets or sets the index from which the property will obtain its value in the source array.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the regex pattern that will be used for validation.
        /// </summary>
        public string ValidationPattern { get; set; }
    }
}
