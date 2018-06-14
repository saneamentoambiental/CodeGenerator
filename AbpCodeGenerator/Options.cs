using AbpCodeGenerator.Lib;
using CommandLine;
using Plossum.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using CommandLine;
//using PowerArgs;
//using CommandLine.Text;
//using Plossum.CommandLine;

namespace AbpCodeGenerator
{
	/// <summary>
	/// <see cref="https://www.codeproject.com/Articles/19869/Powerful-and-simple-command-line-parsing-in-C"/>
	/// <see cref="https://github.com/adamabdelhamed/PowerArgs"/>
	/// <see cref="http://adrianaisemberg.github.io/CLAP/#what"/>
	/// <see cref="https://github.com/commandlineparser/commandline"/>
	/// <see cref="https://codereview.stackexchange.com/questions/142627/optimizing-simple-menu-in-console-app?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa"></see>
	/// </summary>
	//[ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
	[CommandLineManager(ApplicationName = "AspNetBoilerPlate Generator",
	Copyright = "Copyright (c) GPSA")]
	[CommandLineOptionGroup("c", Name = "Required")]
	[CommandLineOptionGroup("o", Name = "Options")]
	public class Options
	{
#if DEBUG
		const BoolFunction ValueForBoolFunction = BoolFunction.TrueIfPresent;
#else
		const BoolFunction ValueForBoolFunction = BoolFunction.FalseIfPresent;
#endif


		[CommandLineOption(Aliases = "t", Description = "Create DataTable Extensions", BoolFunction = ValueForBoolFunction, GroupId = "o")]
		public bool CreateDataTable { get; set; }

		[CommandLineOption(Aliases = "m", Description = "Add Menu Navigation", BoolFunction = ValueForBoolFunction, GroupId = "o")]
		public bool CreateNavigationMenu { get; set; }
		[CommandLineOption(Aliases = "s", Description = "Create Service", BoolFunction = ValueForBoolFunction, GroupId = "o")]
		public bool CreateServiceClass { get; set; }
		[CommandLineOption(Aliases = "d", Description = "Create DTO", BoolFunction = ValueForBoolFunction, GroupId = "o")]
		public bool CreateDtos { get; set; }
		[CommandLineOption(Aliases = "c", Description = "Create Consts used into permissions and menu", BoolFunction = ValueForBoolFunction, GroupId = "o")]
		public bool CreatePageConsts { get; set; }
		[CommandLineOption(Aliases = "p", Description = "Set permissions", BoolFunction = ValueForBoolFunction, GroupId = "o")]
		public bool SetPermissions { get; set; }
		[CommandLineOption(Aliases = "l", Description = "Add localizations keys", BoolFunction = ValueForBoolFunction, GroupId = "o")]
		public bool CreateLoacalization { get; set; }
		[CommandLineOption(Aliases = "w", Description = "Create Client Controller And Views", BoolFunction = ValueForBoolFunction, GroupId = "o")]
		public bool CreateClientControllerAndViews { get; set; }
		[CommandLineOption(Aliases = "k", Description = "Create Lookup", BoolFunction = ValueForBoolFunction, GroupId = "o")]
		public bool CreateLookup { get; set; }
		string entityName;
		[CommandLineOption(Aliases = "e", Description = "Specifies the Entity to generate", MinOccurs = 1, GroupId = "c")]
		public string EntityName
		{
			get => entityName; set
			{
				this.entityName = value;
				MetaTableInfoList = MetaTableInfo.GetMetaTableInfoListForAssembly(entityName);
				if (MetaTableInfoList.Count == 0)
				{
					throw new InvalidOptionValueException($"The entity {value} not exists or properties for {value} not found!", false);
				}
			}
		}


		[CommandLineOption(Aliases = "h", Description = "Displays help text", BoolFunction = BoolFunction.TrueIfPresent, GroupId = "o")]
		public bool Help { get; set; }
		public void ExecuteCommand()
		{
			this.EntityName = this.EntityName.Trim();
			Console.WriteLine("You entered string '{0}'", this.EntityName);
			//Obter o tipo de chave primária
			var propertyType = MetaTableInfoList.FirstOrDefault(m => m.Name == "Id").PropertyType;

			if (this.CreateDataTable)
			{
				CodeGeneratorHelper.CreateDataTableWrapper();
			}
			if (this.CreateLookup)
			{
				CodeGeneratorHelper.AddLoockupMethodIntoService(EntityName);
			}

			if (this.CreateNavigationMenu)
			{
				CodeGeneratorHelper.AddNavigationMenu(EntityName);
			}

			if (this.CreateServiceClass)
			{
				// Geração do lado do servidor  
				CodeGeneratorHelper.SetAppServiceIntercafeClass(EntityName, propertyType);
				CodeGeneratorHelper.SetAppServiceClass(EntityName, propertyType);
			}
			if (this.CreateDtos)
			{
				CodeGeneratorHelper.SetListDtoClass(EntityName, this.MetaTableInfoList);
				CodeGeneratorHelper.SetCreateOrEditInputClass(EntityName, this.MetaTableInfoList);
				CodeGeneratorHelper.SetGetForEditOutputClass(EntityName);
				CodeGeneratorHelper.SetGetInputClass(EntityName, this.MetaTableInfoList);
			}
			if (this.CreatePageConsts)
			{
				CodeGeneratorHelper.GeneretePageNameConsts(EntityName);
			}
			if (this.SetPermissions)
			{
				CodeGeneratorHelper.SetAppPermissions(EntityName);
				CodeGeneratorHelper.SetAppAuthorizationProvider(EntityName);
			}
			if (this.CreateLoacalization)
			{
				CodeGeneratorHelper.setLocalizationKeys(EntityName, this.MetaTableInfoList);
			}

			if (this.CreateClientControllerAndViews)
			{
				CodeGeneratorHelper.SetCreateOrEditHtmlTemplate(EntityName, this.MetaTableInfoList);
				CodeGeneratorHelper.SetControllerClass(EntityName, propertyType);
				CodeGeneratorHelper.SetCreateOrEditJs(EntityName);
				CodeGeneratorHelper.SetCreateOrEditViewModelClass(EntityName);
				CodeGeneratorHelper.SetListViewModelClass(EntityName);
				CodeGeneratorHelper.SetIndexHtmlTemplate(EntityName, this.MetaTableInfoList);
				CodeGeneratorHelper.SetIndexJsTemplate(EntityName, this.MetaTableInfoList);
			}
		}
		protected List<MetaTableInfo> MetaTableInfoList
		{
			get; set;

		}
	}
}
