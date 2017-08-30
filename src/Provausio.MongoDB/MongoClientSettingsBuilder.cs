using System;
using System.Net.Sockets;
using MongoDB.Driver;

namespace Provausio.MongoDB
{
    /// <summary>
    /// Helper class for setting up MongoDB clients using some values that we determined through testing.
    /// </summary>
    public class MongoClientSettingsBuilder
    {
        private readonly MongoClientSettings _settings;

        public MongoClientSettingsBuilder()
        {
            _settings = new MongoClientSettings
            {
                // our default settings
                ConnectTimeout = TimeSpan.FromSeconds(60),
                SocketTimeout = TimeSpan.FromSeconds(60),
                MaxConnectionIdleTime = TimeSpan.FromSeconds(60)
            };

            // enable keep-alives
            void SocketConfigurator(Socket s) => s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _settings.ClusterConfigurator = builder =>
                builder.ConfigureTcp(tcp => tcp.With(socketConfigurator: (Action<Socket>)SocketConfigurator));
        }

        /// <summary>
        /// Parses the connection string. Only 'ssl' and 'replicaSet' options are set. To set other options explicitly, use Apply()
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public MongoClientSettingsBuilder FromConnectionString(string connectionString)
        {
            var values = new MongoConnectionStringValues(connectionString);
            _settings.Servers = values.Servers;
            _settings.Credentials = new[] {values.Credentials};
            _settings.UseSsl = Convert.ToBoolean(values.Options["ssl"] ?? "false");
            _settings.ReplicaSetName = values.Options["replicaSet"];

            return this;
        }

        /// <summary>
        /// Generic action to set values directly.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public MongoClientSettingsBuilder Apply(Action<MongoClientSettings> configuration)
        {
            configuration(_settings);
            return this;
        }

        /// <summary>
        /// Returns the resultant <see cref="MongoClientSettings"/>
        /// </summary>
        /// <returns></returns>
        public MongoClientSettings Build()
        {
            return _settings;
        }
    }
}
