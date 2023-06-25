namespace DocuRISE.Shared.Models.User
{
    public class RegisterResult
    {
        public bool Successful { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public IEnumerable<string> Success { get; set; }
    }
}
