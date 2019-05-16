using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCStore.ViewModels
{
	public class LoginModel
	{
		[Required]
		public string Name { get; set; }
		[Required]
		[UIHint("password")]
		public string Password { get; set; }
		public string ReturnUrl { get; set; } = "/";
	}
}
