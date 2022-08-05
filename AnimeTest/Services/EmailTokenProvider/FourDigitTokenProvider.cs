using System;
using System.Globalization;
using System.Threading.Tasks;
using AnimeBack.Entities;
using Microsoft.AspNetCore.Identity;

namespace AnimeBack.Services.EmailTokenProvider
{
    public class FourDigitTokenProvider : PhoneNumberTokenProvider<User>
    {
        
		public static string FourDigitPhone = "4DigitPhone";
		public static string FourDigitEmail = "4DigitEmail";

		public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<User> manager, User user)
		{
			return Task.FromResult(false);
		}

		public override async Task<string> GenerateAsync(string purpose, UserManager<User> manager, User user)
		{
			var token = new SecurityToken(await manager.CreateSecurityTokenAsync(user));
			var modifier = await GetUserModifierAsync(purpose, manager, user);
			var code = Rfc6238AuthenticationService.GenerateCode(token, modifier, 6).ToString("D4", CultureInfo.InvariantCulture);

			return code;
		}
		public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager, User user)
		{
			int code;
			if (!Int32.TryParse(token, out code))
			{
				return false;
			}
			var securityToken = new SecurityToken(await manager.CreateSecurityTokenAsync(user));
			var modifier = await GetUserModifierAsync(purpose, manager, user);
			var valid = Rfc6238AuthenticationService.ValidateCode(securityToken, code, modifier, token.Length);
			return valid;
		}
		public override Task<string> GetUserModifierAsync(string purpose, UserManager<User> manager, User user)
		{
			return base.GetUserModifierAsync(purpose, manager, user);
		}
	}
    }
