namespace devbuddy.business.Models
{
    public class RegisterRequest : LoginRequest
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
