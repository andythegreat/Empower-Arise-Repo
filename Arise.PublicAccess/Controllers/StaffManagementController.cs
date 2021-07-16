﻿using Arise.PublicAccess.Controllers;
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
        public static int StaffID;
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

            staffManagementViewModel.FacilityIDs = (from app in ProviderDomainService.Repository.PA_Applications
                                                    join fac in ProviderDomainService.Repository.PA_Facilities
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Edit(int? ID)
        {
            StaffManagementViewModel staffManagementViewModel = new StaffManagementViewModel();

            // we cant used GetBindToItems here becouse we need to facility name and facility id not ID and Name
            staffManagementViewModel.FacilityIDs = (from app in ProviderDomainService.Repository.PA_Applications
                                                    join fac in ProviderDomainService.Repository.PA_Facilities
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
            staffManagementViewModel.MainAddress = new PA_Address();
            staffManagementViewModel.HealthInformationAddress = new PA_Address();
            staffManagementViewModel.EmergencyAddress = new PA_Address();
            staffManagementViewModel.DocumentUploadApplicableTypeIDs = ProviderDomainService.Repository.GetBindToItems<DocumentUploadApplicableType>().ToList();
            staffManagementViewModel.GenderSelect = ProviderDomainService.Repository.GetBindToItems<Gender>(true);
            var staff = ProviderDomainService.Repository.PA_Staffs
                            .Include(x => x.Address).Include(x => x.Person).Include(x => x.Phone)
                            .Where(s => s.ID == ID).FirstOrDefault();

            if (staff != null)
            {
                StaffID = Convert.ToInt32(ID);
                staffManagementViewModel.Staff = staff;
                staffManagementViewModel.Staff.Person = staff.Person;
                staffManagementViewModel.MainAddress = staff.Address;
                staffManagementViewModel.PhoneConfig = staff.Phone;
                staffManagementViewModel.DateOfBirth = staff.Person.DateOfBirth;
                staffManagementViewModel.GenderSelect = ProviderDomainService.Repository.GetBindToItems<Gender>(true, false, staffManagementViewModel.Gender);
            }

            var staffCharacteristic = ProviderDomainService.Repository.PA_StaffCharacteristics.Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffCharacteristic != null)
            {
                staffManagementViewModel.StaffCharacteristicID = staffCharacteristic.ID;
                staffManagementViewModel.StaffCharacteristic = staffCharacteristic;
                if (staffCharacteristic.ProfileImage != null)
                {
                    staffManagementViewModel.Image = "data:image/jpeg;base64," + "" + Convert.ToBase64String(staffCharacteristic.ProfileImage);
                }
            };

            var staffHealthInformation = ProviderDomainService.Repository.PA_StaffHealthInformations
                                            .Include(x => x.Name).Include(x => x.Phone).Include(x => x.Address)
                                            .Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffHealthInformation != null)
            {
                staffManagementViewModel.StaffHealthInformationID = staffHealthInformation.ID;
                staffManagementViewModel.StaffHealthInformation = staffHealthInformation;
                staffManagementViewModel.HealthInformationAddress = staffHealthInformation.Address;
            };

            var staffEmergencyContatctInformation = ProviderDomainService.Repository.PA_StaffEmergencyContactInformations
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
        [AllowAnonymous]
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
                var objStaff = ProviderDomainService.Repository.PA_Staffs
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

                if (await TryUpdateModelAsync<PA_StaffMember>(objStaff, nameof(staffManagementViewModel.Staff)))
                {
                    ProviderDomainService.Save();
                }

                var objStaffCharacteristic = ProviderDomainService.Repository.PA_StaffCharacteristics.Where(p => p.ID == staffManagementViewModel.StaffCharacteristicID).FirstOrDefault();
                objStaffCharacteristic.FileName = fileName;
                objStaffCharacteristic.ProfileImage = fileData;
                await TryUpdateModelAsync<PA_StaffCharacteristic>(objStaffCharacteristic, nameof(staffManagementViewModel.StaffCharacteristic));
                ProviderDomainService.Save();

                var objStaffHealthInformation = ProviderDomainService.Repository.PA_StaffHealthInformations
                                                .Include(x => x.Name).Include(x => x.Phone).Include(x => x.Address)
                                                .Where(p => p.ID == staffManagementViewModel.StaffHealthInformationID).FirstOrDefault();

                if (await TryUpdateModelAsync(objStaffHealthInformation.Address, nameof(staffManagementViewModel.HealthInformationAddress)))
                {
                    ProviderDomainService.Save();
                }

                await TryUpdateModelAsync(objStaffHealthInformation, nameof(staffManagementViewModel.StaffHealthInformation));
                ProviderDomainService.Save();

                var objStaffEmergencyContactInformation = ProviderDomainService.Repository.PA_StaffEmergencyContactInformations
                                                            .Include(x => x.Name).Include(x => x.Phone).Include(x => x.Address)
                                                           .Where(p => p.ID == staffManagementViewModel.StaffEmenrgencyContactID).FirstOrDefault();

                if (await TryUpdateModelAsync(objStaffEmergencyContactInformation.Address, nameof(staffManagementViewModel.EmergencyAddress)))
                    ProviderDomainService.Save();
                await TryUpdateModelAsync(objStaffEmergencyContactInformation, nameof(staffManagementViewModel.StaffEmergencyContactInformation));

                ProviderDomainService.Save();
            }

            else
            {
                var objStaff = new PA_StaffMember();
                if (staffManagementViewModel.MainAddress != null)
                {
                    objStaff.Address = new PA_Address();
                    objStaff.Address.CreatedDate = System.DateTime.Now;
                    objStaff.Address.AddressTypeID = Empower.Model.LookupIDs.AddressTypes.Main;

                    if (await TryUpdateModelAsync(objStaff.Address, nameof(staffManagementViewModel.MainAddress)))
                    {
                        ProviderDomainService.Save();
                    }
                }
                if (staffManagementViewModel.PhoneConfig != null)
                {
                    objStaff.Phone = new PA_Phone();
                    if (await TryUpdateModelAsync(objStaff.Phone, nameof(staffManagementViewModel.PhoneConfig)))
                    {
                        ProviderDomainService.Save();
                    }

                }
                if (staffManagementViewModel.Staff.Person != null)
                {                    
                    objStaff.Person = new PA_Person();
                    objStaff.Person.DateOfBirth = staffManagementViewModel.DateOfBirth;
                    await TryUpdateModelAsync(objStaff.Person, nameof(staffManagementViewModel.Staff.Person));
                }

                if (await TryUpdateModelAsync(objStaff, nameof(staffManagementViewModel.Staff)))
                {
                    ProviderDomainService.Save();
                }

                ProviderDomainService.Repository.Add(objStaff);
                ProviderDomainService.Repository.Save();
                objStaff.StaffKey = PA_StaffMember.GetFormattedKey(objStaff.ID);
                ProviderDomainService.Repository.Save();

                PA_StaffCharacteristic ObjpA_StaffCharacteristic = new PA_StaffCharacteristic();

                ObjpA_StaffCharacteristic.StaffID = objStaff.ID;
                ObjpA_StaffCharacteristic.FileName = fileName;
                ObjpA_StaffCharacteristic.ProfileImage = fileData;
                if (await TryUpdateModelAsync(ObjpA_StaffCharacteristic, nameof(staffManagementViewModel.StaffCharacteristic)))
                {
                    ProviderDomainService.Save();
                }

                ProviderDomainService.Repository.Add(ObjpA_StaffCharacteristic);
                ProviderDomainService.Repository.Save();

                var objStaffHealthInformation = new PA_StaffHealthInformation();
                if (staffManagementViewModel.HealthInformationAddress != null)
                {
                    objStaffHealthInformation.Address = new PA_Address();
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

                var objStaffEmergencyContactInformation = new PA_StaffEmergencyContactInformation();
                if (staffManagementViewModel.EmergencyAddress != null)
                {
                    objStaffEmergencyContactInformation.Address = new PA_Address();
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

            }
            return RedirectToAction("Index", "StaffManagement");
        }

        public IActionResult GetStaffs([DataSourceRequest] DataSourceRequest request, int facilityID)
        {
                                var objStaffData = (from s in ProviderDomainService.Repository.PA_Staffs
                                                    join sc in ProviderDomainService.Repository.PA_StaffCharacteristics on s.ID equals sc.StaffID
                                                    join st in ProviderDomainService.Repository.StaffTypes on sc.TitleOfPosition equals st.ID
                                                    join f in ProviderDomainService.Repository.PA_Facilities on s.FacilityID equals f.ID
                                                    join certification in ProviderDomainService.Repository.PA_CertifiedStaffInFirstAidCPRs
                                                    on s.ID equals certification.SfattID into certified
                                                    join criminal in ProviderDomainService.Repository.PA_CriminalHistories
                                                    on s.ID equals criminal.StaffMemberID into criminalhistory
                                                    join cph in ProviderDomainService.Repository.PA_ChildProtectionRegisterHistories
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
            var objStaff = ProviderDomainService.Repository.PA_Staffs
                            .Where(c => c.ID == staffManagementViewModel.ID).FirstOrDefault();
            objStaff.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }

        public IActionResult GetStaffEducations([DataSourceRequest] DataSourceRequest request)
        {
            var staffQualification = ProviderDomainService.Repository.PA_StaffEducations
                .Select(s => new StaffManagementViewModel
                {
                    StaffEducationID = s.ID,
                    StaffID = s.StaffID,
                    StaffQualificationID = s.StaffQualificationID,
                    InstituteName = s.InstituteName,
                    DateAwarded = (DateTime)s.DateAwarded,
                    IsDeleted = s.IsDeleted,
                }
          ).Where(s => s.StaffID == StaffID && s.IsDeleted != true).ToList();

            return Json(staffQualification.ToDataSourceResult(request));
        }

        public IActionResult AddStaffEducation([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {
            PA_StaffEducation pA_StaffEducation = new PA_StaffEducation();
            TryUpdateModelAsync<PA_StaffEducation>(pA_StaffEducation);
            pA_StaffEducation.StaffID = StaffID;
            ProviderDomainService.Repository.Add(pA_StaffEducation);
            ProviderDomainService.Repository.Save();
            staffManagementViewModel.StaffEducationID = pA_StaffEducation.ID;

            return Json(new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }

        public IActionResult UpdateStaffEducation([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {
            if (staffManagementViewModel.StaffEducationID > 0)
            {
                var objStaffQualification = ProviderDomainService.Repository.PA_StaffEducations.Where(S => S.ID == staffManagementViewModel.StaffEducationID).FirstOrDefault();
                PA_StaffEducation pA_StaffEducation = new PA_StaffEducation();
                TryUpdateModelAsync<PA_StaffEducation>(objStaffQualification);
                objStaffQualification.ID = staffManagementViewModel.StaffEducationID;
                ProviderDomainService.Repository.Update(objStaffQualification, objStaffQualification.ID);
                ProviderDomainService.Save();
            }

            return Json(new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult DeleteStaffEducation([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {
            var objStaffQualification = ProviderDomainService.Repository.PA_StaffEducations
                            .Where(c => c.ID == staffManagementViewModel.StaffEducationID).FirstOrDefault();
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
            PA_StaffDocument pA_StaffDocument = new PA_StaffDocument();
            pA_StaffDocument.StaffID = StaffID;
            pA_StaffDocument.DocumentUploadApplicableTypeID = staffManagementViewModel.DocumentUploadApplicableTypeID;
            pA_StaffDocument.MetaData = staffManagementViewModel.MetaData;
            pA_StaffDocument.Document = fileData;
            pA_StaffDocument.DocumentName = fileName;
            pA_StaffDocument.IsDeleted = false;
            ProviderDomainService.Repository.Add(pA_StaffDocument);
            ProviderDomainService.Save();

            return Json("Ok");
        }

        public ActionResult GetDocumentList([DataSourceRequest] DataSourceRequest request)
        {
            var staffDocument = ProviderDomainService.Repository.PA_StaffDocuments
                 .Select(s => new StaffManagementViewModel
                 {
                     StaffDocumentId = s.ID,
                     DocumentName = s.DocumentName,
                     MetaData = s.MetaData,
                     StaffID = s.StaffID,
                     IsDeleted = s.IsDeleted,
                     DocumentUploadApplicableTypeID = s.DocumentUploadApplicableTypeID,
                 }
           ).Where(s => s.StaffID == StaffID && s.IsDeleted != true).ToList();

            return Json(staffDocument.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult DeleteStaffDocument([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {

            var objStaffDocument = ProviderDomainService.Repository.PA_StaffDocuments
                                        .Where(c => c.ID == staffManagementViewModel.StaffDocumentId).FirstOrDefault();
            objStaffDocument.IsDeleted = true;
            ProviderDomainService.Save();
            return Json(data: new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }

        public IActionResult DownloadDocument(int id)
        {
            var doc = ProviderDomainService.Repository.PA_StaffDocuments
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
            var facilityTypeID = ProviderDomainService.Repository.PA_Facilities.Where(x=>x.ID == facilityID).Select(x => x.FacilityTypeID).FirstOrDefault();
            var staffType = ProviderDomainService.Repository.StaffTypes.Where(x => x.ProviderTypeID == facilityTypeID).ToList();
            return Json(staffType.Select(p => new { Value = p.ID, Text = p.Name }));

        }
    }
}