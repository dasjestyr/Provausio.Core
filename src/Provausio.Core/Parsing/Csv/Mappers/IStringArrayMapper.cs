using System.Collections.Generic;

namespace Provausio.Core.Parsing.Csv.Mappers
{
    public interface IStringArrayMapper<T>
    {
        ParsedObjectReader<T> AttachedReader { get; set; }

        T Map(IReadOnlyList<string> source, T target);

        ParsedObjectReader<T> GetReader();
    }
}