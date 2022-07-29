using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Microsoft.Owin.Security;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using xDC.Services;
using xDC_Web.Models;

namespace xDC_Web.Extension
{
	public class AdAuthentication
	{
		private readonly IAuthenticationManager _authManager;

		public AdAuthentication(IAuthenticationManager authenticationManager)
		{
			this._authManager = authenticationManager;
		}

		public AuthenticationResult SignIn(LoginViewModel model)
		{
			//#if DEBUG
			// authenticates against your local machine - for development time
			// ContextType authenticationType = ContextType.Machine;
			//#else
			// authenticates against your Domain AD
			ContextType authenticationType = ContextType.Domain;
			//#endif

			try
			{
				//var principalContext = new PrincipalContext(authenticationType, Utils.Config.AdDomain, Utils.Config.AdUsername, Utils.Config.AdPass);
				var principalContext = new PrincipalContext(authenticationType);
				var isAuthenticated = false;
				var userPrincipal = new UserPrincipal(principalContext);
				userPrincipal.SamAccountName = model.Username;
				var searcher = new PrincipalSearcher(userPrincipal);
				try
				{
					userPrincipal = searcher.FindOne() as UserPrincipal;
					if (userPrincipal != null)
					{
						isAuthenticated = principalContext.ValidateCredentials(model.Username, model.Password, ContextOptions.Negotiate);
					}
				}
				catch (Exception ex)
				{
					return new AuthenticationResult("Incorrect Credential");
				}

				if (!isAuthenticated)
				{
					return new AuthenticationResult("Incorrect Credential");
				}

				if (userPrincipal.IsAccountLockedOut())
				{
					return new AuthenticationResult("Account Locked. AD Level.");
				}

				if (userPrincipal.Enabled.HasValue && userPrincipal.Enabled.Value == false)
				{
					return new AuthenticationResult("Account Disabled. AD Level.");
				}

				if (!new AuthService().IsUserExist(model.Username))
				{
					return new AuthenticationResult("You did not have access into the system.");
				}

				var identity = CreateIdentity(userPrincipal);

				_authManager.SignOut(xDcAuth.ApplicationCookie);
				_authManager.SignIn(new AuthenticationProperties() { IsPersistent = false }, identity);

				TrackerService.TrackUserLogin(model.Username, model.IpAddress, model.ClientBrowser);

				return new AuthenticationResult();
			}
			catch (Exception ex)
			{
				return new AuthenticationResult(ex.Message);
			}
		}


		private ClaimsIdentity CreateIdentity(UserPrincipal userPrincipal)
		{
			var identity = new ClaimsIdentity(xDcAuth.ApplicationCookie, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
			identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "Active Directory"));
			identity.AddClaim(new Claim(ClaimTypes.Name, userPrincipal.SamAccountName));
			identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userPrincipal.SamAccountName));
			if (!String.IsNullOrEmpty(userPrincipal.EmailAddress))
			{
				identity.AddClaim(new Claim(ClaimTypes.Email, userPrincipal.EmailAddress));
			}

			// add your own claims if you need to add more information stored on the cookie
			identity.AddClaim(new Claim(ClaimTypes.Role, new AuthService().GetUserRoles(userPrincipal.SamAccountName)));

			return identity;
		}
	}

	public class AuthenticationResult
	{
		public AuthenticationResult(string errorMessage = null)
		{
			ErrorMessage = errorMessage;
		}

		public String ErrorMessage { get; private set; }
		public Boolean IsSuccess => String.IsNullOrEmpty(ErrorMessage);
	}
}