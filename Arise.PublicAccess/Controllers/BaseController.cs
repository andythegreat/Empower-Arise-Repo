﻿using Arise.PublicAccess.Areas.ProviderApplication.Controllers;
using Empower.AccessControl;
using Empower.AccessControl.Helpers;
using Empower.Common.CacheProviders;
using Empower.Common.Extensions;
using Empower.DomainService;
using Empower.Logging;
using Empower.Model;
using Microsoft.AspNetCore.Authorization;
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

        public const string JavascriptHeader = "application/javascript";


        private ILogger _logger;
        private ICacheProvider _cache;
        private AccessControlManager _acm;

        //private IDomainService _domainService;

        private ProviderDomainService _domainService;
        //private ReimbursementDomainService _reimbursementDomainService;
        //private AttendanceDomainService _attendanceDomainService;

        protected ProviderDomainService ProviderDomainService
        {
            get { return _domainService; }
        }

        //protected ReimbursementDomainService ReimbursementDomainService
        //{
        //    get { return _reimbursementDomainService; }
        //}

        //protected AttendanceDomainService AttendanceDomainService
        //{
        //    get { return _attendanceDomainService; }
        //}

        public AccessControlManager AccessControlManager
        {
            get { return _acm; }
        }

        public ILogger Logger
        {
            get { return _logger; }
        }

        public ICacheProvider Cache
        {
            get { return _cache; }
        }

        public int UserID => int.Parse(User.FindFirstValue(Authentication.Claim.UserID) ?? "0");

        public string UserName => User.Identity.Name;

        public string UserRole => User.FindFirstValue(ClaimTypes.Role);

        public int UserProgramID => int.Parse(User.FindFirstValue(ProgramIdClaim) ?? "0");

        private List<Models.Old_TreeViewItemModel> GetLeftNavItems()
        {
            var profileItems = new List<Models.Old_TreeViewItemModel>();
            profileItems.Add(new Models.Old_TreeViewItemModel { Text = "Messages and Notification", Action = "Index", Controller = "Home", ClientID = "lnkHome" });
            profileItems.Add(new Models.Old_TreeViewItemModel { Text = "Provider Profile", Action = "Index", Controller = "ProviderProfile", ClientID = "lnkProviderProfile" });
            
            if (_domainService.PermitStatusID == Empower.Model.LookupIDs.PermitStatuses.Issued)
            {
                profileItems.Add(new Models.Old_TreeViewItemModel { Text = "License Status", Action = "Index", Controller = "PermitStatus", ClientID = "lnkPermitStatus" });
            }

            profileItems.Add(new Models.Old_TreeViewItemModel { Text = "Application", Action = "Index", Controller = "Home", Area = Empower.Common.Constant.PublicAccessApp.Modules.ProviderApplication, ClientID = "lnkApplication" });
            
            if (ProviderDomainService.IsCEPS)
            {
                profileItems.Add(new Models.Old_TreeViewItemModel { Text = "Payments", Action = "Index", Controller = "FeePayment", ClientID = "lnkPayment" });
            }

            var reportingItems= new List<Models.Old_TreeViewItemModel>();
            reportingItems.Add(new Models.Old_TreeViewItemModel { Text = "Waivers", Action = "", Controller = "", ClientID = "" });
            reportingItems.Add(new Models.Old_TreeViewItemModel { Text = "Incident Reports", Action = "Index", Controller = "AccountIncidentReport", ClientID = "lnkAccountIncedentReport" });
            
            var attendanceItems = new List<Models.Old_TreeViewItemModel>();
            attendanceItems.Add(new Models.Old_TreeViewItemModel { Text = "Attendance", Action = "Index", Controller = "Attendance", ClientID = "lnkAttendance" });
            attendanceItems.Add(new Models.Old_TreeViewItemModel { Text = "Adjustments", Action = "Index", Controller = "Adjustment", ClientID = "lnkAdjustment" });

            var reimbursementItems = new List<Models.Old_TreeViewItemModel>();
            
            if (ProviderDomainService.IsCCAR)
            {
                reimbursementItems.Add(new Models.Old_TreeViewItemModel { Text = "Reimbursements ", Action = "Index", Controller = "ReimbursementSummary", ClientID = "lnkReimbursementSummary" });
                reimbursementItems.Add(new Models.Old_TreeViewItemModel { Text = "Children Enrollment", Action = "Index", Controller = "CCAREnrollments", ClientID = "lnkCCAREnrollments" });
            }

            var ProviderRegulatoryAgency = ProviderDomainService.Repository.Providers.Include(p => p.ProviderRegulatoryAgency).Where(p => p.ID == ProviderDomainService.ProviderID).FirstOrDefault();
            
            var fmItems = new List<Models.Old_TreeViewItemModel>();
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Facilities", Action = "Index", Controller = "Facilities", ClientID = "#" });
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Staff Management", Action = "Index", Controller = "StaffManagement", ClientID = "lnkStaffManagemet" });
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Food Service Managment", Action = "Index", Controller = "FoodServiceManagment", ClientID = "lnkFoodServiceManagment" });

            if (ProviderRegulatoryAgency != null && ProviderRegulatoryAgency.ProviderRegulatoryAgencyID == Empower.Model.LookupIDs.ProviderRegulatoryAgencies.FairfaxCountyPermit)
            {
                fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Training Summary", Action = "Index", Controller = "ProviderTrainings", ClientID = "lnkTrainingSummary" });
            }

            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Certification and Accreditations", Action = "Accreditation", Controller = "ResourceAndReferral", ClientID = "lnkAccreditation" });
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Services", Action = "ChildCareServices", Controller = "ResourceAndReferral", ClientID = "lnkChildCareServices" });

            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Environment", Action = "Index", Controller = "EnvironmentManagement", ClientID = "lnkEnvironmentManagment" });
           
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Children's record", Action = "", Controller = "", ClientID = "#" });
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Add Class Room", Action = "Index", Controller = "ClassRoom", ClientID = "lnkClassRoom" });
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Staff Change Log History", Action = "Index", Controller = "StaffChangeHistory", ClientID = "lnkClassRoom" });

            if (ProviderDomainService.IsCEPS)
            {
                fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Inspections", Action = "Index", Controller = "ScheduledInspections", ClientID = "lnkScheduledInspection" });
            }

            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Private Market Rates", Action = "PrivateMarketRates", Controller = "ResourceAndReferral", ClientID = "lnkPrivateMarketRates" });
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "First Aid, CPR", Action = "Index", Controller = "FirstAidCPR", ClientID = "lnkFirstAidCPRController" });
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Equipment Maintenance Log", Action = "Index", Controller = "EquipmentMaintenanceLog", ClientID = "lnkEquipmentMaintenanceLog" });
            fmItems.Add(new Models.Old_TreeViewItemModel { Text = "Equipment Inspection Log", Action = "Index", Controller = "EquipmentInspectionLog", ClientID = "lnkEquipmentInspectionLog" });
            var qualityRatingItems = new List<Models.Old_TreeViewItemModel>();
            qualityRatingItems.Add(new Models.Old_TreeViewItemModel { Text = "QRIS Pages", Action = "Index", Controller = "QRISApplication", ClientID = "lnkQRISApplication" });

            var additionalItems = new List<Models.Old_TreeViewItemModel>();
            
            if (ProviderDomainService.IsCEPS)
            {
                additionalItems.Add(new Models.Old_TreeViewItemModel { Text = "Contact Us", Action = "CEPS", Controller = "RequestContact", ClientID = "lnkRequestContactCEPS" });
            }

            var treeViewItems = new List<Models.Old_TreeViewItemModel>();
            treeViewItems.Add(new Models.Old_TreeViewItemModel { Text = "Profile", Items = profileItems });
            
            if (ProviderDomainService.IsCCAR)
            {
                treeViewItems.Add(new Models.Old_TreeViewItemModel { Text = "Reporting", Items = reportingItems });
            }

            treeViewItems.Add(new Models.Old_TreeViewItemModel { Text = "Attendance", Items = attendanceItems });
            treeViewItems.Add(new Models.Old_TreeViewItemModel { Text = "Reimbursement", Items = reimbursementItems });
            treeViewItems.Add(new Models.Old_TreeViewItemModel { Text = "Facility Management", Items = fmItems });
            treeViewItems.Add(new Models.Old_TreeViewItemModel { Text = "Quality Rating  ", Items = qualityRatingItems });
            treeViewItems.Add(new Models.Old_TreeViewItemModel { Text = "Additional", Items = additionalItems });
            
            return treeViewItems;
        }

        public BaseController(ProviderDomainService domainService, Empower.Logging.ILogger logger, AccessControlManager accessControlManager, ICacheProvider cacheProvider)
        {
            //_domainService = container.Resolve<ProviderDomainService>();
            //_reimbursementDomainService = container.Resolve<ReimbursementDomainService>();
            //_attendanceDomainService = container.Resolve<AttendanceDomainService>();
            _domainService = domainService;

            //_acm = container.Resolve<AccessControlManager>();
            //_logger = container.Resolve<ILogger>();
            //_cache = container.Resolve<ICacheProvider>();
            _acm = accessControlManager;
            _logger = logger;
            _cache = cacheProvider;
        }

        //protected override void OnAuthorization(AuthorizationContext filterContext)
        //{
        //    if (!User.Identity.IsAuthenticated && System.Configuration.ConfigurationManager.AppSettings["OverrideUser"] != null)
        //    {
        //        var loginResults = _acm.Authenticate(System.Configuration.ConfigurationManager.AppSettings["OverrideUser"], "");
        //        var identity = loginResults.ClaimsPrincipal.Identity as ClaimsIdentity;
        //        HttpContext.GetOwinContext().Authentication.SignIn(new Microsoft.Owin.Security.AuthenticationProperties() { IsPersistent = false }, identity);
        //        HttpContext.User = loginResults.ClaimsPrincipal;
        //    }

        //    string area = filterContext.RouteData.DataTokens["area"] != null ? filterContext.RouteData.DataTokens["area"].ToString().SplitCamelCase().Split(' ').First() : null;
        //    string controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
        //    string action = filterContext.ActionDescriptor.ActionName;

        //    if (area != null)
        //    {
        //        if (User.Identity.IsAuthenticated)
        //        {
        //            var user = User as ClaimsPrincipal;

        //            var authorized = _acm.CheckAccess(user, area, Actions.View);

        //            if (!authorized)
        //            {
        //                _logger.Log(string.Format("User: {0} attempted to access resource: {1}.", UserName, Request.Url), LoggingLevel.Warn);
        //                filterContext.Result = new ViewResult { ViewName = "Unauthorized", ViewData = this.ViewData };
        //            }
        //        }
        //    }

        //    SetPermissions(this.ViewBag, area, controller, action);
        //    base.OnAuthorization(filterContext);
        //}

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if (filterContext.RouteData.Values.ContainsKey("action") && filterContext.RouteData.Values["action"].ToString() == "Register")
            //{
            //}
            //else if ((UserName == null || HttpContext.Session.IsAvailable) && (System.Configuration.ConfigurationManager.AppSettings["OverrideUser"] == null))
            //{
            //    //Session.RemoveAll();
            //    //Session.Clear();
            //    HttpContext.Session.Clear();

            //    var controllerDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;

            //    if (controllerDescriptor.ActionName.ToLower() != "login"
            //        && controllerDescriptor.ActionName.ToLower() != "forgotpassword"
            //        && controllerDescriptor.ActionName.ToLower() != "sessionverification"
            //        && controllerDescriptor.ActionName.ToLower() != "sessionnotification"
            //        && controllerDescriptor.ActionName.ToLower() != "testpayment")
            //    {
            //        filterContext.Result = RedirectToAction("Login", "Account", new
            //        {
            //            returnUrl = "/" + controllerDescriptor.ControllerName + "/" +
            //                          controllerDescriptor.ActionName
            //        });
            //    }
            //}
            //else
            //{
            if (RouteData.Values["appId"] != null)
            {
                ProviderDomainService.FacilityApplicationID = int.Parse((string)RouteData.Values["appId"]);
                var appStatusID = ProviderDomainService.Repository.PA_Applications.Where(pa => pa.ID == ProviderDomainService.FacilityApplicationID).Single().ApplicationStatusID;
                
                if(appStatusID != Empower.Model.LookupIDs.ApplicationStatuses.Pending)
                {
                     RedirectToAction(nameof(SummaryController.Index), nameof(SummaryController).RemoveControllerFromName(), new { area = Empower.Common.Constant.PublicAccessApp.Modules.ProviderApplication});
                }

                ViewBag.FacilityApplicationID = ProviderDomainService.FacilityApplicationID;
            }

            if (User.Identity.IsAuthenticated)
            {
                var provider = _domainService.Repository.Providers.Where(p => p.User.UserName == UserName)
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
                    _domainService.ProviderID = provider.ID;
                    _domainService.PermitStatusID = provider.PermitStatusID;
                    _domainService.IsCEPS = provider.IsCEPS;
                    _domainService.IsCCAR = provider.IsCCAR;

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
            //}

            ViewBag.ShowLeftNav = true;
            ViewBag.LeftNavItems = GetLeftNavItems();
            ViewBag.ShowNotes = true;
            ViewBag.ShowContentHeader = true;

            //string user = User.Identity.IsAuthenticated ? UserName : " ";
            //string address = Request.UserHostAddress;
            //string agent = Request.UserAgent;
            //string url = Request.Url.AbsoluteUri;
            var user = User.Identity.Name;
            var address = HttpContext.Connection.RemoteIpAddress.ToString();
            var agent = Request.Headers["User-Agent"];
            var url = Request.Path;

            //string area = filterContext.RouteData.DataTokens["area"] != null ? filterContext.RouteData.DataTokens["area"].ToString().SplitCamelCase() : "";
            //string controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            //string action = filterContext.ActionDescriptor.ActionName;
            var area = filterContext.RouteData.Values["area"]?.ToString().SplitCamelCase();

            var controller = string.Empty;
            var controllerActionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                controller = controllerActionDescriptor.ControllerName;
            }

            var action = filterContext.ActionDescriptor.DisplayName;

            _logger.Log("OnActionExecuting", LoggingLevel.Debug, user, agent, address, url, area, controller, action);

            ViewBag.Title = ApplicationTitle + area + " " + ViewBag.PageTitle;
            ViewBag.ShowUserDetails = true;

            if (Request.HasFormContentType && Request.Form.ContainsKey("navigate:Cancel"))
            {
                var routeData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Request.Form["navigate:CancelRoute"]);
                filterContext.Result = RedirectToAction((string)routeData["action"], (string)routeData["controller"], new RouteValueDictionary(routeData));
            }

            base.OnActionExecuting(filterContext);
        }

        //protected override void OnException(ExceptionContext filterContext)
        //{
        //    base.OnException(filterContext);

        //    string user = User.Identity.IsAuthenticated ? UserName : "";
        //    string address = Request.UserHostAddress;
        //    string agent = Request.UserAgent;
        //    string url = Request.Url.AbsoluteUri;
        //    string area = filterContext.RouteData.DataTokens["area"] != null ? filterContext.RouteData.DataTokens["area"].ToString().SplitCamelCase() : "";

        //    _logger.Log("", LoggingLevel.Error, filterContext.Exception, user, agent, address, url, area, null, null);

        //    var vr = filterContext.Result as ViewResult;

        //    if (vr != null)
        //    {
        //        SetPermissions(vr.ViewBag, null, null, null);
        //    }
        //}

        private void SetPermissions(dynamic viewBag, string area, string controller, string action)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User as ClaimsPrincipal;
                viewBag.CanEdit = _acm.CheckAccess(user, string.Format("{0}.{1}.{2}", area, controller, action), Actions.Edit);
            }
            else
            {
                viewBag.CanEdit = _acm.CheckAccess(null, string.Format("{0}.{1}.{2}", area, controller, action), Actions.Edit);
            }
        }

        protected bool IsAjax
        {
            get
            {
                return Request != null && Request.Headers != null && Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            }
        }

        public IActionResult JavaScript(string script)
        {
            return Content(script, JavascriptHeader);
        }

        public JsonResult PreservedJson(object data)
        {
            return Json(data, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
        }
    }
}