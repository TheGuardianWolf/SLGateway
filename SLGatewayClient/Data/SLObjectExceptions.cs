using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient
{
    public class CommandFailedException : Exception
    {
        public CommandFailedException(int statusCode) : base($"Command failed with status: {statusCode}") { }
        public CommandFailedException(string message) : base($"Command failed with message: {message}") { }
    }

    public class InvalidReturnDataTypeException : Exception
    {
        public InvalidReturnDataTypeException(Type expected, object? data) : base($"Expected data of type {expected.FullName} but got type {data?.GetType().FullName}") { }
    }

    public class InvalidReturnDataLengthException : Exception
    {
        public InvalidReturnDataLengthException(int expected, int actual) : base($"Expected data of length {expected} but got {actual}") { }
    }

    public class InvalidScopeException : Exception
    {
        public InvalidScopeException(string message) : base(message) { }
    }
}
