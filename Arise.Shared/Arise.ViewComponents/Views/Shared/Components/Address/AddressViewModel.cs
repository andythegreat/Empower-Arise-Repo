using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arise.Shared.ViewComponents.Address
{
    public class AddressProfile : AutoMapper.Profile
    {
        public AddressProfile()
        {
            CreateMap<Empower.Model.AbstractAddress, AddressViewModel>();

            // Mapping when property names are different
            //CreateMap<User, UserViewModel>()
            //    .ForMember(dest =>
            //    dest.FName,
            //    opt => opt.MapFrom(src => src.FirstName))
            //    .ForMember(dest =>
            //    dest.LName,
            //    opt => opt.MapFrom(src => src.LastName));
        }
    }

    public class AddressViewModel : Empower.Model.AbstractAddress
    {
        public AddressViewModel()
        {
        }

        public SelectList StateCodes { get; set; }

        public SelectList MagisterialDistricts { get; set; }

        public SelectList Wards { get; set; }

        public bool? IsReadOnly { get; set; }
    }
}
