﻿using Ninject.Modules;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Security.Commands;

namespace Revo.Infrastructure.Security
{
    public class SecurityInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEntityQueryAuthorizer>()
                .To<EntityQueryAuthorizer>()
                .InRequestOrJobScope();

            Bind<IEntityQueryFilterFactory>()
                .To<EntityQueryFilterFactory>()
                .InRequestOrJobScope();

            Bind<ICommandPermissionCache, IApplicationStartListener>()
                .To<CommandPermissionCache>()
                .InSingletonScope();

            Bind<IPreCommandFilter<ICommandBase>>()
                .To<CommandPermissionAuthorizer>()
                .InTransientScope();

            Bind<ITokenValidator>()
                .To<TokenValidator>()
                .InTransientScope();
        }
    }
}
