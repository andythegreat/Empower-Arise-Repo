using Empower.AccessControl;
using Empower.Common.CacheProviders;
using Empower.DomainService;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Linq.Translations;
using System.Data.Entity;
using System.Linq;
using Arise.PublicAccess.Models;

namespace Arise.PublicAccess.Controllers
{
    public partial class ScheduledInspectionsController : BaseController
    {
        public ScheduledInspectionsController(ProviderDomainService domainService,
            Empower.Logging.ILogger logger, AccessControlManager accessControlManager, ICacheProvider cacheProvider)
            : base(domainService, logger, accessControlManager, cacheProvider)
        {
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListScheduledInspectionsHome([DataSourceRequest]DataSourceRequest request)
        {
            var ListScheduledInspections = ProviderDomainService.Repository.ProviderInspectionHomes
             .Include(pi => pi.ProviderAddress)
             .Include(pi => pi.ProviderAddress.AddressType)
             .Include(pi => pi.ProviderAddress.Provider)
             .Where(pi => pi.ProviderAddress.ProviderID == ProviderDomainService.ProviderID
                     && pi.ProviderAddress.IsCurrent == true
                     && pi.InspectionDate == null).Select(pi => new ScheduledInspectionsHomeViewModel
                     {
                         HomeInspectionId = pi.ID,
                         HomeInspectionAddressId = pi.ProviderAddressID,
                         HomeInspectionAddress = pi.ProviderAddress.AddressText,
                         HomeInspectionAddressType = pi.ProviderAddress.AddressType.Description,

                         RequestedDate = pi.RequestedDate,
                         ScheduledDate = pi.ScheduledDate

                     }).WithTranslations().ToList();

            return Json(ListScheduledInspections.ToDataSourceResult(request));
        }

        public ActionResult ListScheduledInspectionsFire([DataSourceRequest]DataSourceRequest request)
        {
            var ListScheduledInspectionsFire = ProviderDomainService.Repository.ProviderInspectionFires
             .Include(pi => pi.ProviderAddress)
             .Include(pi => pi.ProviderAddress.AddressType)
             .Include(pi => pi.ProviderAddress.Provider)
             .Where(pi => pi.ProviderAddress.ProviderID == ProviderDomainService.ProviderID
                     && pi.ProviderAddress.IsCurrent == true
                     && pi.InspectionDate == null).Select(pi => new ScheduledInspectionsFireViewModel
                     {
                         FireInspectionId = pi.ID,
                         FireInspectionAddressId = pi.ProviderAddressID,
                         FireInspectionAddress = pi.ProviderAddress.AddressText,
                         FireInspectionAddressType = pi.ProviderAddress.AddressType.Description,

                         RequestedDate = pi.RequestedDate,
                         ScheduledDate = pi.ScheduledDate

                     }).WithTranslations().ToList();

            return Json(ListScheduledInspectionsFire.ToDataSourceResult(request));
        }
    }
}
