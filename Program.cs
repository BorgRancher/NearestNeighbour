using ClosestCommonNeighbour.Data;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace ClosestCommonNeighbour
{
    record VehicleEntry
    {
        public string Registration = "";
        public Vector2 Coord;
        public VehicleEntry(string registration, Vector2 vector)
        {
            this.Coord = vector;
            this.Registration = registration;
        }

    }

    class Program
    {
        private static string fileName = "Resources\\VehiclePositions.dat";
        private static Dictionary<Int32, Vector2> pointsDict = new Dictionary<Int32, Vector2>();
        private static Dictionary<Int32, VehicleEntry> vehicleDict = new Dictionary<Int32,VehicleEntry>();

        public static void Main(string[] args)
        {
            // create reference points
            pointsDict.TryAdd(1, new Vector2(34.544909f, -102.100843f));
            pointsDict.TryAdd(2, new Vector2(32.345544f, -99.123124f));
            pointsDict.TryAdd(3, new Vector2(33.234235f, -100.214124f));
            pointsDict.TryAdd(4, new Vector2(35.195739f, -95.348899f));
            pointsDict.TryAdd(5, new Vector2(31.895839f, -97.789573f));
            pointsDict.TryAdd(6, new Vector2(32.895839f, -101.789573f));
            pointsDict.TryAdd(7, new Vector2(34.115839f, -100.225732f));
            pointsDict.TryAdd(8, new Vector2(32.335839f, -99.992232f));
            pointsDict.TryAdd(9, new Vector2(33.535339f, -94.792232f));
            pointsDict.TryAdd(10, new Vector2(32.234235f, -97.789573f));

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    // Read a record from the file
                    var vehiclePosition = ReadBinaryVehicle(reader);
                    // LogObjectProperties(vehiclePosition);
                    vehicleDict.Add(vehiclePosition.Id, new VehicleEntry(vehiclePosition.Registration, new Vector2(vehiclePosition.Latitude, vehiclePosition.Longitude)));
                }
            }

            stopwatch.Stop();
            var message = String.Format("File read: {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine(message);

            stopwatch.Restart();
            var pointTasks = new List<Task>();
            foreach(var targetPoint in pointsDict)
            {
                // Use some vector math to find the closest point
                var closestKey = vehicleDict.OrderBy(p => (p.Value.Coord - targetPoint.Value).LengthSquared()).First().Key;
                var vehicleEntry = vehicleDict[closestKey];

                // Use haversine to calculate the actual distance in Km
                var distance = GeoSpatial.Instance.CalculateDistance((double) targetPoint.Value.X, (double) targetPoint.Value.Y,
                     (double) vehicleEntry.Coord.X, (double) vehicleEntry.Coord.Y);
                
                // Write the result out to the console
                Console.WriteLine($"{targetPoint.Key} {vehicleEntry.Registration} - {Math.Round(distance,2)} km away");
            }
            stopwatch.Stop();
            message = String.Format("Nearest neighbours found: {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine(message);
        }

        private static Vehicle ReadBinaryVehicle(BinaryReader reader)
        {
            var id = reader.ReadInt32();
            var registration = ReadNullTerminatedString(reader);
            var latitude = reader.ReadSingle();
            var longitude = reader.ReadSingle();
            var recordedTime = reader.ReadUInt64();

            return new Vehicle(id, registration, latitude, longitude, recordedTime);


        }

        public static string ReadNullTerminatedString(BinaryReader reader)
        {
            StringBuilder result = new StringBuilder();
            char ch;
            while ((ch = reader.ReadChar()) != '\0')
            {
                result.Append(ch);
            }
            return result.ToString();
        }
    }
}

