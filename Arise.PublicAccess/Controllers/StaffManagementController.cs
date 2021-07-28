using Arise.PublicAccess.Controllers;
using Arise.PublicAccess.Models;
using Arise.Shared.CoreUI.Helpers;
using Empower.AccessControl;
using Empower.Common.CacheProviders;
using Empower.Common.Extensions;
using Empower.DomainService;
using Empower.Messaging;
using Empower.Model;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Linq.Translations;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Arise.PublicAccess.Controllers
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
            StaffManagementViewModel staffManagementViewModel = new StaffManagementViewModel();

            staffManagementViewModel.FacilityIDs = (from app in ProviderDomainService.Repository.Applications
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
            return View(staffManagementViewModel);
        }

        public IActionResult Edit(int? ID)
        {
            StaffManagementViewModel staffManagementViewModel = new StaffManagementViewModel();

            // we cant used GetBindToItems here becouse we need to facility name and facility id not ID and Name
            staffManagementViewModel.FacilityIDs = (from app in ProviderDomainService.Repository.Applications
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

            staffManagementViewModel.InformationSourceIDs = ProviderDomainService.Repository.GetBindToItems<InformationSource>().ToList();
            staffManagementViewModel.PreFixIDs = ProviderDomainService.Repository.GetBindToItems<Prefix>().ToList();
            staffManagementViewModel.SuffixIDs = ProviderDomainService.Repository.GetBindToItems<Suffix>().ToList();
            staffManagementViewModel.LanguageIDs = ProviderDomainService.Repository.GetBindToItems<Language>().ToList();
            staffManagementViewModel.StaffIDs = ProviderDomainService.Repository.GetBindToItems<StaffType>().ToList();
            staffManagementViewModel.StaffQualificationIDs = ProviderDomainService.Repository.GetBindToItems<Empower.Model.StaffQualification>().ToList();
            staffManagementViewModel.RelationshipIDs = ProviderDomainService.Repository.GetBindToItems<Relationship>().ToList();
            staffManagementViewModel.MainAddress = new Address();
            staffManagementViewModel.HealthInformationAddress = new Address();
            staffManagementViewModel.EmergencyAddress = new Address();
            staffManagementViewModel.DocumentUploadApplicableTypeIDs = ProviderDomainService.Repository.GetBindToItems<DocumentUploadApplicableType>().ToList();
            staffManagementViewModel.GenderSelect = ProviderDomainService.Repository.GetBindToItems<Gender>(true);
            var staff = ProviderDomainService.Repository.StaffMembers
                            .Include(x => x.Address)
                            .Include(x => x.Person)
                            .Include(x => x.Phone)
                            .Where(s => s.ID == ID).FirstOrDefault();

            if (staff != null)
            {
                staffManagementViewModel.Staff = staff;
                staffManagementViewModel.Staff.Person = staff.Person;
                staffManagementViewModel.MainAddress = staff.Address;
                staffManagementViewModel.PhoneConfig = staff.Phone;
                staffManagementViewModel.DateOfBirth = staff.Person.DateOfBirth;
                staffManagementViewModel.GenderSelect = ProviderDomainService.Repository.GetBindToItems<Gender>(true, false, staffManagementViewModel.Gender);
            }

            var staffCharacteristic = ProviderDomainService.Repository.StaffCharacteristics.Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffCharacteristic != null)
            {
                staffManagementViewModel.StaffCharacteristicID = staffCharacteristic.ID;
                staffManagementViewModel.StaffCharacteristic = staffCharacteristic;
                if (staffCharacteristic.ProfileImage != null)
                {
                    staffManagementViewModel.Image = "data:image/jpeg;base64," + "" + Convert.ToBase64String(staffCharacteristic.ProfileImage);
                }
            };

            var staffHealthInformation = ProviderDomainService.Repository.StaffHealthInformations
                                            .Include(x => x.Name).Include(x => x.Phone).Include(x => x.Address)
                                            .Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffHealthInformation != null)
            {
                staffManagementViewModel.StaffHealthInformationID = staffHealthInformation.ID;
                staffManagementViewModel.StaffHealthInformation = staffHealthInformation;
                staffManagementViewModel.HealthInformationAddress = staffHealthInformation.Address;
            };

            var staffEmergencyContatctInformation = ProviderDomainService.Repository.StaffEmergencyContactInformations
                                                    .Include(x => x.Name).Include(x => x.Phone).Include(x => x.Address)
                                                    .Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffEmergencyContatctInformation != null)
            {
                staffManagementViewModel.StaffEmenrgencyContactID = staffEmergencyContatctInformation.ID;
                staffManagementViewModel.StaffEmergencyContactInformation = staffEmergencyContatctInformation;
                staffManagementViewModel.EmergencyAddress = staffEmergencyContatctInformation.Address;
            }
            return View(staffManagementViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(StaffManagementViewModel staffManagementViewModel)
        {
            string fileName = "";
            byte[] fileData = null;
            if (staffManagementViewModel.ProfileImage != null)
            {
                fileName = staffManagementViewModel.ProfileImage.GetFileName();
                fileData = staffManagementViewModel.ProfileImage.ToByteArray();
            }

            if (staffManagementViewModel.ID > 0)
            {
                var objStaff = ProviderDomainService.Repository.StaffMembers
                                .Include(x => x.Address).Include(x => x.Person).Include(x => x.Phone)
                                .Where(p => p.ID == staffManagementViewModel.ID).FirstOrDefault();

                if (await TryUpdateModelAsync(objStaff.Address, nameof(staffManagementViewModel.MainAddress)))
                {
                    ProviderDomainService.Save();
                }

                objStaff.Person.DateOfBirth = staffManagementViewModel.DateOfBirth;
                if (await TryUpdateModelAsync(objStaff.Person, nameof(staffManagementViewModel.StaffPersonDetail)))
                {
                    ProviderDomainService.Save();
                }

                if (await TryUpdateModelAsync(objStaff.Phone, nameof(staffManagementViewModel.PhoneConfig)))
                {
                    ProviderDomainService.Save();
                }

                if (await TryUpdateModelAsync<StaffMember>(objStaff, nameof(staffManagementViewModel.Staff)))
                {
                    ProviderDomainService.Save();
                }

                var objStaffCharacteristic = ProviderDomainService.Repository.StaffCharacteristics.Where(p => p.ID == staffManagementViewModel.StaffCharacteristicID).FirstOrDefault();
                objStaffCharacteristic.FileName = fileName;
                objStaffCharacteristic.ProfileImage = fileData;
                await TryUpdateModelAsync<StaffCharacteristic>(objStaffCharacteristic, nameof(staffManagementViewModel.StaffCharacteristic));
                ProviderDomainService.Save();

                var objStaffHealthInformation = ProviderDomainService.Repository.StaffHealthInformations
                                                .Include(x => x.Name).Include(x => x.Phone).Include(x => x.Address)
                                                .Where(p => p.ID == staffManagementViewModel.StaffHealthInformationID).FirstOrDefault();

                if (await TryUpdateModelAsync(objStaffHealthInformation.Address, nameof(staffManagementViewModel.HealthInformationAddress)))
                {
                    ProviderDomainService.Save();
                }

                await TryUpdateModelAsync(objStaffHealthInformation, nameof(staffManagementViewModel.StaffHealthInformation));
                ProviderDomainService.Save();

                var objStaffEmergencyContactInformation = ProviderDomainService.Repository.StaffEmergencyContactInformations
                                                            .Include(x => x.Name).Include(x => x.Phone).Include(x => x.Address)
                                                           .Where(p => p.ID == staffManagementViewModel.StaffEmenrgencyContactID).FirstOrDefault();

                if (await TryUpdateModelAsync(objStaffEmergencyContactInformation.Address, nameof(staffManagementViewModel.EmergencyAddress)))
                    ProviderDomainService.Save();
                await TryUpdateModelAsync(objStaffEmergencyContactInformation, nameof(staffManagementViewModel.StaffEmergencyContactInformation));

                ProviderDomainService.Save();
            }
            else
            {
                var objStaff = new StaffMember();

                if (staffManagementViewModel.MainAddress != null)
                {
                    objStaff.Address = new Address();
                    objStaff.Address.CreatedDate = System.DateTime.Now;
                    objStaff.Address.AddressTypeID = Empower.Model.LookupIDs.AddressTypes.Main;

                    if (await TryUpdateModelAsync(objStaff.Address, nameof(staffManagementViewModel.MainAddress)))
                    {
                        ProviderDomainService.Save();
                    }
                }
                if (staffManagementViewModel.PhoneConfig != null)
                {
                    objStaff.Phone = new Phone();
                    if (await TryUpdateModelAsync(objStaff.Phone, nameof(staffManagementViewModel.PhoneConfig)))
                    {
                        ProviderDomainService.Save();
                    }

                }
                if (staffManagementViewModel.Staff.Person != null)
                {
                    objStaff.Person = new Person();
                    objStaff.Person.DateOfBirth = staffManagementViewModel.DateOfBirth;
                    await TryUpdateModelAsync(objStaff.Person, nameof(staffManagementViewModel.Staff.Person));
                }

                if (await TryUpdateModelAsync(objStaff, nameof(staffManagementViewModel.Staff)))
                {
                    ProviderDomainService.Save();
                }

                ProviderDomainService.Repository.Add(objStaff);
                ProviderDomainService.Repository.Save();
                objStaff.StaffKey = StaffMember.GetFormattedKey(objStaff.ID);
                ProviderDomainService.Repository.Save();

                StaffCharacteristic ObjpA_StaffCharacteristic = new StaffCharacteristic();

                ObjpA_StaffCharacteristic.StaffID = objStaff.ID;
                ObjpA_StaffCharacteristic.FileName = fileName;
                ObjpA_StaffCharacteristic.ProfileImage = fileData;
                if (await TryUpdateModelAsync(ObjpA_StaffCharacteristic, nameof(staffManagementViewModel.StaffCharacteristic)))
                {
                    ProviderDomainService.Save();
                }

                ProviderDomainService.Repository.Add(ObjpA_StaffCharacteristic);
                ProviderDomainService.Repository.Save();

                var objStaffHealthInformation = new StaffHealthInformation();
                if (staffManagementViewModel.HealthInformationAddress != null)
                {
                    objStaffHealthInformation.Address = new Address();
                    objStaffHealthInformation.Address.CreatedDate = System.DateTime.Now;
                    objStaffHealthInformation.Address.AddressTypeID = Empower.Model.LookupIDs.AddressTypes.Main;
                    await TryUpdateModelAsync(objStaffHealthInformation.Address, nameof(staffManagementViewModel.HealthInformationAddress));
                    ProviderDomainService.Save();

                }

                if (await TryUpdateModelAsync(objStaffHealthInformation, nameof(staffManagementViewModel.StaffHealthInformation)))
                {
                    ProviderDomainService.Save();
                }

                objStaffHealthInformation.StaffID = objStaff.ID;
                ProviderDomainService.Repository.Add(objStaffHealthInformation);
                ProviderDomainService.Repository.Save();

                var objStaffEmergencyContactInformation = new StaffEmergencyContactInformation();
                if (staffManagementViewModel.EmergencyAddress != null)
                {
                    objStaffEmergencyContactInformation.Address = new Address();
                    objStaffEmergencyContactInformation.Address.CreatedDate = System.DateTime.Now;
                    objStaffEmergencyContactInformation.Address.AddressTypeID = Empower.Model.LookupIDs.AddressTypes.Main;
                    await TryUpdateModelAsync(objStaffEmergencyContactInformation.Address, nameof(staffManagementViewModel.EmergencyAddress));
                    ProviderDomainService.Save();
                }

                if (await TryUpdateModelAsync(objStaffEmergencyContactInformation, nameof(staffManagementViewModel.StaffEmergencyContactInformation)))
                {
                    ProviderDomainService.Save();
                }
                objStaffEmergencyContactInformation.StaffID = objStaff.ID;
                ProviderDomainService.Repository.Add(objStaffEmergencyContactInformation);
                ProviderDomainService.Repository.Save();
                staffManagementViewModel.ID = objStaff.ID;

            }
            return RedirectToAction(nameof(Arise.PublicAccess.Controllers.AccountIncidentReportController.Edit), nameof(Arise.PublicAccess.Controllers.StaffManagementController).RemoveControllerFromName(), new { ID = staffManagementViewModel.ID });
        }

        public IActionResult GetStaffs([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
                                var objStaffData = (from s in ProviderDomainService.Repository.StaffMembers
                                                    join sc in ProviderDomainService.Repository.StaffCharacteristics on s.ID equals sc.StaffID
                                                    join st in ProviderDomainService.Repository.StaffTypes on sc.StaffTypeID equals st.ID
                                                    join f in ProviderDomainService.Repository.Facilities on s.FacilityID equals f.ID
                                                    join certification in ProviderDomainService.Repository.CertifiedStaffInFirstAidCPRs
                                                    on s.ID equals certification.SfattID into certified
                                                    join criminal in ProviderDomainService.Repository.CriminalHistories
                                                    on s.ID equals criminal.StaffMemberID into criminalhistory
                                                    join cph in ProviderDomainService.Repository.ChildProtectionRegisterHistories
                                                    on s.ID equals cph.StaffMemberID into childprotection
                                                    from certification in certified.DefaultIfEmpty()
                                                     from criminal in criminalhistory.DefaultIfEmpty()
                                                    from cph in childprotection.DefaultIfEmpty()
                                                    select new StaffManagementViewModel
                                                    {
                                                        ID = s.ID,
                                                        StaffName = s.Person.FullName,
                                                        FacilityID = s.FacilityID,
                                                        StaffKey = s.StaffKey,
                                                        StaffType = st.Name,
                                                        FacilityName = f.FacilityName,
                                                        DateOfHireGridDateFormat = sc.DateHired,
                                                        SeprationGridDateFormat = sc.SeparationDate,
                                                        Phone = s.Phone.HomePhone,
                                                        IsDeleted = s.IsDeleted,
                                                        Certification = certification == null ? Empower.Common.Constant.UI.CertificateStatus.Fail: certification.ExpirationDate > DateTime.Now ? Empower.Common.Constant.UI.CertificateStatus.Pass : Empower.Common.Constant.UI.CertificateStatus.Fail,
                                                        Clearance = criminal == null ? cph == null ? Empower.Common.Constant.UI.CertificateStatus.Fail : criminal.ExpirationDate > DateTime.Now ? Empower.Common.Constant.UI.CertificateStatus.Pass: Empower.Common.Constant.UI.CertificateStatus.Fail : criminal.ExpirationDate > DateTime.Now ? Empower.Common.Constant.UI.CertificateStatus.Pass : Empower.Common.Constant.UI.CertificateStatus.Fail,
                                                    }).Where(s => !s.IsDeleted).WithTranslations().ToList();

            if (facilityID > 0)
            {
                objStaffData = objStaffData.Where(s => s.FacilityID == facilityID).ToList();
            }

            return Json(objStaffData.ToDataSourceResult(request));
        }

        public ActionResult DeleteStaff([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {
            var objStaff = ProviderDomainService.Repository.StaffMembers
                            .Where(c => c.ID == staffManagementViewModel.ID).FirstOrDefault();
            objStaff.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }

        public IActionResult GetStaffEducations([DataSourceRequest] DataSourceRequest request, int staffID)
        {
            var staffQualification = ProviderDomainService.Repository.StaffEducations
                .Select(s => new StaffManagementViewModel
                {
                    ID = s.ID,
                    StaffID = s.StaffID,
                    StaffQualificationID = s.StaffQualificationID,
                    InstituteName = s.InstituteName,
                    DateAwarded = (DateTime)s.DateAwarded,
                    IsDeleted = s.IsDeleted,
                }
          ).Where(s => s.StaffID == staffID && !s.IsDeleted ).ToList();

            return Json(staffQualification.ToDataSourceResult(request));
        }


        [HttpPost]
        public async Task<IActionResult> AddStaffEducation([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel, int staffID)
        {
            StaffEducation StaffEducation = new StaffEducation();
            await TryUpdateModelAsync<StaffEducation>(StaffEducation);
            StaffEducation.StaffID = staffID;
            ProviderDomainService.Repository.Add(StaffEducation);
            ProviderDomainService.Repository.Save();
            return Json(new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }



        [HttpPost]
        public async Task<ActionResult> UpdateStaffEducation([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {
            var objStaffQualification = ProviderDomainService.Repository.StaffEducations.Where(S => S.ID == staffManagementViewModel.ID).FirstOrDefault();
            StaffEducation pA_StaffEducation = new StaffEducation();
            await TryUpdateModelAsync<StaffEducation>(objStaffQualification);
            ProviderDomainService.Repository.Update(objStaffQualification, objStaffQualification.ID);
            ProviderDomainService.Save();

            return Json(new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult DeleteStaffEducation([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {
            var objStaffQualification = ProviderDomainService.Repository.StaffEducations
                            .Where(c => c.ID == staffManagementViewModel.ID).FirstOrDefault();
            objStaffQualification.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }

        public ActionResult TemplateList([DataSourceRequest] DataSourceRequest request)
        {
            List<DocumentTemplateViewModel> DocumentTemplates = ProviderDomainService.Repository.DocumentTemplates
                .AsNoTracking()
                .Where(dt => dt.IsActive)
                .Select(dt => new DocumentTemplateViewModel
                {
                    ID = dt.ID,
                    Name = dt.Name,
                    Description = dt.Description,
                    FileName = dt.FileName,
                    Category = dt.Category,
                    DateCreated = dt.DateCreated,
                }).OrderByDescending(c => c.ID).ToList();

            return Json(DocumentTemplates.ToDataSourceResult(request));
        }

        public IActionResult DownloadTemplate(int id)
        {
            var doc = ProviderDomainService.Repository.DocumentTemplates
                .Where(d => d.ID == id)
                .FirstOrDefault();

            if (doc == null)
            {
                return NotFound();
            }

            if (doc.TemplateFile == null)
            {
                return File(new byte[0], "application/msword", doc.FileName);
            }

            return File(doc.TemplateFile, "application/msword", doc.FileName);
        }

        [HttpPost]
        public JsonResult UploadDocuments(StaffManagementViewModel staffManagementViewModel)
        {
            string fileName = "";
            byte[] fileData = null;
            if (staffManagementViewModel.Document != null)
            {
                fileName = staffManagementViewModel.Document.GetFileName();
                fileData = staffManagementViewModel.Document.ToByteArray();
            }
            StaffDocument pA_StaffDocument = new StaffDocument();
            pA_StaffDocument.StaffID = staffManagementViewModel.ID;
            pA_StaffDocument.DocumentUploadApplicableTypeID = staffManagementViewModel.DocumentUploadApplicableTypeID;
            pA_StaffDocument.MetaData = staffManagementViewModel.MetaData;
            pA_StaffDocument.Document = fileData;
            pA_StaffDocument.DocumentName = fileName;
            pA_StaffDocument.IsDeleted = false;
            ProviderDomainService.Repository.Add(pA_StaffDocument);
            ProviderDomainService.Save();

            return Json("Ok");
        }

        public ActionResult GetDocumentList([DataSourceRequest] DataSourceRequest request, int staffID)
        {
            var staffDocument = ProviderDomainService.Repository.StaffDocuments
                 .Select(s => new StaffManagementViewModel
                 {
                     StaffDocumentId = s.ID,
                     DocumentName = s.DocumentName,
                     MetaData = s.MetaData,
                     StaffID = s.StaffID,
                     IsDeleted = s.IsDeleted,
                     DocumentUploadApplicableTypeID = s.DocumentUploadApplicableTypeID,
                 }
           ).Where(s => s.StaffID == staffID && !s.IsDeleted).ToList();

            return Json(staffDocument.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult DeleteStaffDocument([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {

            var objStaffDocument = ProviderDomainService.Repository.StaffDocuments
                                        .Where(c => c.ID == staffManagementViewModel.StaffDocumentId).FirstOrDefault();
            objStaffDocument.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }

        public IActionResult DownloadDocument(int id)
        {
            var doc = ProviderDomainService.Repository.StaffDocuments
                .Where(d => d.ID == id)
                .FirstOrDefault();

            if (doc == null)
            {
                return NotFound();
            }

            if (doc.Document == null)
            {
                return File(new byte[0], "application/msword", doc.DocumentName);
            }

            return File(doc.Document, "application/msword", doc.DocumentName);
        }

        public JsonResult GetStaffType(int facilityID)
        {
            var facilityTypeID = ProviderDomainService.Repository.Facilities.Where(x => x.ID == facilityID).Select(x => x.FacilityTypeID).FirstOrDefault();
            var staffType = ProviderDomainService.Repository.StaffTypes.Where(x => x.ProviderTypeID == facilityTypeID).ToList();
            return Json(staffType.Select(p => new { Value = p.ID, Text = p.Name }));

        }

        public ActionResult StaffCPRCheckList()
        {
            return View();
        }

        public JsonResult GetStaffCPRCheckList([DataSourceRequest] DataSourceRequest request)
        {

            var cprResult = (from cph in ProviderDomainService.Repository.ChildProtectionRegisterHistories
                              join s in ProviderDomainService.Repository.StaffMembers on cph.StaffMemberID equals s.ID
                              join pe in ProviderDomainService.Repository.Persons on s.PersonID equals pe.ID
                              select new ChildProtectionRegisterHistoryViewModel
                              {
                                  ID = cph.ID,
                                  Name = pe.FirstName + " " + pe.LastName,
                                  SentDate = cph.SentDate,
                                  ReceivedDate = cph.ReceivedDate,
                                  Status = cph.Result == null ? null : cph.Result.Name,
                                  UploadDocument = cph.BackgroundCheckDocument == null ? null : cph.BackgroundCheckDocument.Name,
                                  Comments = cph.Comments,
                              });
            return Json(cprResult.ToDataSourceResult(request));
        }

        public IActionResult EditStaffCPRCheck(int? ID)
        {
            int providerID = ProviderDomainService.ProviderID;

            var cprVM = new ChildProtectionRegisterHistoryViewModel();

            var cprValue = ProviderDomainService.Repository.ChildProtectionRegisterHistories
                                .Include(p => p.Person)
                                .Include(p => p.BackgroundCheckDocument)
                                .Where(c => c.ID == ID)
                                .FirstOrDefault();

            var staffNameList = (from ap in ProviderDomainService.Repository.Applications
                                 join sf in ProviderDomainService.Repository.StaffMembers on ap.FacilityID equals sf.FacilityTypeID
                                 join sfc in ProviderDomainService.Repository.StaffCharacteristics on sf.ID equals sfc.StaffID
                                 join pe in ProviderDomainService.Repository.Persons on sf.PersonID equals pe.ID
                                 where !sf.IsDeleted
                                 select new
                                 {
                                     sf.ID,
                                     FullName = pe.FullName,
                                     StaffTypeID = sfc.StaffTypeID
                                 }).WithTranslations().ToList();

            cprVM.NameLists = (from snl in staffNameList
                               select new SelectListItem
                               {
                                   Value = snl.ID.ToString(),
                                   Text = snl.FullName,
                               }).ToList();

            cprVM.States = (from f in ProviderDomainService.Repository.States
                              select new SelectListItem
                              {
                                  Value = f.ID.ToString(),
                                  Text = f.Code.ToString()
                              }).ToList();

            cprVM.StatusList = ProviderDomainService.Repository.GetBindToItems<CriminalHistoryResultType>();

            if (cprValue != null)
            {
                cprVM.ID = cprValue.ID;
                var crimeType = ProviderDomainService.Repository.GetBindToItems<CriminalHistoryResultType>();
                string resVal = (cprValue.ResultID ?? 0).ToString();
                crimeType.Where(x => x.Value == resVal).ToList().ForEach(x => x.Selected = true);
                cprVM.StatusList = crimeType;
                cprVM.NameId = cprValue.PersonID ?? 0;
                cprVM.ProviderID = ProviderDomainService.ProviderID;
                cprVM.SentDate = cprValue.SentDate;
                cprVM.ReceivedDate = cprValue.ReceivedDate;
                cprVM.StatusID = cprValue.ResultID ?? 0;
                cprVM.Comments = cprValue.Comments;
                cprVM.StateID = cprValue.StateID;

                if (cprValue.BackgroundCheckDocument != null)
                {
                    cprVM.UploadDocument = cprValue.BackgroundCheckDocument.Name;
                }

            }
            return View(cprVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStaffCPRCheck(ChildProtectionRegisterHistoryViewModel cprVM)
        {
            if (ModelState.IsValid)
            {
                var userId = ProviderDomainService.Repository.Users
                  .Where(u => u.UserName == UserName).Select(u => u.ID).FirstOrDefault();
                int providerID = ProviderDomainService.ProviderID;
                if(cprVM.ID == 0)
                {
                    ChildProtectionRegisterHistory val = new();

                    if (cprVM.Documents != null)
                    {
                        var backgroundDoc = new BackgroundCheckDocument();
                        backgroundDoc.Name = cprVM.Documents.GetFileName();
                        backgroundDoc.Data = cprVM.Documents.ToByteArray();
                        backgroundDoc.CreatedDate = DateTime.Now;
                        backgroundDoc.CreatedByID = userId;
                        backgroundDoc.DocumentTypeID = Empower.Model.LookupIDs.DocumentTypes.ChildProtectionRegisterCheck;
                        val.BackgroundCheckDocument = backgroundDoc;
                    }

                    val.PersonID = cprVM.NameId;
                    val.StaffMember = ProviderDomainService.Repository.StaffMembers.Where(x => x.PersonID == cprVM.NameId).FirstOrDefault();
                    val.SentDate = cprVM.SentDate;
                    val.ReceivedDate = cprVM.ReceivedDate;
                    val.ResultID = cprVM.StatusID;
                    var x = ProviderDomainService.Repository.SystemConfigurations.Where(x => x.ConfigurationOption == ConfigurationOptions.CpsTimeSpan)
                                    .Select(x => x.IntegerValue).FirstOrDefault();
                    val.ExpirationYears = x ?? 0;
                    val.Comments = cprVM.Comments;
                    val.StateID = cprVM.StateID;
                    ProviderDomainService.Repository.Add(val);
                    ProviderDomainService.Save();
                }
                else
                {
                    var childProtVal = ProviderDomainService.Repository.ChildProtectionRegisterHistories.Where(x => x.ID == cprVM.ID).FirstOrDefault();
                    if (childProtVal.ID > 0)
                    {

                        if (cprVM.Documents != null)
                        {
                            var backgroundDoc = new BackgroundCheckDocument
                            {
                                Name = cprVM.Documents.GetFileName(),
                                Data = cprVM.Documents.ToByteArray(),
                                CreatedDate = DateTime.Now,
                                CreatedByID = userId,
                                DocumentTypeID = Empower.Model.LookupIDs.DocumentTypes.ChildProtectionRegisterCheck
                            };
                            childProtVal.BackgroundCheckDocument = backgroundDoc;
                        }

                        childProtVal.SentDate = cprVM.SentDate;
                        childProtVal.ReceivedDate = cprVM.ReceivedDate;
                        childProtVal.ResultID = cprVM.StatusID;
                        childProtVal.Comments = cprVM.Comments;
                        childProtVal.PersonID = cprVM.NameId;
                        ProviderDomainService.Repository.Update(childProtVal, cprVM.ID);
                        ProviderDomainService.Save();
                    }
                }
                return RedirectToAction("StaffCPRCheckList"); ;
            }

            return View(cprVM);
        }


    }
}