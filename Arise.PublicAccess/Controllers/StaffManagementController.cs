
using Arise.PublicAccess.Areas.ProviderApplication.Models;
using Arise.PublicAccess.Controllers;
using Arise.PublicAccess.Helpers;
using Arise.PublicAccess.Models;
using Arise.PublicAccess.Views.Shared.Components.Phone;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
using Empower.AccessControl;
using Empower.Common.CacheProviders;
using Empower.Common.Extensions;
using Empower.DomainService;
using Empower.Messaging;
using Empower.Model;
using Empower.Model.Entities;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Linq.Translations;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Arise.PublicAccess.Areas.ProviderApplication.Controllers
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
            staffManagementViewModel.ProviderTypeIDs = ProviderDomainService.Repository.GetBindToItems<ProviderType>().ToList();
            return View(staffManagementViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AddNewStaff(int? ID)
        {
            StaffManagementViewModel staffManagementViewModel = new StaffManagementViewModel();
            staffManagementViewModel.ProviderTypeIDs = ProviderDomainService.Repository.GetBindToItems<ProviderType>().ToList();
            staffManagementViewModel.InformationSourceIDs = ProviderDomainService.Repository.GetBindToItems<InformationSource>().ToList();
            staffManagementViewModel.PreFixIDs = ProviderDomainService.Repository.GetBindToItems<Prefix>().ToList();
            staffManagementViewModel.SuffixIDs = ProviderDomainService.Repository.GetBindToItems<Suffix>().ToList();
            staffManagementViewModel.LanguageIDs = ProviderDomainService.Repository.GetBindToItems<Language>().ToList();
            staffManagementViewModel.StaffIDs = ProviderDomainService.Repository.GetBindToItems<StaffType>().ToList();
            staffManagementViewModel.StaffQualificationIDs = ProviderDomainService.Repository.GetBindToItems<Empower.Model.StaffQualification>().ToList();
            staffManagementViewModel.RelationshipIDs = ProviderDomainService.Repository.GetBindToItems<Relationship>().ToList();
            staffManagementViewModel.MainAddress = new ProviderAddress();
            staffManagementViewModel.MainAddress1 = new ProviderAddress();
            staffManagementViewModel.EmergencyAddress = new ProviderAddress();
            staffManagementViewModel.DocumentUploadApplicableTypeIDs = ProviderDomainService.Repository.GetBindToItems<DocumentUploadApplicableType>().ToList();
            var staff = ProviderDomainService.Repository.PA_Staffs.Where(s => s.ID == ID).FirstOrDefault();
            if (staff != null)
            {
                StaffID = Convert.ToInt32(ID);
                staffManagementViewModel.InformationSourceID = staff.InformationSourceID;
                staffManagementViewModel.ProviderTypeID = staff.ProviderTypeID;
                staffManagementViewModel.ID = staff.ID;
                staffManagementViewModel.Prefix = staff.Prefix;
                staffManagementViewModel.Suffix = staff.Suffix;
                staffManagementViewModel.LastName = staff.LastName;
                staffManagementViewModel.MiddleName = staff.MiddleName;
                staffManagementViewModel.FirstName = staff.FirstName;
                staffManagementViewModel.Suffix = staff.Suffix;
                staffManagementViewModel.MainAddress.Address1 = staff.Address1;
                staffManagementViewModel.MainAddress.Address2 = staff.Address2;
                staffManagementViewModel.MainAddress.City = staff.City;
                staffManagementViewModel.MainAddress.State = staff.State;
                staffManagementViewModel.MainAddress.Zip = staff.Zip;
                staffManagementViewModel.MainAddress.MagisterialDistrictID = staff.MagisterialDistrictID;
                staffManagementViewModel.MainAddress.WardID = staff.WardID;
                staffManagementViewModel.HomePhone = staff.HomePhone;
                staffManagementViewModel.DateOfBirth = staff.DateOfBirth;
                staffManagementViewModel.FaxNumber = staff.FaxNumber;
            }

            var staffCharacteristic = ProviderDomainService.Repository.PA_StaffCharacteristics.Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffCharacteristic != null)
            {
                staffManagementViewModel.StaffCharacteristicID = staffCharacteristic.ID;
                staffManagementViewModel.TitleofPosition = staffCharacteristic.TitleofPosition;
                staffManagementViewModel.DateHired = staffCharacteristic.DateHired;
                staffManagementViewModel.SeparationDate = staffCharacteristic.SeparationDate;
                staffManagementViewModel.YearOfExperience = staffCharacteristic.YearsofRequiredExperience;
                staffManagementViewModel.Language = staffCharacteristic.LanguageID;
                staffManagementViewModel.ProfessionalDevelopmentCourses = staffCharacteristic.ProfessionalDevelopmentCourse;
                staffManagementViewModel.SupervisedOccupationExperience = staffCharacteristic.SupervisedOccupationExperience;
                if (staffCharacteristic.ProfileImage != null)
                {
                    staffManagementViewModel.Image = "data:image/jpeg;base64," + "" + Convert.ToBase64String(staffCharacteristic.ProfileImage);
                }
            };

            var staffHealthInformation = ProviderDomainService.Repository.PA_StaffHealthInformations.Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffHealthInformation != null)
            {
                staffManagementViewModel.StaffHealthInformationID = staffHealthInformation.ID;
                staffManagementViewModel.PhysicianFirstName = staffHealthInformation.FirstName;
                staffManagementViewModel.PhysicianLastName = staffHealthInformation.LastName;
                staffManagementViewModel.PhysicianPrefix = staffHealthInformation.Prefix;
                staffManagementViewModel.PhysicianSuffix = staffHealthInformation.Suffix;
                staffManagementViewModel.MainAddress1.Address1 = staffHealthInformation.Address1;
                staffManagementViewModel.MainAddress1.Address2 = staffHealthInformation.Address2;
                staffManagementViewModel.MainAddress1.City = staffHealthInformation.City;
                staffManagementViewModel.MainAddress1.State = staffHealthInformation.State;
                staffManagementViewModel.MainAddress1.Zip = staffHealthInformation.Zip;
                staffManagementViewModel.MainAddress1.MagisterialDistrictID = staffHealthInformation.MagisterialDistrictID;
                staffManagementViewModel.MainAddress1.WardID = staffHealthInformation.WardID;
                staffManagementViewModel.PhysicianHomePhone = staffHealthInformation.HomePhone;
                staffManagementViewModel.PhysicianWorkPhone = staffHealthInformation.WorkPhone;
                staffManagementViewModel.PhysicianWorkPhoneExtension = staffHealthInformation.WorkPhoneExtension;
                staffManagementViewModel.PhysicianMobilePhone = staffHealthInformation.MobilePhone;
                staffManagementViewModel.Allergies = staffHealthInformation.KnownAllergies;
                staffManagementViewModel.InsurencePhoneNumber = staffHealthInformation.InsurencePhoneNumber;
                staffManagementViewModel.HealthInsuranceCompany = staffHealthInformation.HealthInsuranceCompany;
            };

            var staffEmergencyContatctInformation = ProviderDomainService.Repository.PA_StaffEmergencyContactInformations.Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffEmergencyContatctInformation != null)
            {
                staffManagementViewModel.StaffEmenrgencyContactID = staffEmergencyContatctInformation.ID;
                staffManagementViewModel.EmergencyFirstName = staffEmergencyContatctInformation.FirstName;
                staffManagementViewModel.EmergencyLastName = staffEmergencyContatctInformation.LastName;
                staffManagementViewModel.EmergencyPrefix = staffEmergencyContatctInformation.Prefix;
                staffManagementViewModel.EmergencySuffix = staffEmergencyContatctInformation.Suffix;
                staffManagementViewModel.EmergencyAddress.Address1 = staffEmergencyContatctInformation.Address1;
                staffManagementViewModel.EmergencyAddress.Address2 = staffEmergencyContatctInformation.Address2;
                staffManagementViewModel.EmergencyAddress.City = staffEmergencyContatctInformation.City;
                staffManagementViewModel.EmergencyAddress.State = staffEmergencyContatctInformation.State;
                staffManagementViewModel.EmergencyAddress.Zip = staffEmergencyContatctInformation.Zip;
                staffManagementViewModel.EmergencyAddress.MagisterialDistrictID = staffEmergencyContatctInformation.MagisterialDistrictID;
                staffManagementViewModel.EmergencyAddress.WardID = staffEmergencyContatctInformation.WardID;
                staffManagementViewModel.EmergencyHomePhone = staffEmergencyContatctInformation.HomePhone;
                staffManagementViewModel.EmergencyWorkPhone = staffEmergencyContatctInformation.WorkPhone;
                staffManagementViewModel.EmergencyWorkPhoneExtension = staffEmergencyContatctInformation.WorkPhoneExtension;
                staffManagementViewModel.EmergencyMobilePhone = staffEmergencyContatctInformation.MobilePhone;
                staffManagementViewModel.RelationShipID = Convert.ToInt32(staffEmergencyContatctInformation.RelationshipID);
            }

            return View(staffManagementViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddNewStaff(StaffManagementViewModel staffManagementViewModel)
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
                var objStaff = ProviderDomainService.Repository.PA_Staffs.Where(p => p.ID == staffManagementViewModel.ID).FirstOrDefault();
                objStaff.InformationSourceID = staffManagementViewModel.InformationSourceID;
                objStaff.ProviderTypeID = staffManagementViewModel.ProviderTypeID;
                objStaff.FirstName = staffManagementViewModel.FirstName;
                objStaff.LastName = staffManagementViewModel.LastName;
                objStaff.Prefix = staffManagementViewModel.Prefix;
                objStaff.Suffix = staffManagementViewModel.Suffix;
                objStaff.Address1 = staffManagementViewModel.MainAddress.Address1;
                objStaff.Address2 = staffManagementViewModel.MainAddress.Address2;
                objStaff.City = staffManagementViewModel.MainAddress.City;
                objStaff.State = staffManagementViewModel.MainAddress.State;
                objStaff.Zip = staffManagementViewModel.MainAddress.Zip;
                objStaff.WardID = Convert.ToInt32(staffManagementViewModel.MainAddress.WardID);
                objStaff.MagisterialDistrictID = Convert.ToInt32(staffManagementViewModel.MainAddress.MagisterialDistrictID);
                objStaff.HomePhone = staffManagementViewModel.HomePhone;
                objStaff.WorkPhone = staffManagementViewModel.WorkPhone;
                objStaff.WorkPhoneExtension = staffManagementViewModel.WorkPhoneExtension;
                objStaff.MobilePhone = staffManagementViewModel.MobilePhone;
                objStaff.DateOfBirth = staffManagementViewModel.DateOfBirth;
                objStaff.FaxNumber = staffManagementViewModel.FaxNumber;
                ProviderDomainService.Repository.Update(objStaff, staffManagementViewModel.ID);
                ProviderDomainService.Save();

                var objStaffCharacteristic = ProviderDomainService.Repository.PA_StaffCharacteristics.Where(p => p.ID == staffManagementViewModel.StaffCharacteristicID).FirstOrDefault();
                objStaffCharacteristic.StaffID = staffManagementViewModel.ID;
                objStaffCharacteristic.ID = staffManagementViewModel.StaffCharacteristicID;
                objStaffCharacteristic.TitleofPosition = staffManagementViewModel.TitleofPosition;
                objStaffCharacteristic.DateHired = staffManagementViewModel.DateHired;
                objStaffCharacteristic.SeparationDate = staffManagementViewModel.SeparationDate;
                objStaffCharacteristic.YearsofRequiredExperience = staffManagementViewModel.YearOfExperience;
                objStaffCharacteristic.LanguageID = staffManagementViewModel.Language;
                objStaffCharacteristic.FileName = fileName;
                objStaffCharacteristic.ProfileImage = fileData;
                objStaffCharacteristic.ProfessionalDevelopmentCourse = staffManagementViewModel.ProfessionalDevelopmentCourses;
                objStaffCharacteristic.SupervisedOccupationExperience = staffManagementViewModel.SupervisedOccupationExperience;
                ProviderDomainService.Repository.Update(objStaffCharacteristic, staffManagementViewModel.StaffCharacteristicID);
                ProviderDomainService.Save();

                var objstaffHealthInformation = ProviderDomainService.Repository.PA_StaffHealthInformations.Where(p => p.ID == staffManagementViewModel.StaffHealthInformationID).FirstOrDefault();
                objstaffHealthInformation.FirstName = staffManagementViewModel.PhysicianFirstName;
                objstaffHealthInformation.LastName = staffManagementViewModel.PhysicianLastName;
                objstaffHealthInformation.Prefix = staffManagementViewModel.PhysicianPrefix;
                objstaffHealthInformation.Suffix = staffManagementViewModel.PhysicianSuffix;
                objstaffHealthInformation.Address1 = staffManagementViewModel.MainAddress1.Address1;
                objstaffHealthInformation.Address2 = staffManagementViewModel.MainAddress1.Address2;
                objstaffHealthInformation.City = staffManagementViewModel.MainAddress1.City;
                objstaffHealthInformation.State = staffManagementViewModel.MainAddress1.State;
                objstaffHealthInformation.Zip = staffManagementViewModel.MainAddress1.Zip;
                objstaffHealthInformation.MagisterialDistrictID = Convert.ToInt32(staffManagementViewModel.MainAddress1.MagisterialDistrictID);
                objstaffHealthInformation.WardID = Convert.ToInt32(staffManagementViewModel.MainAddress1.WardID);
                objstaffHealthInformation.HomePhone = staffManagementViewModel.PhysicianHomePhone;
                objstaffHealthInformation.WorkPhone = staffManagementViewModel.PhysicianWorkPhone;
                objstaffHealthInformation.WorkPhoneExtension = staffManagementViewModel.PhysicianWorkPhoneExtension;
                objstaffHealthInformation.MobilePhone = staffManagementViewModel.PhysicianMobilePhone;
                objstaffHealthInformation.KnownAllergies = staffManagementViewModel.Allergies;
                objstaffHealthInformation.InsurencePhoneNumber = staffManagementViewModel.InsurencePhoneNumber;
                objstaffHealthInformation.HealthInsuranceCompany = staffManagementViewModel.HealthInsuranceCompany;
                ProviderDomainService.Repository.Update(objStaffCharacteristic, staffManagementViewModel.StaffHealthInformationID);
                ProviderDomainService.Save();

                var objStaffEmergencyContatctInformation = ProviderDomainService.Repository.PA_StaffEmergencyContactInformations.Where(p => p.ID == staffManagementViewModel.StaffEmenrgencyContactID).FirstOrDefault();
                objStaffEmergencyContatctInformation.FirstName = staffManagementViewModel.EmergencyFirstName;
                objStaffEmergencyContatctInformation.LastName = staffManagementViewModel.EmergencyLastName;
                objStaffEmergencyContatctInformation.Prefix = staffManagementViewModel.EmergencyPrefix;
                objStaffEmergencyContatctInformation.Suffix = staffManagementViewModel.EmergencySuffix;
                objStaffEmergencyContatctInformation.Address1 = staffManagementViewModel.EmergencyAddress.Address1;
                objStaffEmergencyContatctInformation.Address2 = staffManagementViewModel.EmergencyAddress.Address2;
                objStaffEmergencyContatctInformation.City = staffManagementViewModel.EmergencyAddress.City;
                objStaffEmergencyContatctInformation.State = staffManagementViewModel.EmergencyAddress.State;
                objStaffEmergencyContatctInformation.Zip = staffManagementViewModel.EmergencyAddress.Zip;
                objStaffEmergencyContatctInformation.MagisterialDistrictID = Convert.ToInt32(staffManagementViewModel.EmergencyAddress.MagisterialDistrictID);
                objStaffEmergencyContatctInformation.WardID = Convert.ToInt32(staffManagementViewModel.EmergencyAddress.WardID);
                objStaffEmergencyContatctInformation.HomePhone = staffManagementViewModel.EmergencyHomePhone;
                objStaffEmergencyContatctInformation.WorkPhone = staffManagementViewModel.EmergencyWorkPhone;
                objStaffEmergencyContatctInformation.WorkPhoneExtension = staffManagementViewModel.EmergencyWorkPhoneExtension;
                objStaffEmergencyContatctInformation.MobilePhone = staffManagementViewModel.EmergencyMobilePhone;
                objStaffEmergencyContatctInformation.RelationshipID = staffManagementViewModel.RelationShipID;
                ProviderDomainService.Repository.Update(objStaffEmergencyContatctInformation, staffManagementViewModel.StaffEmenrgencyContactID);
                ProviderDomainService.Save();
            }
            else
            {
                PA_Staff pA_Staff = new PA_Staff
                {
                    InformationSourceID = staffManagementViewModel.InformationSourceID,
                    ProviderTypeID = staffManagementViewModel.ProviderTypeID,
                    FirstName = staffManagementViewModel.FirstName,
                    LastName = staffManagementViewModel.LastName,
                    Prefix = staffManagementViewModel.Prefix,
                    Suffix = staffManagementViewModel.Suffix,
                    Address1 = staffManagementViewModel.MainAddress.Address1,
                    Address2 = staffManagementViewModel.MainAddress.Address2,
                    City = staffManagementViewModel.MainAddress.City,
                    State = staffManagementViewModel.MainAddress.State,
                    Zip = staffManagementViewModel.MainAddress.Zip,
                    WardID = Convert.ToInt32(staffManagementViewModel.MainAddress.WardID),
                    MagisterialDistrictID = Convert.ToInt32(staffManagementViewModel.MainAddress.MagisterialDistrictID),
                    HomePhone = staffManagementViewModel.HomePhone,
                    WorkPhone = staffManagementViewModel.WorkPhone,
                    WorkPhoneExtension = staffManagementViewModel.WorkPhoneExtension,
                    MobilePhone = staffManagementViewModel.MobilePhone,
                    DateOfBirth = staffManagementViewModel.DateOfBirth,
                    FaxNumber = staffManagementViewModel.FaxNumber,
                    IsDeleted = false,
                };
                ProviderDomainService.Repository.Add(pA_Staff);
                ProviderDomainService.Repository.Save();
                pA_Staff.StaffKey = PA_Staff.GetFormattedKey(pA_Staff.ID);
                ProviderDomainService.Repository.Save();

                PA_StaffCharacteristic PA_StaffCharacteristic = new PA_StaffCharacteristic
                {
                    StaffID = pA_Staff.ID,
                    TitleofPosition = staffManagementViewModel.TitleofPosition,
                    DateHired = staffManagementViewModel.DateHired,
                    SeparationDate = staffManagementViewModel.SeparationDate,
                    YearsofRequiredExperience = staffManagementViewModel.YearOfExperience,
                    FileName = fileName,
                    ProfileImage = fileData,
                    LanguageID = staffManagementViewModel.Language,
                    SupervisedOccupationExperience = staffManagementViewModel.SupervisedOccupationExperience,
                    ProfessionalDevelopmentCourse = staffManagementViewModel.ProfessionalDevelopmentCourses,
                };
                ProviderDomainService.Repository.Add(PA_StaffCharacteristic);
                ProviderDomainService.Repository.Save();

                PA_StaffHealthInformation pA_staffHealthInformation = new PA_StaffHealthInformation
                {
                    StaffID = pA_Staff.ID,
                    FirstName = staffManagementViewModel.PhysicianFirstName,
                    LastName = staffManagementViewModel.PhysicianLastName,
                    Prefix = staffManagementViewModel.PhysicianPrefix,
                    Suffix = staffManagementViewModel.PhysicianSuffix,
                    Address1 = staffManagementViewModel.MainAddress1.Address1,
                    Address2 = staffManagementViewModel.MainAddress1.Address2,
                    City = staffManagementViewModel.MainAddress1.City,
                    State = staffManagementViewModel.MainAddress1.State,
                    Zip = staffManagementViewModel.MainAddress1.Zip,
                    MagisterialDistrictID = Convert.ToInt32(staffManagementViewModel.MainAddress1.MagisterialDistrictID),
                    WardID = Convert.ToInt32(staffManagementViewModel.MainAddress1.WardID),
                    HomePhone = staffManagementViewModel.PhysicianHomePhone,
                    WorkPhone = staffManagementViewModel.PhysicianWorkPhone,
                    WorkPhoneExtension = staffManagementViewModel.PhysicianWorkPhoneExtension,
                    MobilePhone = staffManagementViewModel.PhysicianMobilePhone,
                    KnownAllergies = staffManagementViewModel.Allergies,
                    InsurencePhoneNumber = staffManagementViewModel.InsurencePhoneNumber,
                    HealthInsuranceCompany = staffManagementViewModel.HealthInsuranceCompany,
                };
                ProviderDomainService.Repository.Add(pA_staffHealthInformation);
                ProviderDomainService.Repository.Save();

                PA_StaffEmergencyContactInformation Pa_StaffEmergencyContactInformation = new PA_StaffEmergencyContactInformation
                {
                    StaffID = pA_Staff.ID,
                    FirstName = staffManagementViewModel.EmergencyFirstName,
                    LastName = staffManagementViewModel.EmergencyLastName,
                    Prefix = staffManagementViewModel.EmergencyPrefix,
                    Suffix = staffManagementViewModel.EmergencySuffix,
                    Address1 = staffManagementViewModel.EmergencyAddress.Address1,
                    Address2 = staffManagementViewModel.EmergencyAddress.Address2,
                    City = staffManagementViewModel.EmergencyAddress.City,
                    State = staffManagementViewModel.EmergencyAddress.State,
                    Zip = staffManagementViewModel.EmergencyAddress.Zip,
                    MagisterialDistrictID = Convert.ToInt32(staffManagementViewModel.EmergencyAddress.MagisterialDistrictID),
                    WardID = Convert.ToInt32(staffManagementViewModel.EmergencyAddress.WardID),
                    HomePhone = staffManagementViewModel.EmergencyHomePhone,
                    WorkPhone = staffManagementViewModel.EmergencyWorkPhone,
                    WorkPhoneExtension = staffManagementViewModel.EmergencyWorkPhoneExtension,
                    MobilePhone = staffManagementViewModel.EmergencyMobilePhone,
                    RelationshipID = staffManagementViewModel.RelationShipID,
                };
                ProviderDomainService.Repository.Add(Pa_StaffEmergencyContactInformation);
                ProviderDomainService.Repository.Save();
            }
            return RedirectToAction("Index", "StaffManagement");
        }
        public IActionResult GetStaffs([DataSourceRequest] DataSourceRequest request, int facilityTypeID)
        {

            var objStaffData = (from s in ProviderDomainService.Repository.PA_Staffs
                                join sc in ProviderDomainService.Repository.PA_StaffCharacteristics on s.ID equals sc.StaffID
                                join st in ProviderDomainService.Repository.StaffTypes on sc.TitleofPosition equals st.ID
                                join pt in ProviderDomainService.Repository.ProviderTypes on s.ProviderTypeID equals pt.ID
                                select new StaffManagementViewModel
                                {
                                    ID = s.ID,
                                    ProviderTypeID = s.ProviderTypeID,
                                    StaffKey = s.StaffKey + " / " + s.FirstName + "  " + s.LastName,
                                    StaffType = st.Name,
                                    FacilityName = pt.Name,
                                    DateOfHireGridDateFormat = sc.DateHired,
                                    SeprationGridDateFormat = sc.SeparationDate,
                                    Phone = s.HomePhone,
                                    IsDeleted = s.IsDeleted,
                                }).Where(s => s.IsDeleted != true).ToList();

            if (facilityTypeID > 0)
            {
                objStaffData = objStaffData.Where(s => s.ProviderTypeID == facilityTypeID).ToList();
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
                    InstitutName = s.InstitutName,
                    DateAwarded = (DateTime)s.DateAwarded,
                    IsDeleted = s.IsDeleted,
                }
          ).Where(s => s.StaffID == StaffID && s.IsDeleted != true).ToList();

            return Json(staffQualification.ToDataSourceResult(request));
        }
        public async Task<IActionResult> AddStaffEducation([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {
            PA_StaffEducation pA_StaffEducation = new PA_StaffEducation
            {
                StaffID = StaffID,
                StaffQualificationID = staffManagementViewModel.StaffQualificationID,
                InstitutName = staffManagementViewModel.InstitutName,
                DateAwarded = staffManagementViewModel.DateAwarded,
            };

            ProviderDomainService.Repository.Add(pA_StaffEducation);
            ProviderDomainService.Repository.Save();
            staffManagementViewModel.StaffEducationID = pA_StaffEducation.ID;

            return Json(new[] { staffManagementViewModel }.ToDataSourceResult(request));
        }

        
        public async Task<IActionResult> UpdateStaffEducationAsync([DataSourceRequest] DataSourceRequest request, StaffManagementViewModel staffManagementViewModel)
        {
            if (staffManagementViewModel.StaffEducationID > 0)
            {
                var objStaffQualification = ProviderDomainService.Repository.PA_StaffEducations.Where(S => S.ID == staffManagementViewModel.StaffEducationID).FirstOrDefault();

                //objStaffQualification.StaffQualificationID = staffManagementViewModel.StaffQualificationID;
                //objStaffQualification.InstitutName = staffManagementViewModel.InstitutName;
                //objStaffQualification.DateAwarded = staffManagementViewModel.DateAwarded;
                //objStaffQualification.StaffID = StaffID;
                //ProviderDomainService.Repository.Update(objStaffQualification, staffManagementViewModel.StaffEducationID);
                //ProviderDomainService.Save();
                if (await TryUpdateModelAsync(objStaffQualification, nameof(staffManagementViewModel)))
                {
                    ProviderDomainService.Repository.Save();
                }
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

    }
}