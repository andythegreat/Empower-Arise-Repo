using Microsoft.AspNetCore.Mvc.Rendering;

namespace Arise.Shared.ViewComponents.Address
{
    public class AddressViewModel : Empower.Model.Address
    {
        public AddressViewModel()
        {

        }

        public AddressViewModel(Empower.Model.Address address)
        {
            Address1 = address.Address1;
            Address2 = address.Address2;
            City = address.City;
            State = address.State;
            Zip = address.Zip;
            MagisterialDistrictID = address.MagisterialDistrictID;
            WardID = address.WardID;
        }

        public SelectList StateCodes { get; set; }

        public SelectList MagisterialDistricts { get; set; }

        public SelectList Wards { get; set; }
        public bool? IsReadOnly { get; set; }
    }
}
