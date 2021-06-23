using Arise.PublicAccess.Models;
using Empower.AccessControl;
using Empower.Common.CacheProviders;
using Empower.DomainService;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Kendo.Mvc.Extensions;
using Empower.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Arise.Shared.CoreUI.Helpers;
using Microsoft.Linq.Translations;

namespace Arise.PublicAccess.Controllers
{
    public class EnvironmentManagmentController : BaseController
    {
        public EnvironmentManagmentController(ProviderDomainService providerDomainService,
           Empower.Logging.ILogger logger, AccessControlManager accessControlManager, ICacheProvider cacheProvider)
           : base(providerDomainService, logger, accessControlManager, cacheProvider)
        {
        }

        public IActionResult Index()
        {
            var viewModel = new EnvironmentManagmentViewModel();
            viewModel.FacilityList = (from ap in ProviderDomainService.Repository.PA_Applications
                                      join fa in ProviderDomainService.Repository.PA_FacilityInformations on ap.FacilityID equals fa.FacilityID
                                      select new SelectListItem
                                      {
                                          Value = fa.FacilityID.ToString(),
                                          Text = fa.FacilityName
                                      }).ToList();
            return View(viewModel);
        }
        public IActionResult Get_FacilityList([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
            var resultdate = (from f in ProviderDomainService.Repository.PA_FacilityInformations
                              join s in ProviderDomainService.Repository.PA_Staffs on f.FacilityID equals s.FacilityID
                              join p in ProviderDomainService.Repository.PA_People on s.PersonID equals p.ID
                              select new
                              {
                                  FacilityID = f.FacilityID,
                                  FacilityName = f.FacilityName,
                                  DirectorName = p.FullName
                              }).WithTranslations();

            if (facilityID > 0)
            {
                resultdate = resultdate.Where(s => s.FacilityID == facilityID);
            }
            return Json(resultdate.ToDataSourceResult(request));
        }

        [HttpGet]
        public IActionResult Edit(int facilityID)
        {
            var vmEnvironment = new EnvironmentManagmentViewModel();
            vmEnvironment.FacilityID = facilityID;
            vmEnvironment.EquipmentTypeList = ProviderDomainService.Repository.GetBindToItems<EquipmentType>().ToList();
            return View(vmEnvironment);
        }
        public IActionResult Get_Material([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
            var vmEnvironment = (from fe in ProviderDomainService.Repository.PA_FacilityEnvironments
                                 where fe.FacilityID == facilityID && !fe.IsDeleted
                                 select new
                                 {
                                     ID = fe.ID,
                                     FacilityID = fe.FacilityID,
                                     EquipmentTypeID = fe.EquipmentTypeID,
                                     EquipmentDescription = fe.EquipmentDescription,
                                     EquipmentCount = fe.EquipmentCount,
                                 });
            return Json(vmEnvironment.ToDataSourceResult(request));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Create_Material([DataSourceRequest] DataSourceRequest request, PA_FacilityEnvironment objVM)
        {
            ProviderDomainService.Repository.Add(objVM);
            ProviderDomainService.Save();
            return Json(new[] { objVM }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Update_Material([DataSourceRequest] DataSourceRequest request, PA_FacilityEnvironment objVM)
        {
            var obj_Environment = new PA_FacilityEnvironment();
            TryUpdateModelAsync<PA_FacilityEnvironment>(obj_Environment);
            ProviderDomainService.Repository.Update(obj_Environment, objVM.ID);
            ProviderDomainService.Save();
            return Json(new[] { objVM }.ToDataSourceResult(request, ModelState));
        }

        public IActionResult Delete_Material([DataSourceRequest] DataSourceRequest request, EnvironmentManagmentViewModel objVM)
        {
            var obj = ProviderDomainService.Repository.PA_FacilityEnvironments
                                     .Where(c => c.ID == objVM.ID).FirstOrDefault();
            obj.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { objVM }.ToDataSourceResult(request));
        }
    }
}
