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
            equipmentMaintenanceLogViewModel.FacilityIDs = (from app in ProviderDomainService.Repository.Applications
                                                            join fac in ProviderDomainService.Repository.Facilities
                                                            on app.FacilityID equals fac.ID
                                                            where app.ApplicationStatusID != Empower.Model.LookupIDs.ApplicationStatuses.Pending
                                                            select new SelectListItem
                                                            {
                                                                Value = fac.ID.ToString(),
                                                                Text = fac.FacilityName.ToString()
                                                            }).Union(
                                                            ProviderDomainService.Repository.Facilities
                                                            .Select(fi => new SelectListItem
                                                            {
                                                                Value = fi.ID.ToString(),
                                                                Text = fi.FacilityName.ToString()
                                                            })).ToList();
            return View(equipmentMaintenanceLogViewModel);
        }
        public IActionResult GetEquipmentInspectionLogs([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
            var objStaffData = ProviderDomainService.Repository.EquipmentInspectionLogs.Where(s => s.IsDeleted != true).ToList();
            if (facilityID > 0)
            {
               
                objStaffData = objStaffData.Where(s => s.FacilityID == facilityID).ToList();
            }
            return Json(objStaffData.ToDataSourceResult(request));
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddEquipmentInspectionLog([DataSourceRequest] DataSourceRequest request, EquipmentInspectionLog EquipmentInspectionLog, int facilityID)
        {
            EquipmentInspectionLog.IsDeleted = false;
            EquipmentInspectionLog.FacilityID = facilityID;
            EquipmentInspectionLog.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
            EquipmentInspectionLog.CreatedBy = UserID;
            ProviderDomainService.Repository.Add(EquipmentInspectionLog);
            ProviderDomainService.Save();
            return Json(new[] { EquipmentInspectionLog }.ToDataSourceResult(request));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> UpdateEquipmentInspectionLog([DataSourceRequest] DataSourceRequest request, EquipmentInspectionLog EquipmentInspectionLog)
        {
            var objEquipmentInspectionLog = ProviderDomainService.Repository.EquipmentInspectionLogs.Where(p => p.ID == EquipmentInspectionLog.ID).FirstOrDefault();
            TryUpdateModelAsync<EquipmentInspectionLog>(objEquipmentInspectionLog);
            ProviderDomainService.Repository.Update(objEquipmentInspectionLog, objEquipmentInspectionLog.ID);
            ProviderDomainService.Save();
            return Json(new[] { EquipmentInspectionLog }.ToDataSourceResult(request));
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult DeleteEquipmentInspectionLog([DataSourceRequest] DataSourceRequest request, EquipmentInspectionLog EquipmentInspectionLog)
        {
            var objEquipmentMaintenanceLog = ProviderDomainService.Repository.EquipmentInspectionLogs
                            .Where(c => c.ID == EquipmentInspectionLog.ID).FirstOrDefault();
            objEquipmentMaintenanceLog.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { EquipmentInspectionLog }.ToDataSourceResult(request));
        }
    }
}
