using Microsoft.Linq.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using Empower.DAL;
using Empower.Logging;
using Empower.Messaging;
using Empower.Model;
using Empower.Model.LookupIDs;

namespace Empower.DomainService
{
    public class ProviderDomainService : DomainServiceQCBase<Provider>
    {
        private MessagingService _messagingService;
        public int ProviderID { get; set; }
        public int PermitStatusID { get; set; }
        public bool IsCEPS { get; set; }
        public bool IsCCAR { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderDomainService"/> class.
        /// </summary>
        /// <param name="container">The Unity container.</param>
        public ProviderDomainService(IRepository repository, ILogger logger, MessagingService messagingService)
            : base(repository, logger)
        {
            //_messagingService = container.Resolve<CCMSIS.Messaging.MessagingService>();
            _messagingService = messagingService;
        }

        /// <summary>
        /// Saves the provider child care services.
        /// </summary>
        /// <param name="providerChildCareServices">The provider child care services.</param>
        /// <param name="providerId">The provider identifier.</param>
        /// <param name="isMeal">if set to <c>true</c> if is meal.</param>
        public void SaveProviderChildCareServices(string[] providerChildCareServices, int providerId, bool isMeal)
        {
            var isModifiedRecords = false;
            var providerCCS =
                 this.Repository.ProviderChildCareServiceses.Where(x => x.ProviderID == providerId && x.ChildCareService.IsMeal == isMeal).ToList();
            var providerCCS1 =
                this.Repository.ProviderChildCareServiceses.Where(x => x.ProviderID == providerId && x.ChildCareService.IsMeal == isMeal)
                    .Select(x => new { ChildCareServiceId = x.ChildCareServiceID })
                    .ToArray();

            if (providerChildCareServices == null)
            {
                providerChildCareServices = new string[0]; 
            }

            //delete items from ProviderChildCareServices table
            var itemsToDelete =
                providerCCS.Where(p => !providerChildCareServices.Contains(p.ChildCareServiceID.ToString()))
                    .ToList();

            foreach (var item in itemsToDelete)
            {
                this.Repository.Delete(item);
                isModifiedRecords = true;
            }

            //add items to ProviderChildCareServices table.
            var itemToAdd = providerChildCareServices.Except(providerCCS1.Select(p => p.ChildCareServiceId.ToString())).ToList();

            foreach (var item in itemToAdd)
            {
                var providerChildCare = new ProviderChildCareServices()
                {
                    ChildCareServiceID = Convert.ToInt32(item),
                    ProviderID = providerId
                };

                this.Repository.Add(providerChildCare);
                isModifiedRecords = true;
            }
            if (isModifiedRecords)
            {
                this.Save();
            }

        }

        /// <summary>
        /// Saves the provider special services.
        /// </summary>
        /// <param name="providerSpecialServices">The provider special services.</param>
        /// <param name="providerId">The provider identifier.</param>
        public void SaveProviderSpecialServices(string[] providerSpecialServices, int providerId)
        {
            var _providerSpecialServices =
                this.Repository.ProviderSpecialServiceses.Where(x => x.ProviderID == providerId).ToList();

            var _providerSSIds =
                this.Repository.ProviderSpecialServiceses.Where(x => x.ProviderID == providerId)
                    .Select(x => new { SpecialServiceId = x.SpecialServiceID })
                    .ToArray();

            if (providerSpecialServices == null)
                providerSpecialServices = new string[0];

            //delete items from ProviderChildCareServices table
            var itemsToDelete =
                _providerSpecialServices.Where(
                    p => !providerSpecialServices.Contains(p.SpecialServiceID.ToString())).ToList();

            foreach (var item in itemsToDelete)
            {
                this.Repository.Delete(item);
            }

            //add items to ProviderChildCareServices table.
            var itemToAdd =
                providerSpecialServices.Except(_providerSSIds.Select(p => p.SpecialServiceId.ToString())).ToList();
            foreach (var item in itemToAdd)
            {
                var objProviderSpecialService = new ProviderSpecialServices()
                {
                    SpecialServiceID = Convert.ToInt32(item),
                    ProviderID = providerId
                };

                this.Repository.Add(objProviderSpecialService);
            }

            this.Save();
        }

        /// <summary>
        /// Saves the provider languages.
        /// </summary>
        /// <param name="providerLanguages">The provider languages.</param>
        /// <param name="OtherLanguage">The other language.</param>
        /// <param name="providerId">The provider identifier.</param>
        public void SaveProviderLanguages(string[] providerLanguages, string OtherLanguage, int providerId)
        {
            var _providerLanguages = this.Repository.ProviderLanguages.Where(x => x.ProviderID == providerId).ToList();

            var _providerLanguagesIds =
                this.Repository.ProviderLanguages.Where(x => x.ProviderID == providerId)
                    .Select(x => new { LanguageId = x.LanguageID })
                    .ToArray();

            if (providerLanguages == null)
            {
                providerLanguages = new string[0]; 
            }

            //delete items from ProviderChildCareServices table
            var itemsToDelete = _providerLanguages.Where(p => !providerLanguages.Contains(p.LanguageID.ToString())).ToList();

            foreach (var item in itemsToDelete)
            {
                this.Repository.Delete(item);
            }

            //add items to ProviderChildCareServices table.
            var itemToAdd = providerLanguages.Except(_providerLanguagesIds.Select(p => p.LanguageId.ToString())).ToList();
            foreach (var item in itemToAdd)
            {
                var objProviderLanguage = new ProviderLanguage()
                {
                    LanguageID = Convert.ToInt32(item),
                    ProviderID = providerId
                };

                if (Convert.ToInt32(item) == Languages.Other)
                {
                    objProviderLanguage.OtherLanguage = OtherLanguage; 
                }

                this.Repository.Add(objProviderLanguage);
            }

            if (OtherLanguage != "")
            {
                var ProviderOtherLanguage = this.Repository.ProviderLanguages.Where(x => x.ProviderID == providerId && x.LanguageID == Languages.Other).FirstOrDefault();
                if (ProviderOtherLanguage != null)
                {
                    if (ProviderOtherLanguage.OtherLanguage != OtherLanguage)
                    {
                        ProviderOtherLanguage.OtherLanguage = OtherLanguage; 
                    }
                }
            }
            

            this.Save();
        }

        /// <summary>
        /// Saves the provider profession exp.
        /// </summary>
        /// <param name="professionalExpIds">The professional exp ids.</param>
        /// <param name="providerId">The provider identifier.</param>
        public void SaveProviderProfessionExp(string[] professionalExpIds, int providerId)
        {
            var _providerProfessionalExp =
                this.Repository.ProviderProfessionalExps.Where(x => x.ProviderID == providerId).ToList();

            var _providerPEIds =
                this.Repository.ProviderProfessionalExps.Where(x => x.ProviderID == providerId)
                    .Select(x => new { ProfessionalExpID = x.ProfessionalExpID })
                    .ToArray();

            if (professionalExpIds == null)
            {
                professionalExpIds = new string[0]; 
            }

            //delete items from ProviderProfessioalExp table
            var itemsToDelete = _providerProfessionalExp.Where(p => !professionalExpIds.Contains(p.ProfessionalExpID.ToString())).ToList();

            foreach (var item in itemsToDelete)
            {
                this.Repository.Delete(item);
            }

            //add items to ProviderProfessionalExp table.
            var itemToAdd = professionalExpIds.Except(_providerPEIds.Select(p => p.ProfessionalExpID.ToString())).ToList();

            foreach (var item in itemToAdd)
            {
                var objProviderProfessionalExp = new ProviderProfessionalExp()
                {
                    ProviderID = providerId,
                    ProfessionalExpID = Convert.ToInt32(item)
                };
                this.Repository.Add(objProviderProfessionalExp);
            }

            this.Save();
        }

        /// <summary>
        /// Saves the provider environment.
        /// </summary>
        /// <param name="environmentIds">The environment ids.</param>
        /// <param name="providerId">The provider identifier.</param>
        public void SaveProviderEnvironment(List<string> environmentIds, int providerId)
        {
            var _providerEnvironment =
                this.Repository.ProviderEnvironments.Where(x => x.ProviderID == providerId).ToList();

            var _providerEnvironmentIds =
                this.Repository.ProviderEnvironments.Where(x => x.ProviderID == providerId)
                    .Select(x => new { EnvironmentID = x.EnvironmentID })
                    .ToArray();

            //if (environmentIds == null)
            //    environmentIds = new string[0];

            //delete items from ProviderEnvironment table
            var itemsToDelete =
                _providerEnvironment.Where(p => !environmentIds.Contains(p.EnvironmentID.ToString())).ToList();

            foreach (var item in itemsToDelete)
            {
                this.Repository.Delete(item);
            }

            //add items to ProviderEnvironment table.
            var itemToAdd =
                environmentIds.Except(_providerEnvironmentIds.Select(p => p.EnvironmentID.ToString())).ToList();

            foreach (var item in itemToAdd)
            {
                var objProviderEnvironment = new ProviderEnvironment()
                {
                    ProviderID = providerId,
                    EnvironmentID = Convert.ToInt32(item)
                };
                this.Repository.Add(objProviderEnvironment);
            }

            this.Save();
        }

        /// <summary>
        /// Saves the provider transportation school.
        /// </summary>
        /// <param name="schoolIds">The school ids.</param>
        /// <param name="providerId">The provider identifier.</param>
        public void SaveProviderTransportationSchool(int[] schoolIds, int providerId)
        {
            var _providerTransportationSchool =
                this.Repository.ProviderTransportationSchools.Where(x => x.ProviderID == providerId).ToList();

            var _providerSSIds =
                this.Repository.ProviderTransportationSchools.Where(x => x.ProviderID == providerId)
                    .Select(x => new { SchoolId = x.SchoolID.Value })
                    .ToArray();

            if (schoolIds == null)
            {
                schoolIds = new int[0]; 
            }

            //delete items from ProviderChildCareServices table
            var itemsToDelete = _providerTransportationSchool.Where(p => !schoolIds.Contains(p.SchoolID.Value)).ToList();

            foreach (var item in itemsToDelete)
            {
                this.Repository.Delete(item);
            }

            //add items to ProviderChildCareServices table.
            var itemToAdd = schoolIds.Except(_providerSSIds.Select(p => p.SchoolId)).ToList();

            foreach (var item in itemToAdd)
            {
                var objProviderSchool = new ProviderTransportationSchool()
                {
                    ProviderID = providerId,
                    SchoolID = Convert.ToInt32(item)
                };
                this.Repository.Add(objProviderSchool);
            }

            this.Save();
        }

        /// <summary>
        /// Saves the provider private market rates.
        /// </summary>
        /// <param name="providerPrivateMarketRate">The provider private market rate.</param>
        /// <param name="providerId">The provider identifier.</param>
        /// <param name="privateMarketRateId">The private market rate identifier.</param>
        public void SaveProviderPrivateMarketRates(List<ProviderPrivateMarketRate> providerPrivateMarketRate, int providerId, int privateMarketRateId)
        {
            var existingMarketRates = providerPrivateMarketRate.Where(c => c.ID > 0).ToList();

            if (existingMarketRates.Count > 0)
            {
                var itemToDelete = existingMarketRates.Where(c => c.Rate == null || c.Rate <=0 ).ToList();

                if (itemToDelete.Count > 0)
                {
                    foreach (var item in itemToDelete)
                    {
                        var deleteObject = this.Repository.ProviderPrivateMarketRates.Where(c => c.ID == item.ID).FirstOrDefault();
                        this.Repository.Delete(deleteObject);
                    }
                }

                var itemToUpdate = existingMarketRates.Where(c => c.Rate > 0).ToList();

                if (itemToUpdate.Count > 0)
                {
                    foreach (var item in itemToUpdate)
                    {
                        var editObject = this.Repository.ProviderPrivateMarketRates.Where(c => c.ID == item.ID).FirstOrDefault();
                        editObject.Rate = item.Rate;
                    }
                }
            }

            var itemToAdd = providerPrivateMarketRate.Where(c => c.ID == 0 && c.Rate > 0).ToList();

            if (itemToAdd.Count > 0)
            {
                foreach (var item in itemToAdd)
                {
                    var providerPrivateMarketRateEntity = new ProviderPrivateMarketRate { Rate = item.Rate, PrivateMarketRateCareLevelID = item.PrivateMarketRateCareLevelID, ProviderPrivateMarketRateSetID = privateMarketRateId };
                    this.Repository.Add(providerPrivateMarketRateEntity);
                }                 
            }

            this.Save(); 
        }

        /// <summary>
        /// Saves the provider alternate rates.
        /// </summary>
        /// <param name="providerAlternateRate">The provider alternate rate.</param>
        /// <param name="providerId">The provider identifier.</param>
        /// <param name="alternateRateSetId">The alternate rate set identifier.</param>
        public void SaveProviderAlternateRates(List<AlternateRate> providerAlternateRate, int providerId, int alternateRateSetId)
        {
            if (providerAlternateRate == null)
            {
                providerAlternateRate = new List<AlternateRate>(); 
            }

            var existingAlternateRates = providerAlternateRate.Where(c => c.ID > 0).ToList();

            if (existingAlternateRates.Count > 0)
            {
                var itemToDelete = existingAlternateRates.Where(c => c.Rate == null || c.Rate <= 0).ToList();

                if (itemToDelete.Count > 0)
                {
                    foreach (var item in itemToDelete)
                    {
                        var deleteObject = this.Repository.AlternateRates.Where(c => c.ID == item.ID).FirstOrDefault();
                        this.Repository.Delete(deleteObject);
                    }
                }

                var itemToUpdate = existingAlternateRates.Where(c => c.Rate > 0).ToList();

                if (itemToUpdate.Count > 0)
                {
                    foreach (var item in itemToUpdate)
                    {
                        var editObject = this.Repository.AlternateRates.Where(c => c.ID == item.ID).FirstOrDefault();
                        editObject.Rate = item.Rate;
                    }
                }
            }

            var itemToAdd = providerAlternateRate.Where(c => c.ID == 0 && c.Rate > 0).ToList();
            
            if (itemToAdd.Count > 0)
            {
                foreach (var item in itemToAdd)
                {
                    var alternateRateEntity = new AlternateRate { Rate = item.Rate, CareLevelID = item.CareLevelID, UnitOfCareID = item.UnitOfCareID, AlternateRateSetID = alternateRateSetId };
                    this.Repository.Add(alternateRateEntity);
                }
            }

            this.Save();
        }

        /// <summary>
        /// Saves changes.
        /// </summary>
        public override void Save(string overrideUser = null, IDictionary<string, object> overrideRouteDataValues = null)
        {
            if (System.Configuration.ConfigurationManager.AppSettings["EnableAutoMessaging"] == "True")
            {
                Action save = () => base.Save(overrideUser, overrideRouteDataValues);
                _messagingService.GenerateImmediateAlerts<Provider>(base.Repository, ProviderID, save);
            }
            else
            {
                base.Save(overrideUser, overrideRouteDataValues); 
            }
        }

        /// <summary>
        /// Calculates the criminal history fees and saves to provider.
        /// </summary>
        /// <param name="providerId">The provider identifier.</param>
        public void CalculateCriminalHistoryFees(int providerId)
        {
            var data = Set.Where(p => p.ID == providerId).Select(p => new { PermitExpDateCompare = p.PermitExpDateCompare, ApplicationID = (int?)p.CurrentApplication.ID, MemberCount = (int?)p.CurrentApplication.HouseholdMemberCount }).WithTranslations().Single();
            if (data.ApplicationID.HasValue)
            {
                var expDate = data.PermitExpDateCompare;
                var memberCount = Set.Where(p => p.ID == providerId).SelectMany(p => p.AtHomeHouseholdMembers).Where(m => m.Age >= 18 && (m.CriminalHistories.Where(h => h.CriminalHistoryTypeID == CriminalHistoryTypes.State && h.ExpirationDate >= expDate).Count() == 0)).WithTranslations().Count();
                if (data.MemberCount.HasValue)
                {
                    memberCount = Math.Max(memberCount, data.MemberCount.Value); 
                }
                var fee = memberCount * Model.CriminalHistory.CriminalHistoryFee;
                var paid = Set.Where(p => p.ID == providerId && p.CurrentApplication != null).SelectMany(p => p.CurrentApplication.ProviderApplicationFeeHistories).Where(f => f.FeeTypeID == FeeTypes.CriminalHistoryFee && f.FeeStatusID == FeeStatuses.Received).WithTranslations().Sum(f => (decimal?)f.Amount) ?? 0;
                var due = fee - paid;
                if (due > 0)
                {
                    var currentDue = Set.Where(p => p.ID == providerId && p.CurrentApplication != null).SelectMany(p => p.CurrentApplication.ProviderApplicationFeeHistories).Where(f => f.FeeTypeID == FeeTypes.CriminalHistoryFee && f.FeeStatusID == FeeStatuses.Due).WithTranslations().FirstOrDefault();
                    if (currentDue == null)
                    {
                        Repository.Add(new ProviderApplicationFeeHistory { ProviderApplicationID = data.ApplicationID.Value, Amount = due, FeeTypeID = FeeTypes.CriminalHistoryFee, FeeStatusID = FeeStatuses.Due }); 
                    }
                    else
                    {
                        currentDue.Amount = due; 
                    }
                    Save();
                }
            }
        }

        /// <summary>
        /// Calculates the application criminal history fees and saves to provider.
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        public void CalculateApplicationCriminalHistoryFees(int applicationId)
        {
            var data = Repository.ProviderApplications.Where(p => p.ID == applicationId).Select(a => new { PermitExpDateCompare = a.Provider.PermitExpDateCompare, ProviderID = a.ProviderID, MemberCount = (int?)a.HouseholdMemberCount }).WithTranslations().Single();
            var app = Repository.ProviderApplications.Where(p => p.ID == applicationId).Single();
            var providerId = data.ProviderID;
            var expDate = data.PermitExpDateCompare;
            var memberCount = Set.Where(p => p.ID == providerId).SelectMany(p => p.AtHomeHouseholdMembers).Where(m => m.Age >= 18 && (m.CriminalHistories.Where(h => h.CriminalHistoryTypeID == CriminalHistoryTypes.State && h.ExpirationDate >= expDate).Count() == 0)).WithTranslations().Count();
            if (data.MemberCount.HasValue)
            {
                memberCount = Math.Max(memberCount, data.MemberCount.Value); 
            }
            app.HouseholdMemberCount = memberCount;
            Save();

            var fee = memberCount * Model.CriminalHistory.CriminalHistoryFee;
            var paid = Repository.ProviderApplications.Where(p => p.ID == applicationId).SelectMany(a => a.ProviderApplicationFeeHistories).Where(f => f.FeeTypeID == FeeTypes.CriminalHistoryFee && f.FeeStatusID == FeeStatuses.Received).WithTranslations().Sum(f => (decimal?)f.Amount) ?? 0;
            var due = fee - paid;
            if (due > 0)
            {
                var currentDue = Repository.ProviderApplications.Where(p => p.ID == applicationId).SelectMany(a => a.ProviderApplicationFeeHistories).Where(f => f.FeeTypeID == FeeTypes.CriminalHistoryFee && f.FeeStatusID == FeeStatuses.Due).WithTranslations().FirstOrDefault();
                if (currentDue == null)
                {
                    Repository.Add(new ProviderApplicationFeeHistory { ProviderApplicationID = applicationId, Amount = due, FeeTypeID = FeeTypes.CriminalHistoryFee, FeeStatusID = FeeStatuses.Due }); 
                }
                else
                {
                    currentDue.Amount = due; 
                }
                Save();
            }
        }

        /// <summary>
        /// Calculates the application fees and saves to provider.
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        public void CalculateApplicationFees(int applicationId)
        {
            if (Repository.ProviderApplications.Where(pa => pa.ID == applicationId).Select(pa => pa.Provider.ProviderTypeID).SingleOrDefault() != Model.LookupIDs.ProviderTypes.Substitute)
            {
                var fee = ProviderApplicationFeeHistory.ApplicationFee;
                var paid = Repository.ProviderApplications.Where(p => p.ID == applicationId).SelectMany(a => a.ProviderApplicationFeeHistories).Where(f => f.FeeTypeID == FeeTypes.ApplicationFee && f.FeeStatusID == FeeStatuses.Received).WithTranslations().Sum(f => (decimal?)f.Amount) ?? 0;
                var due = fee - paid;
                if (due > 0)
                {
                    var currentDue = Repository.ProviderApplications.Where(p => p.ID == applicationId).SelectMany(a => a.ProviderApplicationFeeHistories).Where(f => f.FeeTypeID == FeeTypes.ApplicationFee && f.FeeStatusID == FeeStatuses.Due).WithTranslations().FirstOrDefault();
                    if (currentDue == null)
                    {
                        Repository.Add(new ProviderApplicationFeeHistory { ProviderApplicationID = applicationId, Amount = due, FeeTypeID = FeeTypes.ApplicationFee, FeeStatusID = FeeStatuses.Due }); 
                    }
                    else
                    {
                        currentDue.Amount = due; 
                    }
                    Save();
                }
            }
        }

        /// <summary>
        /// Checks to see if renewal application is needed.
        /// </summary>
        /// <param name="providerId">The provider identifier.</param>
        public void CheckRenewalApplicationNeeded(int providerId)
        {
            var providerStatus = Set.Where(p => p.ID == providerId).Select(p => p.CurrentApplication.ProviderPermit.PermitStatusText).WithTranslations().FirstOrDefault();
            if (providerStatus == PermitStatus.Expired)
            {
                var newApplication = new Model.ProviderApplication
                {
                    ProviderID = providerId,
                    ApplicationTypeID = Model.LookupIDs.ApplicationTypes.Renewal,
                    ApplicationStatusID = Model.LookupIDs.ApplicationStatuses.Incomplete,
                    ApplicationDate = DateTime.Now
                };
                Repository.Add(newApplication);
                Save();
                CalculateApplicationFees(newApplication.ID);
                CalculateApplicationCriminalHistoryFees(newApplication.ID);
            }
        }

        /// <summary>
        /// Auto assignment of CEPS and CCAR teams for Provider 
        /// </summary>
        /// <param name="ProviderId"></param>
        [Obsolete]
        public void AutoTeamAssigment(int ProviderId)
        {
            var providerInfo = Repository.Providers.Where(p => p.ID == ProviderId).Select(p => new
            {
                Provider = p,
                LastName = (p.ProviderTypeID == Model.LookupIDs.ProviderTypes.CDC || p.ProviderTypeID == Model.LookupIDs.ProviderTypes.SACC) ? p.BusinessName : p.Person.LastName,
                p.MainAddress.Zip,
            }).WithTranslations().FirstOrDefault();

            var provider = providerInfo.Provider;

            var cepsTeamId = Repository.TeamAutoAssignments
                                            .Where(p => p.Team.ProgramTypeID == ProgramTypes.CEPS
                                                    && (p.JoinedZipCodes.Contains(providerInfo.Zip) || p.JoinedZipCodes == null || p.JoinedZipCodes == "")
                                                    && (p.JoinedFirstCharacterOfLastName.Contains(providerInfo.LastName.Substring(0, 1).ToUpper()) || p.JoinedFirstCharacterOfLastName == null || p.JoinedFirstCharacterOfLastName == "")).Select(x => x.TeamID).FirstOrDefault();
            if (cepsTeamId > 0)
                provider.CEPSTeamID = cepsTeamId;

            var ccarTeamId = Repository.TeamAutoAssignments
                                            .Where(p => p.Team.ProgramTypeID == ProgramTypes.CCAR
                                                    && (p.JoinedZipCodes.Contains(providerInfo.Zip) || p.JoinedZipCodes == null || p.JoinedZipCodes == "")
                                                    && (p.JoinedFirstCharacterOfLastName.Contains(providerInfo.LastName.Substring(0, 1).ToUpper()) || p.JoinedFirstCharacterOfLastName == null || p.JoinedFirstCharacterOfLastName == "")).Select(x => x.TeamID).FirstOrDefault();
            if (ccarTeamId > 0)
                provider.CCARTeamID = ccarTeamId;

            Repository.Save();
        }

        /// <summary>
        /// Auto assignment of CEPS teams for Provider 
        /// </summary>
        /// <param name="ProviderId"></param>
        public bool AutoCEPSTeamAssigment(int ProviderId, bool force = false)
        {
            var providerInfo = Repository.Providers.Where(p => p.ID == ProviderId).Select(p => new { Provider = p, p.Person.LastName, p.MainAddress.Zip, }).WithTranslations().FirstOrDefault();
            var provider = providerInfo.Provider;

            if (!provider.CEPSTeamID.HasValue || force)
            {
                var cepsTeamId = Repository.TeamAutoAssignments
                                                .Where(p => p.Team.ProgramTypeID == ProgramTypes.CEPS
                                                        && (p.JoinedZipCodes.Contains(providerInfo.Zip) || p.JoinedZipCodes == null || p.JoinedZipCodes == "")
                                                        && (p.JoinedFirstCharacterOfLastName.Contains(providerInfo.LastName.Substring(0, 1).ToUpper()) || p.JoinedFirstCharacterOfLastName == null || p.JoinedFirstCharacterOfLastName == "")).Select(x => x.TeamID).FirstOrDefault();
                if (cepsTeamId > 0)
                    provider.CEPSTeamID = cepsTeamId;
                else
                    return false;
            }
            Repository.Save();
            return true;
        }

        /// <summary>
        /// Auto assignment of CCAR teams for Provider 
        /// </summary>
        /// <param name="ProviderId"></param>
        public bool AutoCCARTeamAssigment(int ProviderId, bool force = false)
        {
            var providerInfo = Repository.Providers.Where(p => p.ID == ProviderId).Select(p => new { Provider = p, p.Person.LastName, p.MainAddress.Zip, }).WithTranslations().FirstOrDefault();
            var provider = providerInfo.Provider;

            if (!provider.CCARTeamID.HasValue || force)
            {
                var ccarTeamId = Repository.TeamAutoAssignments
                                            .Where(p => p.Team.ProgramTypeID == ProgramTypes.CCAR
                                                    && (p.JoinedZipCodes.Contains(providerInfo.Zip) || p.JoinedZipCodes == null || p.JoinedZipCodes == "")
                                                    && (p.JoinedFirstCharacterOfLastName.Contains(providerInfo.LastName.Substring(0, 1).ToUpper()) || p.JoinedFirstCharacterOfLastName == null || p.JoinedFirstCharacterOfLastName == "")).Select(x => x.TeamID).FirstOrDefault();
                if (ccarTeamId > 0)
                    provider.CCARTeamID = ccarTeamId;
                else
                    return false;
            }
            Repository.Save();
            return true;
        }
        
        /// <summary>
        /// Locks the permit checks when permitted.
        /// </summary>
        /// <param name="providerId">The provider identifier.</param>
        /// <returns>Action&lt;System.Int32&gt;.</returns>
        public Action<int> LockPermitChecksSaveAction(int providerId, DateTime permitIssuedDate)
        {
            var trainings = Repository.ProviderTrainings.Where(t => t.ProviderID == providerId && !t.IsExpired && !t.ProviderPermitID.HasValue && t.CompletedDate < permitIssuedDate).WithTranslations().ToList();
            var certificates = Repository.ProviderCertifications.Where(c => c.ProviderID == providerId && !c.IsExpired && !c.ProviderPermitID.HasValue).WithTranslations().ToList();
            var crHistories = Repository.CriminalHistories.Where(cr => cr.ProviderID == providerId && !cr.IsExpired && !cr.ProviderPermitID.HasValue && cr.ReceivedDate.HasValue).WithTranslations().ToList();
            var cpsHistories = Repository.CPSHistories.Where(cps => cps.ProviderID == providerId && !cps.IsExpired && !cps.ProviderPermitID.HasValue && cps.ReceivedDate.HasValue).WithTranslations().ToList();
            var tbHistories = Repository.TBHistories.Where(tb => tb.ProviderID == providerId && !tb.IsExpired && !tb.ProviderPermitID.HasValue && tb.ReceivedDate.HasValue).WithTranslations().ToList();
            var swornAffirmations = Repository.SwornAffirmations.Where(sa => sa.ProviderID == providerId && !sa.IsExpired && !sa.ProviderPermitID.HasValue).WithTranslations().ToList();
            var fireInspections = Repository.ProviderInspectionFires.Where(i => i.ProviderAddress.ProviderID == providerId && !i.IsExpired && !i.ProviderPermitID.HasValue).WithTranslations().ToList();
            var homeInspections = Repository.ProviderInspectionHomes.Where(i => i.ProviderAddress.ProviderID == providerId && !i.IsExpired && !i.ProviderPermitID.HasValue).WithTranslations().ToList();

            Action<int> save = (int providerPermitId) =>
            {
                foreach (var training in trainings)
                    training.ProviderPermitID = providerPermitId;
                foreach (var certificate in certificates)
                    certificate.ProviderPermitID = providerPermitId;
                foreach (var crHistory in crHistories)
                    crHistory.ProviderPermitID = providerPermitId;
                foreach (var cpsHistory in cpsHistories)
                    cpsHistory.ProviderPermitID = providerPermitId;
                foreach (var tbHistory in tbHistories)
                    tbHistory.ProviderPermitID = providerPermitId;
                foreach (var fireInspection in fireInspections)
                    fireInspection.ProviderPermitID = providerPermitId;
                foreach (var homeInspection in homeInspections)
                    homeInspection.ProviderPermitID = providerPermitId;
                foreach (var swornAffirmation in swornAffirmations)
                    swornAffirmation.ProviderPermitID = providerPermitId;
                this.Save(overrideRouteDataValues: new Dictionary<string, object> { { "controller", "PermitAuthorization" }, { "action", "CreateStandard" } });
            };
            return save;
        }

        protected override int EntityID
        {
            get
            {
                return ProviderID;
            }
        }

        protected override int? TeamID
        {
            get
            {
                return Repository.Providers.Where(p => p.ID == ProviderID).Select(p => p.CEPSTeamID).FirstOrDefault();
            }
        }
    }
}