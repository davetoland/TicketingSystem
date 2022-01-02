using System;

namespace Aareon.Data.Exceptions
{
    public class RepositoryException : Exception
    {
        public RepositoryException(string message) : base (message) { }
    }
}