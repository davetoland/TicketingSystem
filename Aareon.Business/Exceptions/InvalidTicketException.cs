using System;

namespace Aareon.Business.Exceptions
{
    public class InvalidTicketException : Exception 
    {
        public int? TicketId { get; }

        public InvalidTicketException(int? ticketId)
        {
            TicketId = ticketId;
        }
    }
}