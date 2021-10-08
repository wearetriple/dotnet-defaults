using System;

namespace UnitTestingExample
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
        { }

        public NotFoundException(string message) : base(message)
        { }
    }
}
