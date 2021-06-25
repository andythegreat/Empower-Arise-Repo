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
    public class EnvironmentManagementController : BaseController
    {
        public EnvironmentManagementController(ProviderDomainService providerDomainService,
           Empower.Logging.ILogger logger, AccessControlManager accessControlManager, ICacheProvider cacheProvider)
           : base(providerDomainService, logger, accessControlManager, cacheProvider)
        {
        }

        public IActionResult Index()
        {
            var viewModel = new EnvironmentManagementViewModel();
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
            var resultdate = (from f in ProviderDomainService.Repository.PA_Facilities
                              join fi in ProviderDomainService.Repository.PA_FacilityInformations on f.ID equals fi.FacilityID
                              join sty in ProviderDomainService.Repository.FacilityTypes on f.FacilityTypeID equals sty.ID
                              join stt in ProviderDomainService.Repository.StaffTypes on sty.ID equals stt.ProviderTypeID
                              where stt.Name == "Director"
                              select new
                              {
                                  fi,
                                  stt
                              }).ToList();

            var gridValues = (from recd in resultdate.ToList()
                              select new
                              {
                                  FacilityID = recd.fi.FacilityID,
                                  FacilityName = recd.fi.FacilityName,
                                  DirectorName = string.Join(", ", (from sft in ProviderDomainService.Repository.PA_Staffs
                                                                    join stc in ProviderDomainService.Repository.PA_StaffCharacteristics on sft.ID equals stc.StaffID
                                                                    join p in ProviderDomainService.Repository.PA_People on sft.PersonID equals p.ID
                                                                    where stc.TitleOfPosition == recd.stt.ID && sft.FacilityID == recd.fi.FacilityID
                                                                    select (p.FullName.ToString())).WithTranslations())
                              }
                                );

            if (facilityID > 0)
            {
                gridValues = gridValues.Where(s => s.FacilityID == facilityID);
            }
            return Json(gridValues.ToDataSourceResult(request));
        }

        [HttpGet]
        public IActionResult Edit(int facilityID)
        {
            var vmEnvironment = new EnvironmentManagementViewModel();
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
        public ActionResult Create_Material([DataSourceRequest] DataSourceRequest request, PA_FacilityEnvironment objFacilityEnvironment)
        {
            ProviderDomainService.Repository.Add(objFacilityEnvironment);
            ProviderDomainService.Save();
            return Json(new[] { objFacilityEnvironment }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public ActionResult Update_Material([DataSourceRequest] DataSourceRequest request, PA_FacilityEnvironment objFacilityEnvironment)
        {
            var obj_Environment = new PA_FacilityEnvironment();
            TryUpdateModelAsync<PA_FacilityEnvironment>(obj_Environment);
            ProviderDomainService.Repository.Update(obj_Environment, objFacilityEnvironment.ID);
            ProviderDomainService.Save();
            return Json(new[] { objFacilityEnvironment }.ToDataSourceResult(request, ModelState));
        }

        public IActionResult Delete_Material([DataSourceRequest] DataSourceRequest request, EnvironmentManagementViewModel objFacilityEnvironment)
        {
            var obj = ProviderDomainService.Repository.PA_FacilityEnvironments
                                     .Where(c => c.ID == objFacilityEnvironment.ID).FirstOrDefault();
            obj.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { objFacilityEnvironment }.ToDataSourceResult(request));
        }
    }
}
