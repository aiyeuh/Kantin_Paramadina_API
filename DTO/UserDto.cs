namespace Kantin_Paramadina.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? FullName { get; set; }
        public int? OutletId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
