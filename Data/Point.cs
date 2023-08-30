using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClosestCommonNeighbour.Data
{
    internal class Point
    {
        public float Longitude { get; set; } = 0;
        public float Latitude { get; set; } = 0;

        public Point(float longitude, float latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        } 
    }

}
