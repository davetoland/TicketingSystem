using System;

namespace Aareon.Business.Exceptions
{
    public class InvalidPersonException : Exception
    {
        public int PersonId { get; }

        public InvalidPersonException(int personId)
        {
            PersonId = personId;
        }
    }
}