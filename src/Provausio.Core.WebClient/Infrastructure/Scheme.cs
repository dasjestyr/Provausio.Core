namespace Provausio.Core.WebClient.Infrastructure
{
    public enum Scheme
    {
        /// <summary>
        /// Default fault. The value was not explicitly set
        /// </summary>
        Unspecified,

        /// <summary>
        /// Hypertext Transfer Protocol
        /// </summary>
        Http,

        /// <summary>
        /// Hypertext Transfer Protocol Secure
        /// </summary>
        Https,

        /// <summary>
        /// File Transfer Protocol
        /// </summary>
        Ftp
    }
}