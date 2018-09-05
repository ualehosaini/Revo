﻿using Ninject.Modules;
using Revo.AspNetCore.Core;
using Revo.AspNetCore.Ninject;
using Revo.Core.Core;
using Revo.Hangfire;

namespace Revo.AspNetCore
{
    [AutoLoadModule(false)]
    public class AspNetCoreModule : NinjectModule
    {
        private readonly HangfireConfigurationSection hangfireConfigurationSection;

        public AspNetCoreModule(HangfireConfigurationSection hangfireConfigurationSection)
        {
            this.hangfireConfigurationSection = hangfireConfigurationSection;
        }

        public override void Load()
        {
            Bind<IConfiguration>()
                .ToMethod(ctx => LocalConfiguration.Current)
                .InTransientScope();

            Bind<IActorContext>()
                .To<UserActorContext>()
                .InRequestOrJobScope();

            Bind<IServiceLocator>()
                .To<NinjectServiceLocator>()
                .InSingletonScope();

            if (hangfireConfigurationSection.IsActive)
            {
                Bind<IAspNetCoreStartupConfigurer>()
                    .To<HangfireStartupConfigurator>()
                    .InSingletonScope();
            }
        }
    }
}
