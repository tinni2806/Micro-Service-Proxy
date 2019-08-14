namespace Micromesh
{
    /// <summary>
    /// A collection of headers that Micromesh relies on.
    /// </summary>
    public struct Headers
    {
        public static readonly string ContentType = "Content-Type";
        public static readonly string ForwardTo = "Forward-To";
        public static readonly string Host = "Host";
        public static readonly string MicromeshVersion = "Micromesh-Version";
        public static readonly string XForwardedFor = "X-Forwarded-For";
        public static readonly string ContextIdentifier = "Context-Identifier";
    } 

    /// <summary>
    /// A collection of named http clients used by Micromesh.
    /// </summary>
    public struct HttpClients
    {
        public static readonly string ResilientClient = "ResilientClient";
    }

    public struct RequestPropertyKeys
    {
        public static readonly string RetryAttempt = "RetryAttempt";
    }
}
