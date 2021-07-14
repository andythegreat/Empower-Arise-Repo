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

        //public AddressViewModel(Empower.Model.Address address)
        //{
        //    Address1 = address.Address1;
        //    Address2 = address.Address2;
        //    City = address.City;
        //    State = address.State;
        //    Zip = address.Zip;
        //    MagisterialDistrictID = address.MagisterialDistrictID;
        //    WardID = address.WardID;
        //}

        //public AddressViewModel(AutoMapper.IMapper mapper, Empower.Model.AbstractAddress address)
        //{
        //    //this = mapper.Map<AddressViewModel>(address);

        //    ID = address.ID;
        //    Address1 = address.Address1;
        //    Address2 = address.Address2;
        //    City = address.City;
        //    State = address.State;
        //    Zip = address.Zip;
        //    MagisterialDistrictID = address.MagisterialDistrictID;
        //    WardID = address.WardID;
        //}

        public SelectList StateCodes { get; set; }

        public SelectList MagisterialDistricts { get; set; }

        public SelectList Wards { get; set; }

        public bool? IsReadOnly { get; set; }
    }
}
