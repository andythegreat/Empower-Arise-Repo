using Arise.PublicAccess.Models;
using Arise.Shared.CoreUI.Helpers;
using Empower.AccessControl;
using Empower.Common.CacheProviders;
using Empower.DomainService;
using Empower.Messaging;
using Empower.Model;
using Empower.Model.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arise.PublicAccess.Controllers
{
    public class EquipmentMaintenanceLogController : BaseController
    {
        public static int FacilityID;
        public EquipmentMaintenanceLogController(MessagingService messagingService, ProviderDomainService domainService,
            Empower.Logging.ILogger logger, AccessControlManager accessControlManager, ICacheProvider cacheProvider)
            : base(domainService, logger, accessControlManager, cacheProvider)
        {
           
        }
        public IActionResult Index()
        {
            EquipmentMaintenanceLogViewModel equipmentMaintenanceLogViewModel = new EquipmentMaintenanceLogViewModel();
            equipmentMaintenanceLogViewModel.FacilityIDs = (from AP in ProviderDomainService.Repository.PA_Applications
                                                            join FA in ProviderDomainService.Repository.PA_FacilityInformations on AP.PA_FacilityID equals FA.FacilityID
                                                            join N in ProviderDomainService.Repository.PA_Names on FA.NameID equals N.ID
                                                            where AP.CreatedBy == UserID
                                                            select new SelectListItem
                                                            {
                                                                Value = FA.FacilityID.ToString(),
                                                                Text = N.FirstName,
                                                            }).ToList();
            return View(equipmentMaintenanceLogViewModel);
        }
        public IActionResult GetEquipmentMaintenanceLogs([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
            var objStaffData =ProviderDomainService.Repository.PA_EquipmentMaintenanceLog.Where(s => s.IsDeleted != true).ToList();

            if (facilityID > 0)
            {
                FacilityID = facilityID;
                objStaffData = objStaffData.Where(s => s.FacilityID == facilityID).ToList();
            }

            return Json(objStaffData.ToDataSourceResult(request));
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddEquipmentMaintenanceLog([DataSourceRequest] DataSourceRequest request, PA_EquipmentMaintenanceLog pA_EquipmentMaintenanceLog)
        {
            pA_EquipmentMaintenanceLog.IsDeleted = false;
            pA_EquipmentMaintenanceLog.FacilityID = FacilityID;
            pA_EquipmentMaintenanceLog.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
            pA_EquipmentMaintenanceLog.CreatedBy = UserID;
            ProviderDomainService.Repository.Add(pA_EquipmentMaintenanceLog);
            ProviderDomainService.Save();
            return Json(new[] { pA_EquipmentMaintenanceLog }.ToDataSourceResult(request));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> UpdateEquipmentMaintenanceLog([DataSourceRequest] DataSourceRequest request, PA_EquipmentMaintenanceLog pA_EquipmentMaintenanceLog)
        {
            var objEquipmentMaintenanceLog = ProviderDomainService.Repository.PA_EquipmentMaintenanceLog.Where(p => p.ID == pA_EquipmentMaintenanceLog.ID).FirstOrDefault();
            PA_EquipmentMaintenanceLog EquipmentMaintenanceLog = new PA_EquipmentMaintenanceLog();
            _ = TryUpdateModelAsync<PA_EquipmentMaintenanceLog>(objEquipmentMaintenanceLog);
            ProviderDomainService.Repository.Update(objEquipmentMaintenanceLog, objEquipmentMaintenanceLog.ID);
            ProviderDomainService.Save();

            return Json(new[] { pA_EquipmentMaintenanceLog }.ToDataSourceResult(request));
           }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult DeleteEquipmentMaintenanceLog([DataSourceRequest] DataSourceRequest request, PA_EquipmentMaintenanceLog pA_EquipmentMaintenanceLog)
        {
            var objEquipmentMaintenanceLog = ProviderDomainService.Repository.PA_EquipmentMaintenanceLog
                            .Where(c => c.ID == pA_EquipmentMaintenanceLog.ID).FirstOrDefault();
            objEquipmentMaintenanceLog.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { pA_EquipmentMaintenanceLog }.ToDataSourceResult(request));
        }
    }
    }
