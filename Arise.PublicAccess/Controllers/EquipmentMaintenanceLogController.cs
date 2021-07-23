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
    public class EquipmentMaintenanceLogController : BaseController
    {
        public EquipmentMaintenanceLogController(MessagingService messagingService, ProviderDomainService domainService,
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
        public IActionResult GetEquipmentMaintenanceLogs([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
            var objStaffData =ProviderDomainService.Repository.EquipmentMaintenanceLogs.Where(s => s.IsDeleted != true).ToList();
            if (facilityID > 0)
            {
                objStaffData = objStaffData.Where(s => s.FacilityID == facilityID).ToList();
            }
            return Json(objStaffData.ToDataSourceResult(request));
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddEquipmentMaintenanceLog([DataSourceRequest] DataSourceRequest request, EquipmentMaintenanceLog EquipmentMaintenanceLog, int facilityID)
        {
            EquipmentMaintenanceLog.IsDeleted = false;
            EquipmentMaintenanceLog.FacilityID = facilityID;
            EquipmentMaintenanceLog.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
            EquipmentMaintenanceLog.CreatedBy = UserID;
            ProviderDomainService.Repository.Add(EquipmentMaintenanceLog);
            ProviderDomainService.Save();
            return Json(new[] { EquipmentMaintenanceLog }.ToDataSourceResult(request));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> UpdateEquipmentMaintenanceLog([DataSourceRequest] DataSourceRequest request, EquipmentMaintenanceLog EquipmentMaintenanceLog)
        {
            var objEquipmentMaintenanceLog = ProviderDomainService.Repository.EquipmentMaintenanceLogs.Where(p => p.ID == EquipmentMaintenanceLog.ID).FirstOrDefault();
            //PA_EquipmentMaintenanceLog EquipmentMaintenanceLog = new PA_EquipmentMaintenanceLog();
            _ = TryUpdateModelAsync<EquipmentMaintenanceLog>(objEquipmentMaintenanceLog);
            ProviderDomainService.Repository.Update(objEquipmentMaintenanceLog, objEquipmentMaintenanceLog.ID);
            ProviderDomainService.Save();
            return Json(new[] { EquipmentMaintenanceLog }.ToDataSourceResult(request));
           }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult DeleteEquipmentMaintenanceLog([DataSourceRequest] DataSourceRequest request, EquipmentMaintenanceLog EquipmentMaintenanceLog)
        {
            var objEquipmentMaintenanceLog = ProviderDomainService.Repository.EquipmentMaintenanceLogs
                            .Where(c => c.ID == EquipmentMaintenanceLog.ID).FirstOrDefault();
            objEquipmentMaintenanceLog.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { EquipmentMaintenanceLog }.ToDataSourceResult(request));
        }
    }
    }
