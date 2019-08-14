using System;

namespace Micromesh
{
    /// <summary>
    /// A custom exception class so it's easier to identify the exceptions
    /// that we throw ourselves.
    /// </summary>
    public class MeshException : Exception 
    {
        public MeshException(string message)
            : base(message)
        {
        }

        public MeshException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}