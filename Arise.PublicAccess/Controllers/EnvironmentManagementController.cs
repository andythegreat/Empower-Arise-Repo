using Arise.PublicAccess.Models;
using Empower.AccessControl;
using Empower.Common.CacheProviders;
using Empower.DomainService;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Kendo.Mvc.Extensions;
using Empower.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            viewModel.FacilityList = (from app in ProviderDomainService.Repository.Applications
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
            return View(viewModel);
        }
        public IActionResult Get_FacilityList([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
            var resultdata = (from f in ProviderDomainService.Repository.Facilities
                              join sty in ProviderDomainService.Repository.FacilityTypes on f.FacilityTypeID equals sty.ID
                              join stt in ProviderDomainService.Repository.StaffTypes on sty.ID equals stt.ProviderTypeID
                              where stt.Name == nameof(Director)
                              select new
                              {
                                  f,
                                  stt
                              }).ToList();

            var gridValues = (from recd in resultdata.ToList()
                              select new
                              {
                                  FacilityID = recd.f.ID,
                                  FacilityName = recd.f.FacilityName,
                                  DirectorName = string.Join(", ", (from sft in ProviderDomainService.Repository.StaffMembers
                                                                    join stc in ProviderDomainService.Repository.StaffCharacteristics on sft.ID equals stc.StaffID
                                                                    join p in ProviderDomainService.Repository.Persons on sft.PersonID equals p.ID
                                                                    where stc.StaffTypeID == recd.stt.ID && sft.FacilityID == recd.f.ID
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
            var vmEnvironment = (from fe in ProviderDomainService.Repository.FacilityEnvironments
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
        public ActionResult Create_Material([DataSourceRequest] DataSourceRequest request, FacilityEnvironment objFacilityEnvironment)
        {
            ProviderDomainService.Repository.Add(objFacilityEnvironment);
            ProviderDomainService.Save();
            return Json(new[] { objFacilityEnvironment }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        public ActionResult Update_Material([DataSourceRequest] DataSourceRequest request, FacilityEnvironment objFacilityEnvironment)
        {
            var obj_Environment = new FacilityEnvironment();
            TryUpdateModelAsync<FacilityEnvironment>(obj_Environment);
            ProviderDomainService.Repository.Update(obj_Environment, objFacilityEnvironment.ID);
            ProviderDomainService.Save();
            return Json(new[] { objFacilityEnvironment }.ToDataSourceResult(request, ModelState));
        }

        public IActionResult Delete_Material([DataSourceRequest] DataSourceRequest request, EnvironmentManagementViewModel objFacilityEnvironment)
        {
            var obj = ProviderDomainService.Repository.FacilityEnvironments
                                     .Where(c => c.ID == objFacilityEnvironment.ID).FirstOrDefault();
            obj.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { objFacilityEnvironment }.ToDataSourceResult(request));
        }
    }
}
