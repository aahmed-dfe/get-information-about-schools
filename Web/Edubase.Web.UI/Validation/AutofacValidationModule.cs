﻿using Autofac;
using Edubase.Web.UI.Areas.Establishments.Models;
using Edubase.Web.UI.Areas.Establishments.Models.Validators;
using Edubase.Web.UI.Areas.Governors.Models;
using Edubase.Web.UI.Areas.Governors.Models.Validators;
using Edubase.Web.UI.Models;
using Edubase.Web.UI.Models.Establishments;
using Edubase.Web.UI.Models.Establishments.Validators;
using Edubase.Web.UI.Models.Validators;
using FluentValidation;

namespace Edubase.Web.UI.Validation
{
    public class ValidationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EditEstablishmentModelValidator>()
                    .Keyed<IValidator>(typeof(IValidator<EditEstablishmentModel>))
                    .As<IValidator>();

            builder.RegisterType<CreateEditGovernorViewModelValidator>()
                    .Keyed<IValidator>(typeof(IValidator<CreateEditGovernorViewModel>))
                    .As<IValidator>();

            builder.RegisterType<GovernorsGridViewModelValidator>()
                    .Keyed<IValidator>(typeof(IValidator<GovernorsGridViewModel>))
                    .As<IValidator>();

            builder.RegisterType<ReplaceChairViewModelValidator>()
                .Keyed<IValidator>(typeof(IValidator<ReplaceChairViewModel>))
                .As<IValidator>();

            builder.RegisterType<EditGroupDelegationInformationViewModelValidator>()
                .Keyed<IValidator>(typeof(IValidator<EditGroupDelegationInformationViewModel>))
                .As<IValidator>();

            builder.RegisterType<SelectSharedGovernorViewModelValidator>()
                .Keyed<IValidator>(typeof(IValidator<SelectSharedGovernorViewModel>))
                .As<IValidator>();

            builder.RegisterType<BulkUpdateViewModelValidator>()
                .Keyed<IValidator>(typeof(IValidator<BulkUpdateViewModel>))
                .As<IValidator>();

            builder.RegisterType<CreateEstablishmentViewModelValidator>()
                .Keyed<IValidator>(typeof(IValidator<CreateEstablishmentViewModel>))
                .As<IValidator>();

            builder.RegisterType<ChangeHistoryViewModelValidator>()
                .Keyed<IValidator>(typeof(IValidator<ChangeHistoryViewModel>))
                .As<IValidator>();

            builder.RegisterType<DateTimeViewModelValidator>()
                .Keyed<IValidator>(typeof(IValidator<DateTimeViewModel>))
                .As<IValidator>();
        }
    }
}