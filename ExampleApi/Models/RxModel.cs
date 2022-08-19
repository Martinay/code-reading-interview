namespace ExampleApi.Models
{
    public class RxModel
    {
        public Guid ID { get; set; }
        public DateTime ScanDate { get; set; }
        public DateTime DueDate { get; set; }
        public string DoctorNotes { get; set; }
        public string TeethToChange { get; set; }
        public int CompanyID { get; set; }
        public int ContactID { get; set; }
    }
}
