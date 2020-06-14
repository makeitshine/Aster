using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTLStation
{
    class NotFoundException : Exception
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message)
        : base(message)
        {
        }

        public NotFoundException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    class DBNotConnectedException : Exception
    {
        public DBNotConnectedException()
        {
        }

        public DBNotConnectedException(string message)
        : base(message)
        {
        }

        public DBNotConnectedException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }

    class UnassignedCategoryException : Exception
    {
        public UnassignedCategoryException()
        {
        }

        public UnassignedCategoryException(string message)
        : base(message)
        {
        }

        public UnassignedCategoryException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
