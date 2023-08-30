namespace ClosestCommonNeighbour.Data
{
    internal record Vehicle(Int32 Id, string Registration, float Latitude, float Longitude, DateTime RecordedTimeUTC)
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public Vehicle(Int32 id, string registration, float latitude, float longitude, UInt64 recordedTime) : this(id, registration, latitude, longitude, epoch.AddSeconds(recordedTime))
        {

        }
    }
}