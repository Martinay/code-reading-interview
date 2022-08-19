namespace ExampleApi.Models
{
    public class DownloadRequest
    {
        public int CompanyID { get; set; }
        public int RxID { get; set; }
        public int ContactID { get; set; }
        public bool ShouldAnonymize { get; set; }
    }
}
