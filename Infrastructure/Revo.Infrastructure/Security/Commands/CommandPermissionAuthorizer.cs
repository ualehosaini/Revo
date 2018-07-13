﻿using System.Threading.Tasks;
using Revo.Core.Commands;
using Revo.Core.Security;

namespace Revo.Infrastructure.Security.Commands
{
    public class CommandPermissionAuthorizer : CommandAuthorizer<ICommandBase>
    {
        private readonly ICommandPermissionCache commandPermissionCache;
        private readonly IPermissionAuthorizer permissionAuthorizer;
        private readonly IUserContext userContext;

        public CommandPermissionAuthorizer(ICommandPermissionCache commandPermissionCache,
            IPermissionAuthorizer permissionAuthorizer,
            IUserContext userContext)
        {
            this.commandPermissionCache = commandPermissionCache;
            this.permissionAuthorizer = permissionAuthorizer;
            this.userContext = userContext;
        }

        protected override async Task AuthorizeCommand(ICommandBase command)
        {
            var requiredPermissions = commandPermissionCache.GetCommandPermissions(command);
            var userPermissions = await userContext.GetPermissionsAsync();

            if (!permissionAuthorizer.CheckAuthorization(userPermissions, requiredPermissions))
            {
                throw new AuthorizationException(
                    $"User not authorized to access command of type '{command.GetType().FullName}'");
            }
        }
    }
}
