using System;

namespace ServiceTests
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
        { }

        public NotFoundException(string message) : base(message)
        { }
    }
}
