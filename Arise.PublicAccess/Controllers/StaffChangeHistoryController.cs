using Arise.PublicAccess.Models;
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
using System.Linq;
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
            StaffChangeLogHistoryViewModel staffChangeLogHistoryViewModel = new StaffChangeLogHistoryViewModel();
            staffChangeLogHistoryViewModel.ClassIDs = (from f in ProviderDomainService.Repository.ClassRoom
                                                       select new SelectListItem
                                                       {
                                                           Value = f.ID.ToString(),
                                                           Text = f.ClassRoomName
                                                       }).ToList();

            staffChangeLogHistoryViewModel.StaffIDs = (from p in ProviderDomainService.Repository.Persons
                                                       join s in ProviderDomainService.Repository.StaffMembers
                                                       on p.ID equals s.PersonID
                                                       join sc in ProviderDomainService.Repository.StaffCharacteristics
                                                       on s.ID equals sc.StaffID
                                                       where s.FacilityTypeID == 0
                                                       select new SelectListItem
                                                       {
                                                           Value = p.ID.ToString(),
                                                           Text = p.FullName
                                                       }).WithTranslations().ToList();

            return View(staffChangeLogHistoryViewModel);
        }

        public IActionResult StaffTransfer()
        {
            StaffChangeLogHistoryViewModel staffChangeLogHistoryViewModel = new StaffChangeLogHistoryViewModel();
            staffChangeLogHistoryViewModel.ClassIDs= (from f in ProviderDomainService.Repository.ClassRoom
                                                      select new SelectListItem
                                                      {
                                                          Value = f.ID.ToString(),
                                                          Text = f.ClassRoomName
                                                      }).ToList();

            staffChangeLogHistoryViewModel.StaffIDs = (from p in ProviderDomainService.Repository.Persons
                                                      join s in ProviderDomainService.Repository.StaffMembers
                                                      on p.ID equals s.PersonID
                                                      join sc in ProviderDomainService.Repository.StaffCharacteristics
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
            var objStaffData = (from sc in ProviderDomainService.Repository.ClassRoom
                                     join st in ProviderDomainService.Repository.StaffClassRoom
                                     on sc.ID equals st.ClassRoomID
                                     where sc.IsDeleted !=true
                                     select new StaffChangeLogHistoryViewModel{
                                      StaffID =st.StaffID,
                                      ClassRoomID = sc.ID
                                     }).ToList();

            return Json(objStaffData.ToDataSourceResult(request));
        }
        public IActionResult CreateStaffChangeLog([DataSourceRequest] DataSourceRequest request, PA_StaffChangeLogHistory pA_StaffChangeLogHistory)
        {
            var staffClassRoomData = ProviderDomainService.Repository.StaffClassRoom.Where(x => x.StaffID == pA_StaffChangeLogHistory.StaffID && x.ClassRoomID == pA_StaffChangeLogHistory.ClassRoomID).FirstOrDefault();
            staffClassRoomData.ClassRoomID = pA_StaffChangeLogHistory.NewClassRoomID;
            staffClassRoomData.StaffID = pA_StaffChangeLogHistory.NewStaffID;
            ProviderDomainService.Save();
            pA_StaffChangeLogHistory.CreatedDate = DateTime.Now;
            ProviderDomainService.Repository.Add(pA_StaffChangeLogHistory);
            ProviderDomainService.Save();
            return Json(new[] { pA_StaffChangeLogHistory }.ToDataSourceResult(request));
        }
        public IActionResult GetStaffChangeLogHistory([DataSourceRequest] DataSourceRequest request)
        {
            var objStaffDataChangeLodData = (from sc in ProviderDomainService.Repository.StaffChangeLogHistory
                                             select new StaffChangeLogHistoryViewModel
                                             {
                                                 ClassRoomID= sc.ClassRoomID,
                                                 OldStaffID =sc.StaffID,
                                                 NewStaffID =sc.NewStaffID,
                                                 CreatedDate =sc.CreatedDate
                                             }).ToList(); // i have getting errro when i dont used .ToList()
            return Json(objStaffDataChangeLodData.ToDataSourceResult(request));
        }
    }
}
