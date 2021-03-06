using Empower.Common.Extensions;
using Empower.DomainService;
using Empower.Model;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Arise.Shared.CoreUI.Helpers;

namespace Arise.Shared.ViewComponents.Address
{
    public class AddressViewComponent : ViewComponent
    {
        private readonly AutoMapper.IMapper _mapper;
        private readonly ProviderDomainService _domainService;

        public AddressViewComponent(
            AutoMapper.IMapper mapper,
            ProviderDomainService domainService)
        {
            _mapper = mapper;
            _domainService = domainService;
        }

        public IViewComponentResult Invoke(AbstractAddress address, string propertyName, bool isReadOnly = false)
        {
            AddressViewModel vmAddress;

            if (address == null)
            {
                vmAddress = new AddressViewModel();
            }
            else
            {
                vmAddress = _mapper.Map<AddressViewModel>(address);
            }
            
            vmAddress.StateCodes = new SelectList(_domainService.Repository.States.AsNoTracking().OrderBy(s => s.Code).ToList(), nameof(State.Code), nameof(State.Code));
            vmAddress.Wards = _domainService.Repository.GetForeignKeyOptions<Ward>();
            vmAddress.Quadrants = _domainService.Repository.GetForeignKeyOptions<Quadrant>();
            vmAddress.IsReadOnly = isReadOnly;
            vmAddress.IsReadOnly = isReadOnly;
            vmAddress.Ward = _domainService.Repository.Wards.Where(w => w.ID == vmAddress.WardID).SingleOrDefault();
            vmAddress.Quadrant = _domainService.Repository.Quadrants.Where(w => w.ID == vmAddress.QuadrantID).SingleOrDefault();
            ViewData.SetHtmlFieldPrefix(propertyName);

            return View(vmAddress);
        }
    }

    public class AddressController : Controller
    {
        private readonly AutoMapper.IMapper _mapper;
        protected GISDomainService _gisDomainService { get; set; }

        public AddressController(
            AutoMapper.IMapper mapper,
            GISDomainService gisDomainService)
        {
            _mapper = mapper;
            _gisDomainService = gisDomainService;
        }

        public void SetupViewBag(bool isRequired)
        {
            ViewBag.StateCodes = new SelectList(_gisDomainService.Repository.States.AsNoTracking().OrderBy(s => s.Code).ToList(), nameof(State.Code), nameof(State.Code));

            if (isRequired)
            {
                ViewBag.MagisterialDistrictID = new SelectList(_gisDomainService.Repository.MagisterialDistricts.AsNoTracking().OrderBy(md => md.Name).ToList(), nameof(MagisterialDistrict.ID), nameof(MagisterialDistrict.Name));
            }
        }

        //public ActionResult SetupAddressWidget(Address address, string htmlFieldPrefix, int labelWidth = 0)
        //{
        //    ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;

        //    if (labelWidth != 0)
        //    {
        //        ViewBag.LabelWidth = labelWidth;
        //    }

        //    SetupViewBag(true);

        //    return PartialView("_Address", address);
        //}

        //public ActionResult SetupAddressUnrequiredWidget(AddressUnrequired address, string htmlFieldPrefix, int labelWidth = 0)
        //{
        //    ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;

        //    if (labelWidth != 0)
        //    {
        //        ViewBag.LabelWidth = labelWidth;
        //    }

        //    SetupViewBag(false);

        //    return PartialView("_AddressUnrequired", address);
        //}

        public IActionResult GenerateMailingAddressScripts(string maIsDiffClientID, string htmlFieldPrefix, int? addressID, int labelWidth = 0)
        {
            StringBuilder sb = new StringBuilder();

            if (!addressID.HasValue)
            {
                addressID = 0;
            }

            sb.AppendLine("$(document).ready(function () {");
            sb.AppendLine("\tif ($('#" + maIsDiffClientID + "')[0].checked) {");
            sb.AppendLine("\t\tPopulateMailingAddress();");
            sb.AppendLine("\t}");
            sb.AppendLine("});");
            sb.AppendLine();
            sb.AppendLine("$('#" + maIsDiffClientID + "').click(function () {");
            sb.AppendLine("\tif ($(this)[0].checked) {");
            sb.AppendLine("\t\tPopulateMailingAddress();");
            sb.AppendLine("\t} else {");
            sb.AppendLine("\t\t$('#divMailingAddress').slideUp(function () {");
            sb.AppendLine("\t\t\t$('#divMailingAddress').empty();");
            sb.AppendLine("\t\t});");
            sb.AppendLine("\t}");
            sb.AppendLine("});");
            sb.AppendLine();
            sb.AppendLine("function PopulateMailingAddress() {");
            sb.AppendLine("\tvar ma = $('#divMailingAddress');");
            sb.AppendLine();
            sb.AppendLine("\tma.css('height', '34px');");
            sb.AppendLine("\tma.css('position', 'relative');");
            sb.AppendLine();
            sb.AppendLine("\tkendo.ui.progress(ma, true);");
            sb.AppendLine("\tma.slideDown();");
            sb.AppendLine();
            sb.AppendLine("\tvar getUrl = '" + Url.Action("GetAddressWidget", nameof(AddressController).RemoveControllerFromName(), new { area = "" }) + "';");
            sb.AppendLine("\tvar addressID = " + addressID + " +0;");
            sb.AppendLine();
            sb.AppendLine("\t$.ajax({");
            sb.AppendLine("\t\turl: getUrl,");
            sb.AppendLine("\t\tdata: { addressID: addressID, htmlFieldPrefix: '" + htmlFieldPrefix + "' , labelWidth: " + labelWidth + " },");
            sb.AppendLine("\t\tsuccess: function (data) {");
            sb.AppendLine("\t\t\tma.slideUp(function () {");
            sb.AppendLine("\t\t\t\tma.css('height', '');");
            sb.AppendLine("\t\t\t\tma.html(data);");
            sb.AppendLine("\t\t\t\t$(':animated').promise().done(function () { ma.slideDown(); });");
            sb.AppendLine("\t\t\t});");
            sb.AppendLine("\t\t},");
            sb.AppendLine("\t\tcache: false");
            sb.AppendLine("\t});");
            sb.AppendLine("}");

            return JavaScript(sb.ToString());
        }

        public ActionResult GetAddressWidget(int addressID, string htmlFieldPrefix, int labelWidth = 0)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;
            ViewBag.IsMailingAddress = true;

            if (labelWidth != 0)
            {
                ViewBag.LabelWidth = labelWidth;
            }

            SetupViewBag(false);

            var address = _gisDomainService.Repository.PersonAddresses.Where(a => a.ID == addressID).SingleOrDefault();

            return PartialView("_AddressMailing", address);
        }

        public ActionResult GetMARAddresses(string addressLine1, string htmlFieldPrefix)
        {
            ViewData.TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;

            //SetupViewBag(true);

            return PartialView("_AddressMARResults", addressLine1);
        }

        public ActionResult GetMARAddressList([DataSourceRequest] DataSourceRequest request, string addressLine1)
        {
            throw new System.NotImplementedException("TODO: FIX THIS!!! Broken as part of .NET 5 migration");

            //var svc = new GIS.MARService();

            //var data = svc.GetMatchingAddresses(addressLine1);

            //return Json(data.ToDataSourceResult(request));
        }

        public IActionResult SelectMARAddress(int MARAddressKey)
        {
            throw new System.NotImplementedException("TODO: FIX THIS!!! Broken as part of .NET 5 migration");

            //var svc = new GIS.MARService();

            //var exactMatch = svc.GetExactAddress(MARAddressKey);

            ////NOTE: As a part of .NET 5.0 upgrade, JsonNetResult was changed to Json
            //// if there are any serialization issue,
            //// either try passing serializer settings to Json()
            //// or port JsonNetResult from old source code

            //return Json(exactMatch);
        }

        public IActionResult JavaScript(string script)
        {
            return Content(script, Empower.Common.Constant.Mvc.JavascriptHeader);
        }

        public bool SaveAddress(
            Empower.Model.Address address)
        {
            var result = false;

            if (address.ID == 0)
            {
                address.CreatedDate = DateTime.Now;

                _gisDomainService.Repository.Add(address);

                _gisDomainService.Save();
            }
            else
            {
                var currentAddress = _gisDomainService.Repository.Address.Where(a => a.ID == address.ID).SingleOrDefault();

                if (currentAddress != null)
                {
                    currentAddress = _mapper.Map<Empower.Model.Address>(address);

                    if (result)
                    {
                        throw new System.Exception();
                    }
                    else
                    {
                        _gisDomainService.Save();
                    }
                }
            }

            return result;
        }
    }
}
