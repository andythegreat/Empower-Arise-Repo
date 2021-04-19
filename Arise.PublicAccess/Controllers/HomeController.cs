using Empower.AccessControl;
using Empower.Common.CacheProviders;
using Empower.DomainService;
using Empower.Messaging;
using Empower.Model;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Linq.Translations;
using System;
using System.Linq;

namespace Arise.PublicAccess.Controllers
{
    public class HomeController : BaseController
    {
        private MessagingService _messagingService;

        public HomeController(MessagingService messagingService, ProviderDomainService domainService, Empower.Logging.ILogger logger, AccessControlManager accessControlManager, ICacheProvider cacheProvider)
            : base(domainService, logger, accessControlManager, cacheProvider)
        {
            _messagingService = messagingService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AboutUs()
        {
            return View();
        }

        /// <summary>
        /// This is a Get method for Contact Fairfax.
        /// When user clicks on "Contact Fairfax" from Left Navigation, request comes here.
        /// this returns a view/interface Where user can view Fairfax Office address & contact details.
        /// </summary>
        /// <returns>Contact Fairfax View</returns>
        public ActionResult ContactUs()
        {
            var model = ProviderDomainService.Repository.FairfaxContacts.ToList();

            return View(model);
        }

        /// <summary>
        /// This is a Home Page for Provider Access Portal.
        /// When any Provider requests for Provider Access & Logs in, Request comes here.
        /// Provider can view Broadcast Messages here.
        /// </summary>
        /// <returns></returns>
        public ActionResult BroadcastMessages()
        {
            return View();
        }

        public ActionResult GetBroadcastMessages([DataSourceRequest] DataSourceRequest request)
        {
            var user = UserName;
            var providerInfo = ProviderDomainService.Set.Where(p => p.ID == ProviderDomainService.ProviderID)
                .Select(p => new {
                    IsCCAR = p.ProviderProgramParticipations.Where(ppp => ppp.ProgramTypeID == Empower.Model.LookupIDs.ProgramTypes.CCAR && (!ppp.InactiveDate.HasValue || DateTime.Today < ppp.InactiveDate)).Any(),
                    IsCEPS = p.ProviderProgramParticipations.Where(ppp => ppp.ProgramTypeID == Empower.Model.LookupIDs.ProgramTypes.CEPS && (!ppp.InactiveDate.HasValue || DateTime.Today < ppp.InactiveDate)).Any(),
                    p.MainAddress.Zip
                }).WithTranslations().FirstOrDefault();

            var results = _messagingService.GetBroadcastMessages(providerInfo.IsCCAR, providerInfo.IsCEPS, providerInfo.Zip);
            return Json(results.ToDataSourceResult(request));
        }

        public ActionResult ProviderMessages([DataSourceRequest] DataSourceRequest request)
        {
            var results = _messagingService.GetProviderMessages(UserName, nameof(Provider))
                            .Where(m => !m.DueDate.HasValue && m.MessageStatusID != Empower.Model.LookupIDs.MessageStatuses.Cleared && m.MessageStatusID != Empower.Model.LookupIDs.MessageStatuses.Expired);

            return Json(results.ToDataSourceResult(request));
        }

        [HttpPost]
        public void MarkCleared(int id)
        {
            _messagingService.ClearMessage(id, UserName);
        }

        [HttpPost]
        public void IncreaseSeverity(int id)
        {
            _messagingService.IncreaseSeverity(id);
        }

        [HttpPost]
        public void DecreaseSeverity(int id)
        {
            _messagingService.DecreaseSeverity(id);
        }
    }
}
