namespace Code_reading
{
    internal class RxGenRequest
    {
        public int CaseType { get; internal set; } // restorative or orthodontic
        public int CompanyId { get; internal set; }
        public int ContactId { get; internal set; }
        public string DoctorNotesString { get; internal set; }
        public string RxJson { get; internal set; }
        public string PatientFirstName { get; internal set; }
        public string PatientLastName { get; internal set; }
    }
}
