using System.Collections.Generic;
using Provausio.Core.Parsing.Csv.Mappers;

namespace Provausio.Core.Parsing.Csv
{
    public class CsvMapperFactory<T>
    {
        private readonly Dictionary<string, IStringArrayMapper<T>> _mappers;

        public CsvMapperFactory()
        {
            _mappers = new Dictionary<string, IStringArrayMapper<T>>
            {
                {"header", new HeaderMapper<T>()},
                {"custom", new CustomMapper<T>()},
                {"attribute", new ArrayPropertyMapper<T>()}
            };
        }

        public IStringArrayMapper<T> GetMapper(string key, object args = null)
        {
            key = key.ToLower();
            var mapper = _mappers[key];

            return mapper;
        }

        
    }
}