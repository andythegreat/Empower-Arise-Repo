using Arise.PublicAccess.Areas.ProviderApplication.Controllers;
using Empower.AccessControl;
using Empower.AccessControl.Helpers;
using Empower.Common;
using Empower.Common.CacheProviders;
using Empower.Common.Extensions;
using Empower.DomainService;
using Empower.Logging;
using Empower.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Linq.Translations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using static Empower.Common.Constant;

namespace Arise.PublicAccess.Controllers
{
    [Authorize]
    [TypeFilter(typeof(AccessControlAttribute))]
    public partial class BaseController : Controller
    {
        public const string ApplicationTitle = "Arise - ";
        public const string ProgramIdClaim = "ProgramID";

        public AccessControlManager AccessControlManager { get; }
        public ICacheProvider CacheProvider { get; }
        public ProviderDomainService ProviderDomainService { get; }
        public QualityRatingDomainService QualityRatingDomainService { get; }
        
        public ILogger Logger { get; }
        public QrisApplicationDomainService QrisApplicationDomainService { get; }

        public int UserID => int.Parse(User.FindFirstValue(Authentication.Claim.UserID) ?? "0");

        public string UserName => User.Identity.Name;

        public string UserRole => User.FindFirstValue(ClaimTypes.Role);

        public int UserProgramID => int.Parse(User.FindFirstValue(ProgramIdClaim) ?? "0");

        public BaseController(
            ProviderDomainService providerDomainService,
            ILogger logger,
            AccessControlManager accessControlManager,
            ICacheProvider cacheProvider)
        {
            ProviderDomainService = providerDomainService;
            Logger = logger;
            AccessControlManager = accessControlManager;
            CacheProvider = cacheProvider;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (RouteData.Values["appId"] != null)
            {
                ProviderDomainService.FacilityApplicationID = int.Parse((string)RouteData.Values["appId"]);
                var appStatusID = ProviderDomainService.Repository.PA_Applications.Where(pa => pa.ID == ProviderDomainService.FacilityApplicationID).Single().ApplicationStatusID;
                
                if(appStatusID != Empower.Model.LookupIDs.ApplicationStatuses.Pending)
                {
                     RedirectToAction(nameof(SummaryController.Index), nameof(SummaryController).RemoveControllerFromName(), new { area = Constant.PublicAccessApp.Modules.ProviderApplication.Area });
                }

                ViewBag.FacilityApplicationID = ProviderDomainService.FacilityApplicationID;
            }

            if (User.Identity.IsAuthenticated)
            {
                var provider = ProviderDomainService.Repository.Providers.Where(p => p.User.UserName == UserName)
                            .Select(p => new
                            {
                                ID = p.ID,
                                ProviderKey = p.ProviderKey,
                                DisplayName = p.DisplayName,
                                ProviderTypeName = p.ProviderType.Name,
                                ProviderStatusName = p.ProviderStatus.Name,
                                PermitStatusName = p.PermitStatusText,
                                PermitStatusID = p.LatestPermit == null ? 0 : p.LatestPermit.PermitStatusID,
                                IsCEPS = p.ProviderProgramParticipations.Where(ppp => ppp.ProgramTypeID == Empower.Model.LookupIDs.ProgramTypes.CEPS && !ppp.InactiveDate.HasValue).Any(),
                                IsCCAR = p.ORS || p.ProviderProgramParticipations.Where(ppp => ppp.ProgramTypeID == Empower.Model.LookupIDs.ProgramTypes.CCAR && !ppp.InactiveDate.HasValue).Any(),
                                PasswordSet = p.User.PasswordSet,
                            })
                            .AsNoTracking()
                            .WithTranslations()
                            .FirstOrDefault();

                if (provider != null)
                {
                    ProviderDomainService.ProviderID = provider.ID;
                    ProviderDomainService.PermitStatusID = provider.PermitStatusID;
                    ProviderDomainService.IsCEPS = provider.IsCEPS;
                    ProviderDomainService.IsCCAR = provider.IsCCAR;

                    ViewBag.ProviderName = provider.DisplayName;
                    ViewBag.ProviderDisplayName = ViewBag.ProviderName;

                    if (provider.DisplayName.Length >= 20)
                    {
                        var tmpName = provider.DisplayName;
                        tmpName = tmpName.Substring(0, 20) + "...";
                        ViewBag.ProviderDisplayName = tmpName;
                    }

                    ViewBag.ProviderSubmenuText = (String.IsNullOrEmpty(provider.ProviderKey) ? String.Empty : provider.ProviderKey) + " | " + provider.ProviderTypeName;
                    ViewBag.ProviderStatus = provider.ProviderStatusName;
                    ViewBag.PermitStatus = provider.PermitStatusName;
                }
            }

            ViewBag.ShowLeftNav = true;
            ViewBag.ShowNotes = true;
            ViewBag.ShowContentHeader = true;

            var user = User.Identity.Name;
            var address = HttpContext.Connection.RemoteIpAddress.ToString();
            var agent = Request.Headers["User-Agent"];
            var url = Request.Path;

            var area = filterContext.RouteData.Values["area"]?.ToString().SplitCamelCase();

            var controller = string.Empty;
            var controllerActionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                controller = controllerActionDescriptor.ControllerName;
            }

            var action = filterContext.ActionDescriptor.DisplayName;

            Logger.Log("OnActionExecuting", LoggingLevel.Debug, user, agent, address, url, area, controller, action);

            ViewBag.Title = ApplicationTitle + area + " " + ViewBag.PageTitle;
            ViewBag.ShowUserDetails = true;

            if (Request.HasFormContentType && Request.Form.ContainsKey("navigate:Cancel"))
            {
                var routeData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Request.Form["navigate:CancelRoute"]);
                filterContext.Result = RedirectToAction((string)routeData["action"], (string)routeData["controller"], new RouteValueDictionary(routeData));
            }

            base.OnActionExecuting(filterContext);
        }

        private void SetPermissions(dynamic viewBag, string area, string controller, string action)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User;
                viewBag.CanEdit = AccessControlManager.CheckAccess(user, string.Format("{0}.{1}.{2}", area, controller, action), Actions.Edit);
            }
            else
            {
                viewBag.CanEdit = AccessControlManager.CheckAccess(null, string.Format("{0}.{1}.{2}", area, controller, action), Actions.Edit);
            }
        }

        protected bool IsAjax
        {
            get
            {
                return Request != null && Request.Headers != null && Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            }
        }

        #region Convenience Return Methods

        // returns Javascript result
        public IActionResult JavaScript(string script)
        {
            return Content(script, Constant.MimeType.Javascript);
        }

        // returns preserved Json result, eg. without changing casing convention of variable names
        public JsonResult PreservedJson(object data)
        {
            return Json(data, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        }

        public IActionResult InternalServerError()
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        #endregion
    }
}