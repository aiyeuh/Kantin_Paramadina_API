namespace Kantin_Paramadina.DTO
{
    public class OutletDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Location { get; set; }
        public string? QrisImageUrl { get; set; }

        public List<MenuItemDto>? MenuItems { get; set; }
    }
}
