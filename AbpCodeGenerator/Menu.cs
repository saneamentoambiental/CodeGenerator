using AbpCodeGenerator.Lib;
using CommandLine;
using PowerArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AbpCodeGenerator
{
	public class EntityName
	{
		[ArgShortcut("-E"), ArgRequired(PromptIfMissing = true), ArgPosition(1), ArgDescription("Entity Name to Generate")]
		public string Name { get; set; }
	}
	[ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
	public class Menu
	{
		public static void Help(string help)
		{
			Console.WriteLine(help);
		}
		public static void Lookup(string EntityName)
		{
			CodeGeneratorHelper.AddLoockupMethodIntoService(EntityName);
		}
		
		public static void CreateNavigationMenu(string EntityName)
		{
			CodeGeneratorHelper.AddNavigationMenu(EntityName);
		}
		public static void CreateServiceClass(string EntityName)
		{
			var MetaTableInfoList = MetaTableInfo.GetMetaTableInfoListForAssembly(EntityName);

			//Obter o tipo de chave primária
			var propertyType = MetaTableInfoList.FirstOrDefault(m => m.Name == "Id").PropertyType;

			// Geração do lado do servidor  
			CodeGeneratorHelper.SetAppServiceIntercafeClass(EntityName, propertyType);
			CodeGeneratorHelper.SetAppServiceClass(EntityName, propertyType);
		}
		public static void CreateDtos(string EntityName)
		{
			var MetaTableInfoList = MetaTableInfo.GetMetaTableInfoListForAssembly(EntityName);
			CodeGeneratorHelper.SetListDtoClass(EntityName, MetaTableInfoList);
			CodeGeneratorHelper.SetCreateOrEditInputClass(EntityName, MetaTableInfoList);
			CodeGeneratorHelper.SetGetForEditOutputClass(EntityName);
			CodeGeneratorHelper.SetGetInputClass(EntityName, MetaTableInfoList);
		}

		public static void CreatePageConsts(string EntityName)
		{
			CodeGeneratorHelper.GeneretePageNameConsts(EntityName);
		}
		//CodeGeneratorHelper.SetExportingIntercafeClass(EntityName);
		//CodeGeneratorHelper.SetExportingClass(EntityName, metaTableInfoList);
		//CodeGeneratorHelper.SetConstsClass(EntityName); Se você usa SetAppPermissions，SetAppAuthorizationProvider，SetZh_CN_LocalizationDictionary_Here, então pode usar este método
		public static void SetPermissions(string EntityName)
		{
			CodeGeneratorHelper.SetAppPermissions(EntityName);
			CodeGeneratorHelper.SetAppAuthorizationProvider(EntityName);
		}
		public static void CreateLocalization(string EntityName)
		{
			var MetaTableInfoList = MetaTableInfo.GetMetaTableInfoListForAssembly(EntityName);
			CodeGeneratorHelper.setLocalizationKeys(EntityName, MetaTableInfoList);
		}

		[ArgActionMethod, ArgDescription("Create client objects"), ArgShortcut("lookup")]
		public static void CreateClientControllerAndViews(EntityName EntityName)
		{
			var MetaTableInfoList = MetaTableInfo.GetMetaTableInfoListForAssembly(EntityName.Name);

			//Obter o tipo de chave primária
			var propertyType = MetaTableInfoList.FirstOrDefault(m => m.Name == "Id").PropertyType;
			CodeGeneratorHelper.SetCreateOrEditHtmlTemplate(EntityName.Name, MetaTableInfoList);
			return; 

		}

		public void Main()
		{
			Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<Menu>());
			
			CreateClientControllerAndViews(new EntityName { Name = "Pessoa" });
		}

		//[Verb("all", HelpText = "Generate all itens to work with an Entity")]
		public static void All(
			//[System.ComponentModel.DataAnnotations.Required]
			//[Aliases("e")]
			//[System.ComponentModel.Description("Entity Name")]
			//string EntityName)
			)
		{
			var EntityName = Console.ReadLine();
			Lookup(EntityName);
			CreateNavigationMenu(EntityName);
			CreateServiceClass(EntityName);
			CreateDtos(EntityName);
			CreatePageConsts(EntityName);
			SetPermissions(EntityName);
			CreateLocalization(EntityName);
			CreateClientControllerAndViews(null);

		}
	}
}
