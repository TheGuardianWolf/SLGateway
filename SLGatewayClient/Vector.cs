using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient
{
    public interface IVector
    {
        float X { get; }
        float Y { get; }
        float Z { get; }
    }

    public class Vector : IVector
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
