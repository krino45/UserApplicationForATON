namespace UserApplication.API.Models
{
    public enum Gender
    {
        Female = 0,
        Male = 1,
        Unknown = 2
    }

    public class User
    {
        
        public Guid Date { get; set; }
        public required string Login { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public Gender Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Admin { get; }
        public DateTime CreatedOn { get; set; }
        public required string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public required string ModifiedBy { get; set; }
        public DateTime RevokedOn { get; set; }
        public required string RevokedBy { get; set; }

    }
}
