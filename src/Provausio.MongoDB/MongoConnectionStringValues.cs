using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MongoDB.Driver;

namespace Provausio.MongoDB
{
    /// <summary>
    /// Helper class makes up for the fact that MongoClientSettings isn't quite in alignment with the connection string format.
    /// </summary>
    public class MongoConnectionStringValues
    {
        private const string CredentialsPattern = @"(?<=//)(.+?)(?=@)";
        private const string ServersPattern = @"(?<=@)(.+?)(?=\?)";
        private const string OptionsPattern = @"(?<=\?).*";
        private const string PortPattern = @":\d{2,5}";
        private const string DatabasePattern = @"/\w.+";

        /// <summary>
        /// Credentials extracted from the connection string.
        /// </summary>
        public MongoCredential Credentials { get; }

        /// <summary>
        /// A list of servers extracted from the connection string.
        /// </summary>
        public IEnumerable<MongoServerAddress> Servers { get; }

        /// <summary>
        /// A collection of options parsed from the query string of the connection string.
        /// </summary>
        public IDictionary<string, string> Options { get; }

        /// <summary>
        /// Currently supports the following format <code>mongodb://username:password@server,server,server?option1=value&amp;option2=value</code> 
        /// </summary>
        /// <param name="connectionString"></param>
        public MongoConnectionStringValues(string connectionString)
        {
             var optionsMatch = Regex.Match(connectionString, OptionsPattern);
            if(optionsMatch.Success)
                Options = Provausio.Core.ObjectPropertyCollection.FromKvpString(optionsMatch.Value).Properties;
            
            var credsMatch = Regex.Match(connectionString, CredentialsPattern);
            if (credsMatch.Success)
            {
                if (!Options.ContainsKey("authSource"))
                    throw new ArgumentException("An 'authSource' needs to be specified in the connection string in order to use credentials.");

                var credsParts = credsMatch.Value.Split(new[] {':'}, StringSplitOptions.None);
                Credentials = MongoCredential.CreateCredential(Options["authSource"], credsParts[0], credsParts[1]);
            }

            var serversMatch = Regex.Match(connectionString, ServersPattern);
            if(!serversMatch.Success)
                throw new ArgumentException("No servers were present in the connection string!");

            var serverParts = serversMatch.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            var servers = new List<MongoServerAddress>();
            foreach (var part in serverParts)
            {
                var cleaned = Regex.Replace(part, PortPattern, string.Empty);
                cleaned = Regex.Replace(cleaned, DatabasePattern, string.Empty);
                servers.Add(new MongoServerAddress(cleaned));
            }

            Servers = servers;
        }
    }
}
