﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Ninject;
using Revo.Core.Security;
using Revo.Platforms.AspNet.Core;

namespace Revo.Platforms.AspNet.Security.Mvc
{
    public class AuthorizePermissionsAttribute : AuthorizeAttribute
    {
        private readonly Guid[] permissionIds;
        private Permission[] requiredPermissions;

        public AuthorizePermissionsAttribute(params string[] permissionIds)
        {
            this.permissionIds = permissionIds.Select(x => Guid.Parse(x)).ToArray();
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            IPrincipal user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false; //TODO: allow anonymous permissions
            }

            bool isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            if (user.Identity is ClaimsIdentity claimsIdentity)
            {
                IKernel kernel = RevoHttpApplication.Current.Kernel;
                IPermissionTypeRegistry permissionCache = kernel.Get<IPermissionTypeRegistry>();

                if (requiredPermissions == null)
                {
                    requiredPermissions = permissionIds.Select(x => new Permission(
                        permissionCache.GetPermissionTypeById(x), null, null)).ToArray();
                }

                IPermissionAuthorizer authorizer = kernel.Get<IPermissionAuthorizer>();
                return authorizer.CheckAuthorization(claimsIdentity, requiredPermissions);
            }
            else
            {
                // only claim-based identities are supported for permission authorization
                return false;
            }
        }
    }
}
