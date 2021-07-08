using Arise.PublicAccess.Models;
using Empower.AccessControl;
using Empower.Common.CacheProviders;
using Empower.DomainService;
using Empower.Messaging;
using Empower.Model;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Arise.PublicAccess.Controllers
{
    public class EquipmentInspectionLogController :BaseController
    {
        public EquipmentInspectionLogController(MessagingService messagingService, ProviderDomainService domainService,
            Empower.Logging.ILogger logger, AccessControlManager accessControlManager, ICacheProvider cacheProvider)
            : base(domainService, logger, accessControlManager, cacheProvider)
        {

        }
        public IActionResult Index()
        {
            EquipmentMaintenanceLogViewModel equipmentMaintenanceLogViewModel = new EquipmentMaintenanceLogViewModel();
            equipmentMaintenanceLogViewModel.FacilityIDs = (from app in ProviderDomainService.Repository.PA_Applications
                                                            join fac in ProviderDomainService.Repository.PA_Facilities
                                                            on app.FacilityID equals fac.ID
                                                            join fi in ProviderDomainService.Repository.PA_FacilityInformations
                                                            on fac.ID equals fi.FacilityID
                                                            where app.ApplicationStatusID != Empower.Model.LookupIDs.ApplicationStatuses.Pending
                                                            select new SelectListItem
                                                            {
                                                                Value = fi.FacilityID.ToString(),
                                                                Text = fi.FacilityName.ToString()
                                                            }).Union(
                                                            ProviderDomainService.Repository.FacilityInformations
                                                            .Select(fi => new SelectListItem
                                                            {
                                                                Value = fi.FacilityID.ToString(),
                                                                Text = fi.FacilityName.ToString()
                                                            })).ToList();
            return View(equipmentMaintenanceLogViewModel);
        }
        public IActionResult GetEquipmentInspectionLogs([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
            var objStaffData = ProviderDomainService.Repository.PA_EquipmentInspectionLogs.Where(s => s.IsDeleted != true).ToList();
            if (facilityID > 0)
            {
               
                objStaffData = objStaffData.Where(s => s.FacilityID == facilityID).ToList();
            }
            return Json(objStaffData.ToDataSourceResult(request));
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddEquipmentInspectionLog([DataSourceRequest] DataSourceRequest request, PA_EquipmentInspectionLog pA_EquipmentInspectionLog, int facilityID)
        {
            pA_EquipmentInspectionLog.IsDeleted = false;
            pA_EquipmentInspectionLog.FacilityID = facilityID;
            pA_EquipmentInspectionLog.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
            pA_EquipmentInspectionLog.CreatedBy = UserID;
            ProviderDomainService.Repository.Add(pA_EquipmentInspectionLog);
            ProviderDomainService.Save();
            return Json(new[] { pA_EquipmentInspectionLog }.ToDataSourceResult(request));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> UpdateEquipmentInspectionLog([DataSourceRequest] DataSourceRequest request, PA_EquipmentInspectionLog pA_EquipmentInspectionLog)
        {
            var objEquipmentInspectionLog = ProviderDomainService.Repository.PA_EquipmentInspectionLogs.Where(p => p.ID == pA_EquipmentInspectionLog.ID).FirstOrDefault();
            PA_EquipmentInspectionLog EquipmentMaintenanceLog = new PA_EquipmentInspectionLog();
            _ = TryUpdateModelAsync<PA_EquipmentInspectionLog>(objEquipmentInspectionLog);
            ProviderDomainService.Repository.Update(objEquipmentInspectionLog, objEquipmentInspectionLog.ID);
            ProviderDomainService.Save();
            return Json(new[] { pA_EquipmentInspectionLog }.ToDataSourceResult(request));
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult DeleteEquipmentInspectionLog([DataSourceRequest] DataSourceRequest request, PA_EquipmentInspectionLog pA_EquipmentInspectionLog)
        {
            var objEquipmentMaintenanceLog = ProviderDomainService.Repository.PA_EquipmentInspectionLogs
                            .Where(c => c.ID == pA_EquipmentInspectionLog.ID).FirstOrDefault();
            objEquipmentMaintenanceLog.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { pA_EquipmentInspectionLog }.ToDataSourceResult(request));
        }
    }
}
