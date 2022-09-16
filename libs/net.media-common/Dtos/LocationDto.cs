namespace adworks.media_common
{
    public class LocationDto: EntityDto
    {
        public string Address { get; set; }
        public string Locale { get; set; }
        public double TimezoneOffset { get; set; }
    }
}