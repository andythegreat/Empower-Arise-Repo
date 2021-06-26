﻿using Arise.PublicAccess.Models;
using Empower.AccessControl;
using Empower.Common.CacheProviders;
using Empower.DomainService;
using Empower.Messaging;
using Empower.Model;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Linq.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arise.PublicAccess.Controllers
{
    public class StaffChangeHistoryController :BaseController
    {
        public StaffChangeHistoryController(MessagingService messagingService, ProviderDomainService domainService,
           Empower.Logging.ILogger logger, AccessControlManager accessControlManager, ICacheProvider cacheProvider)
           : base(domainService, logger, accessControlManager, cacheProvider)
        {
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult StaffTransfer()
        {
            StaffChangeLogHistoryViewModel staffChangeLogHistoryViewModel = new StaffChangeLogHistoryViewModel();
            staffChangeLogHistoryViewModel.ClassIDs= (from f in ProviderDomainService.Repository.PA_AddClassRoom
                                                      select new SelectListItem
                                                      {
                                                          Value = f.ID.ToString(),
                                                          Text = f.ClassRoomName
                                                      }).ToList();

            staffChangeLogHistoryViewModel.StaffIDs = (from p in ProviderDomainService.Repository.PA_People
                                                      join s in ProviderDomainService.Repository.PA_Staffs
                                                      on p.ID equals s.PersonID
                                                      join sc in ProviderDomainService.Repository.PA_StaffCharacteristics
                                                      on s.ID equals sc.StaffID
                                                      where s.FacilityTypeID == 0 
                                                      select new SelectListItem
                                                      {
                                                          Value = p.ID.ToString(),
                                                          Text = p.FullName
                                                      }).WithTranslations().ToList();
                        return View(staffChangeLogHistoryViewModel);
        }
        public IActionResult GetStaffData([DataSourceRequest] DataSourceRequest request)
        {
            var objStaffData = (from sc in ProviderDomainService.Repository.PA_AddClassRoom
                                     join st in ProviderDomainService.Repository.PA_StaffClassRoom
                                     on sc.ID equals st.ClassRoomID
                                    select new StaffChangeLogHistoryViewModel{
                                      StaffID =st.StaffID,
                                      CurrentClassRoomID =sc.ID
                                     });
            return Json(objStaffData.ToDataSourceResult(request));
        }
        public IActionResult SaveStaff([DataSourceRequest] DataSourceRequest request, PA_StaffChangeLogHistory pA_StaffChangeLogHistory)
        {
            var staffClassRoomData = ProviderDomainService.Repository.PA_StaffClassRoom.Where(x => x.StaffID == pA_StaffChangeLogHistory.StaffID && x.ClassRoomID == pA_StaffChangeLogHistory.CurrentClassRoomID).FirstOrDefault();
            staffClassRoomData.ClassRoomID = pA_StaffChangeLogHistory.NewClassRoomID;
            ProviderDomainService.Save();
            pA_StaffChangeLogHistory.CreatedDate = DateTime.Now;
            ProviderDomainService.Repository.Add(pA_StaffChangeLogHistory);
            ProviderDomainService.Save();
            return Json(new[] { pA_StaffChangeLogHistory }.ToDataSourceResult(request));
        }
    }
}
