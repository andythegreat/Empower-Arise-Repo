﻿using Arise.PublicAccess.Areas.ProviderApplication.Models;
using Arise.PublicAccess.Controllers;
using Arise.PublicAccess.Helpers;
using Arise.PublicAccess.Models;
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
        public IActionResult Edit(int? ID)
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
                staffManagementViewModel.Staff= staff;
                staffManagementViewModel.MainAddress.Address1 = staff.Address1;
                staffManagementViewModel.MainAddress.Address2 = staff.Address2;
                staffManagementViewModel.MainAddress.City = staff.City;
                staffManagementViewModel.MainAddress.State = staff.State;
                staffManagementViewModel.MainAddress.Zip = staff.Zip;
                staffManagementViewModel.MainAddress.MagisterialDistrictID = staff.MagisterialDistrictID;
                staffManagementViewModel.MainAddress.WardID = staff.WardID;
                staffManagementViewModel.PhoneConfig.HomePhone = staff.HomePhone;
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

            var staffHealthInformation = ProviderDomainService.Repository.PA_StaffHealthInformations.Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffHealthInformation != null)
            {
                staffManagementViewModel.StaffHealthInformationID = staffHealthInformation.ID;
                staffManagementViewModel.StaffHealthInformation = staffHealthInformation;
                staffManagementViewModel.MainAddress1.Address1 = staffHealthInformation.Address1;
                staffManagementViewModel.MainAddress1.Address2 = staffHealthInformation.Address2;
                staffManagementViewModel.MainAddress1.City = staffHealthInformation.City;
                staffManagementViewModel.MainAddress1.State = staffHealthInformation.State;
                staffManagementViewModel.MainAddress1.Zip = staffHealthInformation.Zip;
                staffManagementViewModel.MainAddress1.MagisterialDistrictID = staffHealthInformation.MagisterialDistrictID;
                staffManagementViewModel.MainAddress1.WardID = staffHealthInformation.WardID;
            };

            var staffEmergencyContatctInformation = ProviderDomainService.Repository.PA_StaffEmergencyContactInformations.Where(s => s.StaffID == ID).FirstOrDefault();
            if (staffEmergencyContatctInformation != null)
            {
                staffManagementViewModel.StaffEmenrgencyContactID = staffEmergencyContatctInformation.ID;
                staffManagementViewModel.StaffEmergencyContactInformation = staffEmergencyContatctInformation;
                staffManagementViewModel.EmergencyAddress.Address1 = staffEmergencyContatctInformation.Address1;
                staffManagementViewModel.EmergencyAddress.Address2 = staffEmergencyContatctInformation.Address2;
                staffManagementViewModel.EmergencyAddress.City = staffEmergencyContatctInformation.City;
                staffManagementViewModel.EmergencyAddress.State = staffEmergencyContatctInformation.State;
                staffManagementViewModel.EmergencyAddress.Zip = staffEmergencyContatctInformation.Zip;
                staffManagementViewModel.EmergencyAddress.MagisterialDistrictID = staffEmergencyContatctInformation.MagisterialDistrictID;
                staffManagementViewModel.EmergencyAddress.WardID = staffEmergencyContatctInformation.WardID;
            }
            return View(staffManagementViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> EditAsync(StaffManagementViewModel staffManagementViewModel)
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

                if (await TryUpdateModelAsync<PA_Staff>(objStaff, nameof(staffManagementViewModel.MainAddress)))
                {
                    ProviderDomainService.Save();
                }

                if (await TryUpdateModelAsync<PA_Staff>(objStaff, nameof(staffManagementViewModel.PhoneConfig)))
                {
                    ProviderDomainService.Save();
                }

                if (await TryUpdateModelAsync<PA_Staff>(objStaff, nameof(staffManagementViewModel.Staff)))
                {
                    ProviderDomainService.Save();
                }

                var objStaffCharacteristic = ProviderDomainService.Repository.PA_StaffCharacteristics.Where(p => p.ID == staffManagementViewModel.StaffCharacteristicID).FirstOrDefault();
                objStaffCharacteristic.FileName = fileName;
                objStaffCharacteristic.ProfileImage = fileData;
                await TryUpdateModelAsync<PA_StaffCharacteristic>(objStaffCharacteristic, nameof(staffManagementViewModel.StaffCharacteristic));
                ProviderDomainService.Save();

                var objStaffHealthInformation = ProviderDomainService.Repository.PA_StaffHealthInformations.Where(p => p.ID == staffManagementViewModel.StaffHealthInformationID).FirstOrDefault();

                if (await TryUpdateModelAsync<PA_StaffHealthInformation>(objStaffHealthInformation, nameof(staffManagementViewModel.MainAddress1)))
                {
                    ProviderDomainService.Save();
                }

                await TryUpdateModelAsync<PA_StaffHealthInformation>(objStaffHealthInformation, nameof(staffManagementViewModel.StaffHealthInformation));
                ProviderDomainService.Save();

                var objStaffEmergencyContactInformation = ProviderDomainService.Repository.PA_StaffEmergencyContactInformations.Where(p => p.ID == staffManagementViewModel.StaffEmenrgencyContactID).FirstOrDefault();

                if (await TryUpdateModelAsync<PA_StaffEmergencyContactInformation>(objStaffEmergencyContactInformation, nameof(staffManagementViewModel.EmergencyAddress)))
                {
                    ProviderDomainService.Save();
                }
                await TryUpdateModelAsync(objStaffEmergencyContactInformation, nameof(staffManagementViewModel.StaffEmergencyContactInformation));

                ProviderDomainService.Save();
            }

            else
            {
                var objStaff = new PA_Staff();
                if (staffManagementViewModel.MainAddress != null)
                {
                    if (await TryUpdateModelAsync(objStaff, nameof(staffManagementViewModel.MainAddress)))
                    {
                        ProviderDomainService.Save();
                    }
                }
                if (staffManagementViewModel.PhoneConfig != null)
                {
                    if (await TryUpdateModelAsync(objStaff, nameof(staffManagementViewModel.PhoneConfig)))
                    {
                        ProviderDomainService.Save();
                    }

                }

                if (await TryUpdateModelAsync(objStaff, nameof(staffManagementViewModel.Staff)))
                {
                    ProviderDomainService.Save();
                }

                ProviderDomainService.Repository.Add(objStaff);
                ProviderDomainService.Repository.Save();
                objStaff.StaffKey = PA_Staff.GetFormattedKey(objStaff.ID);
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
                if (staffManagementViewModel.MainAddress1 != null)
                {
                    if (await TryUpdateModelAsync(objStaffHealthInformation, nameof(staffManagementViewModel.MainAddress1)))
                    {
                        ProviderDomainService.Save();
                    }
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
                    if (await TryUpdateModelAsync(objStaffEmergencyContactInformation, nameof(staffManagementViewModel.EmergencyAddress)))
                    {
                        ProviderDomainService.Save();
                    }
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

        public IActionResult GetStaffs([DataSourceRequest] DataSourceRequest request, int facilityTypeID)
        {
            var objStaffData = (from s in ProviderDomainService.Repository.PA_Staffs
                                join sc in ProviderDomainService.Repository.PA_StaffCharacteristics on s.ID equals sc.StaffID
                                join st in ProviderDomainService.Repository.StaffTypes on sc.TitleOfPosition equals st.ID
                                join pt in ProviderDomainService.Repository.ProviderTypes on s.FacilityTypeID equals pt.ID
                                select new StaffManagementViewModel
                                {
                                    ID = s.ID,
                                    FacilityTypeID = s.FacilityTypeID,
                                    StaffKey = s.StaffKey ,
                                    StaffType = st.Name,
                                    FacilityName = pt.Name,
                                    DateOfHireGridDateFormat = sc.DateHired,
                                    SeprationGridDateFormat = sc.SeparationDate,
                                    Phone = s.HomePhone,
                                    IsDeleted = s.IsDeleted,
                                }).Where(s => s.IsDeleted != true).ToList();

            if (facilityTypeID > 0)
            {
                objStaffData = objStaffData.Where(s => s.FacilityTypeID == facilityTypeID).ToList();
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

    }
}