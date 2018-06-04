using System;
using System.Collections.Generic;
using System.Text;

namespace AbpCodeGenerator
{
	public class GenerateOptions
	{
		public bool CreateNavigationMenu { get; set; }
		public bool CreateServiceClass { get; set; }
		public bool CreateDtos { get; set; }
		public bool CreatePageConsts { get; set; }
		public bool SetPermissions { get; set; }
		public bool CreateLoacalization { get; set; }
		public bool CreateClientControllerAndViews { get; set; }

		public bool CreateLookup { get; set; }
	}
}
