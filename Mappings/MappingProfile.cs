using AutoMapper;
using Kantin_Paramadina.DTO;
using Kantin_Paramadina.Model;

namespace Kantin_Paramadina.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Outlet ↔ OutletDto
            CreateMap<Outlet, OutletDto>()
                .ForMember(dest => dest.MenuItems, opt => opt.MapFrom(src => src.MenuItems))
                .ForMember(dest => dest.MenuItems, opt => opt.MapFrom(src => src.MenuItems))
                .ForMember(dest => dest.QrisImageUrl, opt => opt.MapFrom(src => src.QrisImageUrl));

            // ===== MENU ITEM =====
            CreateMap<MenuItem, MenuItemDto>()
                .ForMember(dest => dest.StockQuantity,
                           opt => opt.MapFrom(src => src.Stock != null ? src.Stock.Quantity : 0))
                .ForMember(dest => dest.OutletName,
                           opt => opt.MapFrom(src => src.Outlet != null ? src.Outlet.Name : null));

            CreateMap<MenuItemCreateDto, MenuItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Stock, opt => opt.Ignore())
                .ForMember(dest => dest.Outlet, opt => opt.Ignore());

            CreateMap<MenuItemUpdateDto, MenuItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Stock, opt => opt.Ignore())
                .ForMember(dest => dest.Outlet, opt => opt.Ignore());

            // Transaction ↔ TransactionDto
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.OutletName,
                    opt => opt.MapFrom(src => src.Outlet != null ? src.Outlet.Name : null))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<TransactionCreateDto, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentProofPath, opt => opt.Ignore());



            // TransactionItem ↔ TransactionItemDto
            CreateMap<TransactionItem, TransactionItemDto>()
                .ForMember(dest => dest.MenuName,
                    opt => opt.MapFrom(src => src.MenuItem != null ? src.MenuItem.Name : "Unknown"))
                .ForMember(dest => dest.SubTotal,
                    opt => opt.MapFrom(src => src.UnitPrice * src.Quantity));

            CreateMap<TransactionItemCreateDto, TransactionItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore()) // harga ditentukan dari menu, bukan input user
                .ForMember(dest => dest.MenuItem, opt => opt.Ignore())
                .ForMember(dest => dest.Transaction, opt => opt.Ignore());
        }
    }

}
