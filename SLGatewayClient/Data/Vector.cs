using System;
using System.Collections.Generic;
using System.Text;

namespace SLGatewayClient.Data
{
    public interface IVector
    {
        double X { get; }
        double Y { get; }
        double Z { get; }
    }

    public struct Vector : IVector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
