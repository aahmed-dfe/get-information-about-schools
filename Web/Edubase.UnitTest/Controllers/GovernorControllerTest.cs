﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Edubase.Services.Domain;
using Edubase.Services.Enums;
using Edubase.Services.Establishments;
using Edubase.Services.Establishments.Models;
using Edubase.Services.Exceptions;
using Edubase.Services.Governors;
using Edubase.Services.Governors.DisplayPolicies;
using Edubase.Services.Governors.Models;
using Edubase.Services.Groups;
using Edubase.Services.Groups.Models;
using Edubase.Services.Lookup;
using Edubase.Services.Nomenclature;
using Edubase.Web.UI.Areas.Governors.Controllers;
using Edubase.Web.UI.Areas.Governors.Models;
using Edubase.Web.UI.Exceptions;
using Edubase.Web.UI.Helpers;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Edubase.UnitTest.Controllers
{
    [TestFixture]
    public class GovernorControllerTest : UnitTestBase<GovernorController>
    {
        [Test]
        public async Task Gov_Edit_Null_Params()
        {
            await ObjectUnderTest.Edit(null, null, null, null).ShouldThrowAsync<InvalidParameterException>();
        }

        [Test]
        public async Task Gov_Edit_GroupIdSpecified()
        {
            var groupId = 5;
            var governorDetailsDto = new GovernorsDetailsDto
            {
                ApplicableRoles = new List<eLookupGovernorRole> { eLookupGovernorRole.AccountingOfficer, eLookupGovernorRole.Governor},
                RoleDisplayPolicies = new Dictionary<eLookupGovernorRole, GovernorDisplayPolicy>
                {
                    { eLookupGovernorRole.AccountingOfficer, new GovernorDisplayPolicy() },
                    { eLookupGovernorRole.Governor, new GovernorDisplayPolicy() }
                },
                CurrentGovernors = new List<GovernorModel>(),
                HistoricalGovernors = new List<GovernorModel>()
            };

            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(null, groupId, It.IsAny<IPrincipal>())).ReturnsAsync(() => governorDetailsDto);
            GetMock<ILayoutHelper>().Setup(l => l.PopulateLayoutProperties(It.IsAny<GovernorsGridViewModel>(), null, groupId, It.IsAny<IPrincipal>(), It.IsAny<Action<EstablishmentModel>>(), It.IsAny<Action<GroupModel>>())).Returns(Task.CompletedTask);
            SetupCachedLookupService();

            GetMock<ICachedLookupService>().Setup(c => c.GovernorRolesGetAllAsync()).ReturnsAsync(() => new List<LookupDto>
            {
                new LookupDto { Id = (int)eLookupGovernorRole.AccountingOfficer, Name = "Accounting Officer"},
                new LookupDto { Id = (int)eLookupGovernorRole.Governor, Name = "Governor"}
            });

            var result = await ObjectUnderTest.Edit(5, null, null, null);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();

            var model = viewResult.Model as GovernorsGridViewModel;
            model.ShouldNotBeNull();

            model.GovernorShared.ShouldBe(false);
            model.RemovalGid.ShouldBeNull();
            model.GroupUId.ShouldBe(groupId);
            model.EstablishmentUrn.ShouldBeNull();

            model.GovernorRoles.Count.ShouldBe(governorDetailsDto.ApplicableRoles.Count);

            viewResult.ViewData.Keys.Contains("DuplicateGovernor").ShouldBe(false);
            viewResult.ViewData.ModelState.IsValid.ShouldBeTrue();
        }

        [Test]
        public async Task Gov_Edit_EstabIdSpecified()
        {
            var establishmentId = 23;
            var governorDetailsDto = new GovernorsDetailsDto
            {
                ApplicableRoles = new List<eLookupGovernorRole> { eLookupGovernorRole.AccountingOfficer, eLookupGovernorRole.Governor },
                RoleDisplayPolicies = new Dictionary<eLookupGovernorRole, GovernorDisplayPolicy>
                {
                    { eLookupGovernorRole.AccountingOfficer, new GovernorDisplayPolicy() },
                    { eLookupGovernorRole.Governor, new GovernorDisplayPolicy() }
                },
                CurrentGovernors = new List<GovernorModel>(),
                HistoricalGovernors = new List<GovernorModel>()
            };

            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(establishmentId, null, It.IsAny<IPrincipal>())).ReturnsAsync(() => governorDetailsDto);
            GetMock<ILayoutHelper>().Setup(l => l.PopulateLayoutProperties(It.IsAny<GovernorsGridViewModel>(), establishmentId, null, It.IsAny<IPrincipal>(), It.IsAny<Action<EstablishmentModel>>(), It.IsAny<Action<GroupModel>>())).Returns(Task.CompletedTask);
            SetupCachedLookupService();

            GetMock<ICachedLookupService>().Setup(c => c.GovernorRolesGetAllAsync()).ReturnsAsync(() => new List<LookupDto>
            {
                new LookupDto { Id = (int)eLookupGovernorRole.AccountingOfficer, Name = "Accounting Officer"},
                new LookupDto { Id = (int)eLookupGovernorRole.Governor, Name = "Governor"}
            });

            var result = await ObjectUnderTest.Edit(null, establishmentId, null, null);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();

            var model = viewResult.Model as GovernorsGridViewModel;
            model.ShouldNotBeNull();

            model.GovernorShared.ShouldBe(false);
            model.RemovalGid.ShouldBeNull();
            model.GroupUId.ShouldBeNull();
            model.EstablishmentUrn.ShouldBe(establishmentId);

            model.GovernorRoles.Count.ShouldBe(governorDetailsDto.ApplicableRoles.Count);

            viewResult.ViewData.Keys.Contains("DuplicateGovernor").ShouldBe(false);
            viewResult.ViewData.ModelState.IsValid.ShouldBeTrue();
        }

        [Test]
        public async Task Gov_Edit_GroupId_RemovalGid_GidExists()
        {
            var groupId = 5;
            var governorDetailsDto = new GovernorsDetailsDto
            {
                ApplicableRoles = new List<eLookupGovernorRole> { eLookupGovernorRole.AccountingOfficer, eLookupGovernorRole.Governor },
                RoleDisplayPolicies = new Dictionary<eLookupGovernorRole, GovernorDisplayPolicy>
                {
                    { eLookupGovernorRole.AccountingOfficer, new GovernorDisplayPolicy() },
                    { eLookupGovernorRole.Governor, new GovernorDisplayPolicy() }
                },
                CurrentGovernors = new List<GovernorModel>
                {
                    new GovernorModel
                    {
                        Id = 43,
                        RoleId = (int)eLookupGovernorRole.Establishment_SharedLocalGovernor
                    }
                },
                HistoricalGovernors = new List<GovernorModel>()
            };

            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(null, groupId, It.IsAny<IPrincipal>())).ReturnsAsync(() => governorDetailsDto);
            GetMock<ILayoutHelper>().Setup(l => l.PopulateLayoutProperties(It.IsAny<GovernorsGridViewModel>(), null, groupId, It.IsAny<IPrincipal>(), It.IsAny<Action<EstablishmentModel>>(), It.IsAny<Action<GroupModel>>())).Returns(Task.CompletedTask);
            SetupCachedLookupService();

            GetMock<ICachedLookupService>().Setup(c => c.GovernorRolesGetAllAsync()).ReturnsAsync(() => new List<LookupDto>
            {
                new LookupDto { Id = (int)eLookupGovernorRole.AccountingOfficer, Name = "Accounting Officer"},
                new LookupDto { Id = (int)eLookupGovernorRole.Governor, Name = "Governor"}
            });

            var result = await ObjectUnderTest.Edit(5, null, 43, null);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();

            var model = viewResult.Model as GovernorsGridViewModel;
            model.ShouldNotBeNull();

            model.GovernorShared.ShouldBe(true);
            model.RemovalGid.ShouldBe(43);
            model.GroupUId.ShouldBe(groupId);

            model.GovernorRoles.Count.ShouldBe(governorDetailsDto.ApplicableRoles.Count);

            viewResult.ViewData.Keys.Contains("DuplicateGovernor").ShouldBe(false);
            viewResult.ViewData.ModelState.IsValid.ShouldBeTrue();
        }

        [Test]
        public async Task Gov_Edit_GroupId_RemovalGid_GidDoesNotExist()
        {
            var groupId = 5;
            var governorDetailsDto = new GovernorsDetailsDto
            {
                ApplicableRoles = new List<eLookupGovernorRole> { eLookupGovernorRole.AccountingOfficer, eLookupGovernorRole.Governor },
                RoleDisplayPolicies = new Dictionary<eLookupGovernorRole, GovernorDisplayPolicy>
                {
                    { eLookupGovernorRole.AccountingOfficer, new GovernorDisplayPolicy() },
                    { eLookupGovernorRole.Governor, new GovernorDisplayPolicy() }
                },
                CurrentGovernors = new List<GovernorModel>(),
                HistoricalGovernors = new List<GovernorModel>()
            };

            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(null, groupId, It.IsAny<IPrincipal>())).ReturnsAsync(() => governorDetailsDto);
            GetMock<ILayoutHelper>().Setup(l => l.PopulateLayoutProperties(It.IsAny<GovernorsGridViewModel>(), null, groupId, It.IsAny<IPrincipal>(), It.IsAny<Action<EstablishmentModel>>(), It.IsAny<Action<GroupModel>>())).Returns(Task.CompletedTask);
            SetupCachedLookupService();

            GetMock<ICachedLookupService>().Setup(c => c.GovernorRolesGetAllAsync()).ReturnsAsync(() => new List<LookupDto>
            {
                new LookupDto { Id = (int)eLookupGovernorRole.AccountingOfficer, Name = "Accounting Officer"},
                new LookupDto { Id = (int)eLookupGovernorRole.Governor, Name = "Governor"}
            });

            var result = await ObjectUnderTest.Edit(5, null, 43, null);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();

            var model = viewResult.Model as GovernorsGridViewModel;
            model.ShouldNotBeNull();

            model.GovernorShared.ShouldBe(false);
            model.RemovalGid.ShouldBe(43);
            model.GroupUId.ShouldBe(groupId);

            model.GovernorRoles.Count.ShouldBe(governorDetailsDto.ApplicableRoles.Count);

            viewResult.ViewData.Keys.Contains("DuplicateGovernor").ShouldBe(false);
            viewResult.ViewData.ModelState.IsValid.ShouldBeTrue();
        }

        [Test]
        public async Task Gov_Edit_GroupId_DuplicateGovernorId()
        {
            var groupId = 5;
            var duplicateId = 13;
            var governorDetailsDto = new GovernorsDetailsDto
            {
                ApplicableRoles = new List<eLookupGovernorRole> { eLookupGovernorRole.AccountingOfficer, eLookupGovernorRole.Governor },
                RoleDisplayPolicies = new Dictionary<eLookupGovernorRole, GovernorDisplayPolicy>
                {
                    { eLookupGovernorRole.AccountingOfficer, new GovernorDisplayPolicy() },
                    { eLookupGovernorRole.Governor, new GovernorDisplayPolicy() }
                },
                CurrentGovernors = new List<GovernorModel>(),
                HistoricalGovernors = new List<GovernorModel>()
            };

            var governor = new GovernorModel
            {
                Id = duplicateId
            };

            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(null, groupId, It.IsAny<IPrincipal>())).ReturnsAsync(() => governorDetailsDto);
            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorAsync(duplicateId, It.IsAny<IPrincipal>())).ReturnsAsync(() => governor);
            GetMock<ILayoutHelper>().Setup(l => l.PopulateLayoutProperties(It.IsAny<GovernorsGridViewModel>(), null, groupId, It.IsAny<IPrincipal>(), It.IsAny<Action<EstablishmentModel>>(), It.IsAny<Action<GroupModel>>())).Returns(Task.CompletedTask);
            SetupCachedLookupService();

            GetMock<ICachedLookupService>().Setup(c => c.GovernorRolesGetAllAsync()).ReturnsAsync(() => new List<LookupDto>
            {
                new LookupDto { Id = (int)eLookupGovernorRole.AccountingOfficer, Name = "Accounting Officer"},
                new LookupDto { Id = (int)eLookupGovernorRole.Governor, Name = "Governor"}
            });

            var result = await ObjectUnderTest.Edit(5, null, null, duplicateId);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();

            var model = viewResult.Model as GovernorsGridViewModel;
            model.ShouldNotBeNull();

            model.GovernorShared.ShouldBe(false);
            model.RemovalGid.ShouldBeNull();
            model.GroupUId.ShouldBe(groupId);

            model.GovernorRoles.Count.ShouldBe(governorDetailsDto.ApplicableRoles.Count);

            viewResult.ViewData.Keys.Contains("DuplicateGovernor").ShouldBe(true);
            viewResult.ViewData["DuplicateGovernor"].ShouldBe(governor);
            viewResult.ViewData.ModelState.IsValid.ShouldBeTrue();
        }

        [Test]
        public async Task Gov_Edit_RoleExists()
        {
            var establishmentId = 23;
            var governorDetailsDto = new GovernorsDetailsDto
            {
                ApplicableRoles = new List<eLookupGovernorRole> { eLookupGovernorRole.AccountingOfficer, eLookupGovernorRole.Governor },
                RoleDisplayPolicies = new Dictionary<eLookupGovernorRole, GovernorDisplayPolicy>
                {
                    { eLookupGovernorRole.AccountingOfficer, new GovernorDisplayPolicy() },
                    { eLookupGovernorRole.Governor, new GovernorDisplayPolicy() }
                },
                CurrentGovernors = new List<GovernorModel>(),
                HistoricalGovernors = new List<GovernorModel>()
            };

            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(establishmentId, null, It.IsAny<IPrincipal>())).ReturnsAsync(() => governorDetailsDto);
            GetMock<ILayoutHelper>().Setup(l => l.PopulateLayoutProperties(It.IsAny<GovernorsGridViewModel>(), establishmentId, null, It.IsAny<IPrincipal>(), It.IsAny<Action<EstablishmentModel>>(), It.IsAny<Action<GroupModel>>())).Returns(Task.CompletedTask);
            SetupCachedLookupService();

            GetMock<ICachedLookupService>().Setup(c => c.GovernorRolesGetAllAsync()).ReturnsAsync(() => new List<LookupDto>
            {
                new LookupDto { Id = (int)eLookupGovernorRole.AccountingOfficer, Name = "Accounting Officer"},
                new LookupDto { Id = (int)eLookupGovernorRole.Governor, Name = "Governor"}
            });

            var result = await ObjectUnderTest.Edit(null, establishmentId, null, null, true);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();

            var model = viewResult.Model as GovernorsGridViewModel;
            model.ShouldNotBeNull();

            model.GovernorShared.ShouldBe(false);
            model.RemovalGid.ShouldBeNull();
            model.GroupUId.ShouldBeNull();
            model.EstablishmentUrn.ShouldBe(establishmentId);

            model.GovernorRoles.Count.ShouldBe(governorDetailsDto.ApplicableRoles.Count);

            viewResult.ViewData.Keys.Contains("DuplicateGovernor").ShouldBe(false);
            viewResult.ViewData.ModelState.IsValid.ShouldBeFalse();
            viewResult.ViewData.ModelState["role"].Errors.Count.ShouldBe(1);
        }

        [Test]
        public void Gov_View_ModelSpecified()
        {
            var model = new GovernorsGridViewModel();
            var result = ObjectUnderTest.View(null, null, model);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();
            viewResult.ViewName.ShouldBe("~/Areas/Governors/Views/Governor/ViewEdit.cshtml");

            var modelResult = viewResult.Model as GovernorsGridViewModel;
            modelResult.ShouldNotBeNull();
            modelResult.ShouldBe(model);
        }

        [Test]
        public void Gov_View_groupUIdSpecified()
        {
            var groupUId = 10;
            var governorDetailsDto = new GovernorsDetailsDto
            {
                ApplicableRoles = new List<eLookupGovernorRole> { eLookupGovernorRole.AccountingOfficer, eLookupGovernorRole.Governor },
                RoleDisplayPolicies = new Dictionary<eLookupGovernorRole, GovernorDisplayPolicy>
                {
                    { eLookupGovernorRole.AccountingOfficer, new GovernorDisplayPolicy() },
                    { eLookupGovernorRole.Governor, new GovernorDisplayPolicy() }
                },
                CurrentGovernors = new List<GovernorModel>(),
                HistoricalGovernors = new List<GovernorModel>()
            };

            var groupModel = new GroupModel {DelegationInformation = "delegation info"};

            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(null, groupUId, It.IsAny<IPrincipal>())).ReturnsAsync(() => governorDetailsDto);
            GetMock<IGroupReadService>().Setup(g => g.GetAsync(groupUId, It.IsAny<IPrincipal>())).ReturnsAsync(() => new ServiceResultDto<GroupModel>(groupModel));
            SetupCachedLookupService();

            var result = ObjectUnderTest.View(groupUId, null, null);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();
            viewResult.ViewName.ShouldBe("~/Areas/Governors/Views/Governor/ViewEdit.cshtml");

            var modelResult = viewResult.Model as GovernorsGridViewModel;
            modelResult.ShouldNotBeNull();
            modelResult.ShowDelegationInformation.ShouldBeFalse();
            modelResult.DelegationInformation.ShouldBe(groupModel.DelegationInformation);
            modelResult.GroupUId.ShouldBe(groupUId);
            modelResult.EstablishmentUrn.ShouldBeNull();
        }

        [Test]
        public void Gov_View_establishmentUrnSpecified()
        {
            var establishmentUrn = 26;
            var governorDetailsDto = new GovernorsDetailsDto
            {
                ApplicableRoles = new List<eLookupGovernorRole> { eLookupGovernorRole.AccountingOfficer, eLookupGovernorRole.Governor },
                RoleDisplayPolicies = new Dictionary<eLookupGovernorRole, GovernorDisplayPolicy>
                {
                    { eLookupGovernorRole.AccountingOfficer, new GovernorDisplayPolicy() },
                    { eLookupGovernorRole.Governor, new GovernorDisplayPolicy() }
                },
                CurrentGovernors = new List<GovernorModel>(),
                HistoricalGovernors = new List<GovernorModel>()
            };

            var establishment = new EstablishmentModel();

            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(establishmentUrn, null, It.IsAny<IPrincipal>())).ReturnsAsync(() => governorDetailsDto);
            GetMock<IEstablishmentReadService>().Setup(e => e.GetAsync(establishmentUrn, It.IsAny<IPrincipal>())).ReturnsAsync(() => new ServiceResultDto<EstablishmentModel>(establishment));
            GetMock<IEstablishmentReadService>().Setup(e => e.GetPermissibleLocalGovernorsAsync(establishmentUrn, It.IsAny<IPrincipal>())).ReturnsAsync(() => new List<LookupDto>());
            SetupCachedLookupService();

            var result = ObjectUnderTest.View(null, establishmentUrn, null);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();
            viewResult.ViewName.ShouldBe("~/Areas/Governors/Views/Governor/ViewEdit.cshtml");

            var modelResult = viewResult.Model as GovernorsGridViewModel;
            modelResult.ShouldNotBeNull();
            modelResult.GovernanceMode.ShouldBeNull();
            modelResult.GroupUId.ShouldBeNull();
            modelResult.EstablishmentUrn.ShouldBe(establishmentUrn);
        }

        [Test]
        public async Task Gov_AddEditOrReplace_NullParams()
        {
            GetMock<ControllerContext>().SetupGet(c => c.RouteData).Returns(new RouteData(new Route("", new PageRouteHandler("~/")), new PageRouteHandler("~/")));
            await ObjectUnderTest.AddEditOrReplace(null, null, null, null).ShouldThrowAsync<EdubaseException>();
        }

        [Test]
        public async Task Gov_AddEditOrReplace_RoleSpecified_Single_NotShared_AlreadyExists()
        {
            var estabUrn = 4;
            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(estabUrn, null, It.IsAny<IPrincipal>())).ReturnsAsync(() => new GovernorsDetailsDto
            {
                CurrentGovernors = new List<GovernorModel> { new GovernorModel { RoleId = (int)eLookupGovernorRole.ChairOfGovernors }}
            });
            GetMock<ControllerContext>().SetupGet(c => c.RouteData).Returns(new RouteData(new Route("", new PageRouteHandler("~/")), new PageRouteHandler("~/")) );

            var result = await ObjectUnderTest.AddEditOrReplace(null, estabUrn, eLookupGovernorRole.ChairOfGovernors, null);

            var redirectResult = result as RedirectToRouteResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.RouteName.ShouldBe("EstabEditGovernance");
        }

        [Test]
        public async Task Gov_AddEditOrReplace_RoleSpecified_Single_NotShared_DoesntExist()
        {
            var estabUrn = 4;
            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(estabUrn, null, It.IsAny<IPrincipal>())).ReturnsAsync(() => new GovernorsDetailsDto
            {
                CurrentGovernors = new List<GovernorModel>()
            });
            GetMock<ControllerContext>().SetupGet(c => c.RouteData).Returns(new RouteData(new Route("", new PageRouteHandler("~/")), new PageRouteHandler("~/")));
            GetMock<ILayoutHelper>().Setup(l => l.PopulateLayoutProperties(It.IsAny<CreateEditGovernorViewModel>(), estabUrn, null, It.IsAny<IPrincipal>(), It.IsAny<Action<EstablishmentModel>>(), It.IsAny<Action<GroupModel>>())).Returns(Task.CompletedTask);
            GetMock<IGovernorsReadService>().Setup(g => g.GetEditorDisplayPolicyAsync(eLookupGovernorRole.ChairOfGovernors, false, It.IsAny<IPrincipal>())).ReturnsAsync(() => new GovernorDisplayPolicy());
            SetupCachedLookupService();

            var result = await ObjectUnderTest.AddEditOrReplace(null, estabUrn, eLookupGovernorRole.ChairOfGovernors, null);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();

            var model = viewResult.Model as CreateEditGovernorViewModel;
            model.ShouldNotBeNull();
            model.GovernorRole.ShouldBe(eLookupGovernorRole.ChairOfGovernors);
            model.EstablishmentUrn.ShouldBe(estabUrn);
            model.GroupUId.ShouldBeNull();
        }

        [Test]
        public async Task Gov_AddEditOrReplace_GIDSpecified()
        {
            var estabUrn = 4;
            var governorId = 1032;

            var governor = new GovernorModel
            {
                Id = governorId,
                RoleId = (int)eLookupGovernorRole.Governor
            };

            GetMock<ControllerContext>().SetupGet(c => c.RouteData).Returns(new RouteData(new Route("", new PageRouteHandler("~/")), new PageRouteHandler("~/")));
            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorAsync(governorId, It.IsAny<IPrincipal>())).ReturnsAsync(() => governor);
            GetMock<ILayoutHelper>().Setup(l => l.PopulateLayoutProperties(It.IsAny<CreateEditGovernorViewModel>(), estabUrn, null, It.IsAny<IPrincipal>(), It.IsAny<Action<EstablishmentModel>>(), It.IsAny<Action<GroupModel>>())).Returns(Task.CompletedTask);
            GetMock<IGovernorsReadService>().Setup(g => g.GetEditorDisplayPolicyAsync(eLookupGovernorRole.Governor, false, It.IsAny<IPrincipal>())).ReturnsAsync(() => new GovernorDisplayPolicy());
            SetupCachedLookupService();

            var result = await ObjectUnderTest.AddEditOrReplace(null, estabUrn, null, 1032);

            var viewResult = result as ViewResult;
            viewResult.ShouldNotBeNull();

            var model = viewResult.Model as CreateEditGovernorViewModel;
            model.ShouldNotBeNull();
            model.Mode.ShouldBe(CreateEditGovernorViewModel.EditMode.Edit);
        }

        [Test]
        public async Task Gov_AddEditOrReplace_RoleSpecified_Shared()
        {
            var estabUrn = 4;
            GetMock<IGovernorsReadService>().Setup(g => g.GetGovernorListAsync(estabUrn, null, It.IsAny<IPrincipal>())).ReturnsAsync(() => new GovernorsDetailsDto
            {
                CurrentGovernors = new List<GovernorModel>()
            });
            GetMock<ControllerContext>().SetupGet(c => c.RouteData).Returns(new RouteData(new Route("", new PageRouteHandler("~/")), new PageRouteHandler("~/")));

            var result = await ObjectUnderTest.AddEditOrReplace(null, estabUrn, eLookupGovernorRole.Establishment_SharedChairOfLocalGoverningBody, null);

            var redirectResult = result as RedirectToRouteResult;
            redirectResult.ShouldNotBeNull();
            redirectResult.RouteName.ShouldBe("SelectSharedGovernor");
        }

        [SetUp]
        public void SetUpTest() => SetupObjectUnderTest();

        [TearDown]
        public void TearDownTest() => ResetMocks();

        [OneTimeSetUp]
        protected override void InitialiseMocks()
        {
            AddMock<IGovernorsReadService>();
            AddMock<NomenclatureService>();
            AddMock<ICachedLookupService>();
            AddMock<IGovernorsWriteService>();
            AddMock<IGroupReadService>();
            AddMock<IEstablishmentReadService>();
            AddMock<ILayoutHelper>();
            base.InitialiseMocks();
        }
    }
}
