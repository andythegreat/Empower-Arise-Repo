using Arise.PublicAccess.Controllers;
using Arise.PublicAccess.Helpers;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arise.PublicAccess.Areas.ProviderApplication.Controllers
{
    public class StaffManagementController : BaseController
    {
        private MessagingService _messagingService;

        public StaffManagementController(MessagingService messagingService, ProviderDomainService domainService,
            Empower.Logging.ILogger logger, AccessControlManager accessControlManager, ICacheProvider cacheProvider)
            : base(domainService, logger, accessControlManager, cacheProvider)
        {
            _messagingService = messagingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AddNewStaff()
        {
            StaffManagementViewModel staffManagementViewModel = new StaffManagementViewModel {
                ProviderTypeIDs = ProviderDomainService.Repository.GetBindToItems<ProviderType>().ToList(),
                InformationSourceIDs = ProviderDomainService.Repository.GetBindToItems<InformationSource>().ToList(),
            };
            return View(staffManagementViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewStaff(StaffManagementViewModel staffManagementViewModel)
        {

            if (ModelState.IsValid)
            {
                try
                {

                }
                catch (Exception ex)
                { 
                }
            }

            return View(staffManagementViewModel);
        }
    }
}
