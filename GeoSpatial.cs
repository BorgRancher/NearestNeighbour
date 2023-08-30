namespace ClosestCommonNeighbour
{
    internal class GeoSpatial
    {
        private GeoSpatial() { }
        private static readonly Lazy<GeoSpatial> lazy = new Lazy<GeoSpatial>(() => new GeoSpatial());
        public static GeoSpatial Instance
        {
            get { return lazy.Value; }
        }

        /**
         * Haversine formula calculates approximate distance between two points
         * over the surface of a sphere. (in Kilometers)
         */

        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radius of the Earth in kilometers

            double lat1Rad = ToRadians(lat1);
            double lon1Rad = ToRadians(lon1);
            double lat2Rad = ToRadians(lat2);
            double lon2Rad = ToRadians(lon2);

            double dLat = lat2Rad - lat1Rad;
            double dLon = lon2Rad - lon1Rad;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distance = R * c;
            return distance;
        }

        private static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}