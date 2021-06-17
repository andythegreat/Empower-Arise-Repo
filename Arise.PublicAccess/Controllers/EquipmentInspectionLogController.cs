﻿using Arise.PublicAccess.Models;
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
    public class EquipmentInspectionLogController :BaseController
    {
        public static int FacilityID;
        public EquipmentInspectionLogController(MessagingService messagingService, ProviderDomainService domainService,
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
        public IActionResult GetEquipmentInspectionLogs([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
            var objStaffData = ProviderDomainService.Repository.PA_EquipmentInspectionLog.Where(s => s.IsDeleted != true).ToList();

            if (facilityID > 0)
            {
                FacilityID = facilityID;
                objStaffData = objStaffData.Where(s => s.FacilityID == facilityID).ToList();
            }

            return Json(objStaffData.ToDataSourceResult(request));
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddEquipmentInspectionLog([DataSourceRequest] DataSourceRequest request, PA_EquipmentInspectionLog pA_EquipmentInspectionLog)
        {
            pA_EquipmentInspectionLog.IsDeleted = false;
            pA_EquipmentInspectionLog.FacilityID = FacilityID;
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
            var objEquipmentInspectionLog = ProviderDomainService.Repository.PA_EquipmentInspectionLog.Where(p => p.ID == pA_EquipmentInspectionLog.ID).FirstOrDefault();
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
            var objEquipmentMaintenanceLog = ProviderDomainService.Repository.PA_EquipmentInspectionLog
                            .Where(c => c.ID == pA_EquipmentInspectionLog.ID).FirstOrDefault();
            objEquipmentMaintenanceLog.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { pA_EquipmentInspectionLog }.ToDataSourceResult(request));
        }
    }
}
