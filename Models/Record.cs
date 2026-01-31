namespace Company.Function
{
    public class Record
    {
        public string Token { get; set; }
        public string id { get; set; }
        public string TagId { get; set; }
        public string EnteredDate { get; set; }
        public string ServicedDate { get; set; }
        public string MechanicName { get; set; }
        public string Odometer { get; set; }
        public string? Certified { get; set; }
        public string ServiceCategory { get; set; }
        public string ServiceType { get; set; }
        public string ServiceOption { get; set; }
        public string Comment { get; set; }
        public List<string> FileUrls { get; set; }
    }
}
