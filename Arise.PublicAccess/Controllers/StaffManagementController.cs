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
        public IActionResult AddNewStaff()
        {
            StaffManagementViewModel staffManagementViewModel = new StaffManagementViewModel {
                ProviderTypeIDs = ProviderDomainService.Repository.GetBindToItems<ProviderType>().ToList(),
                InformationSourceIDs = ProviderDomainService.Repository.GetBindToItems<InformationSource>().ToList(),
            };
            return View(staffManagementViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewStaff([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("duplicate key"))
                        ModelState.AddModelError(nameof(FiscalYear.FiscalYearRange), "Fiscal Year already exists.");
                    else
                        ModelState.AddModelError("", "Error while updating the Fiscal Year.");
                }
            }

            return Json(new[] { staffManagementViewModel }.ToDataSourceResult(request, ModelState));
        }
    }
}
