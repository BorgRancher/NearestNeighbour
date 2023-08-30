using ClosestCommonNeighbour.Data;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;

namespace ClosestCommonNeighbour
{
    record VehicleEntry
    {
        public int Id { get; set; }
        public string Registration = "";
        public Vector2 Coord;
        public DateTime RecordedDateTime;
        public VehicleEntry(int id, string registration, Vector2 vector, DateTime recordedDateTime)
        {
            this.Id = id;
            this.Coord = vector;
            this.Registration = registration;
            this.RecordedDateTime = recordedDateTime;

        }

    }

    class Program
    {
        private const int RECORD_LIMIT = 2000000;
        private static string fileName = "Resources\\VehiclePositions.dat";
        private static ConcurrentDictionary<Int32, Vector2> pointsDict = new ConcurrentDictionary<int, Vector2>();
        private static ConcurrentDictionary<Int32, VehicleEntry> vehicleDict = new ConcurrentDictionary<int,VehicleEntry>();

        public static async Task Main(string[] args)
        {
           
            var executionTime = new Stopwatch();
            executionTime.Start();
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

            await LoadBinaryFileToDictionaryAsync();
           
            CalculateNearestNeighbours();
           
            executionTime.Stop();
            var message = $"Total execution time: {executionTime.ElapsedMilliseconds} ms.";
            Console.WriteLine(message);
        }

        private static void CalculateNearestNeighbours()
        {
            var postionCalculation = new Stopwatch();
            postionCalculation.Start();

            var output = new ConcurrentBag<string>();

            Parallel.ForEach(Partitioner.Create(0, pointsDict.Count), range =>
            {
                for(int index = range.Item1; index < range.Item2; index++)
                {
                    var targetPoint = pointsDict.ElementAt(index);
                    // Use some vector math to find the closest point
                    var vehicleKey = vehicleDict.OrderBy(p => (p.Value.Coord - targetPoint.Value).LengthSquared()).First().Key;
                    var vehicleEntry = vehicleDict[vehicleKey];
                    // Use haversine to calculate the actual distance in Km
                    var distance = GeoSpatial.Instance.CalculateDistance((double)targetPoint.Value.X, (double)targetPoint.Value.Y,
                         (double)vehicleEntry.Coord.X, (double)vehicleEntry.Coord.Y);

                    // Write the result out to the console
                    output.Add($"{targetPoint.Key} {vehicleEntry.Registration} - {Math.Round(distance, 2)} km away on {vehicleEntry.RecordedDateTime}");
                }

            });
            // Write the collected output to the console
            foreach (var line in output)
            {
                Console.WriteLine(line);
            }

            postionCalculation.Stop();
            var message = $"Nearest neighbours found: {postionCalculation.ElapsedMilliseconds} ms.";
            Console.WriteLine(message);
        }

        /**
         * Use batch-loading to speed up the process.
         */
        private static async Task LoadBinaryFileToDictionaryAsync()
        {
            var fileLoading = new Stopwatch();
            fileLoading.Start();

            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                var batch = new List<VehicleEntry>();
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        var vehiclePosition = await ReadBinaryVehicleAsync(reader);
                        batch.Add(new VehicleEntry(vehiclePosition.Id, vehiclePosition.Registration, new Vector2(vehiclePosition.Latitude, vehiclePosition.Longitude), vehiclePosition.RecordedTimeUTC));

                        if (batch.Count >= 1000) // Adjust batch size as needed
                        {
                            AddBatchToDictionary(batch);
                            batch.Clear();
                        }
                    }
                }

                if (batch.Count > 0)
                {
                    AddBatchToDictionary(batch);
                }
            }

            fileLoading.Stop();

            var message = $"File read & loaded {vehicleDict.Count} records: {fileLoading.ElapsedMilliseconds} ms.";
            Console.WriteLine(message);
        }

        private static void AddBatchToDictionary(List<VehicleEntry> batch)
        {
            foreach (var vehicleEntry in batch)
            {
                vehicleDict.TryAdd(vehicleEntry.Id, vehicleEntry);
            }
        }

        private static async Task<Vehicle> ReadBinaryVehicleAsync(BinaryReader reader)
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
            var chars = new Span<char>(new char[1024]); // Initialize a Span with a reasonable initial capacity
            int index = 0;
            char ch;
            while ((ch = reader.ReadChar()) != '\0')
            {
                if (index >= chars.Length) // Resize the Span if necessary
                {
                    var newChars = new char[chars.Length * 2];
                    chars.CopyTo(newChars);
                    chars = newChars;
                }
                chars[index++] = ch;
            }
            return new string(chars.Slice(0, index)); // Use Slice to get only the filled part of the Span

        }
    }
}

