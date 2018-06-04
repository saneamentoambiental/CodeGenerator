using System;
using System.IO;
using System.Text;
using System.Linq;
using AbpCodeGenerator.Lib;
using Microsoft.Extensions.Configuration;

namespace AbpCodeGenerator
{
	class Program
	{

		static void Main(string[] args)
		{

			Console.WriteLine("Informe o classe Model ou Tabela");
			string className = "X";
			className = "Pessoa"; // Console.ReadLine();
			while (!string.IsNullOrWhiteSpace(className) &&  !className.Equals("S", StringComparison.InvariantCultureIgnoreCase))
			{
				GenerateOptions options = getOptionsFor(className);
				#region Duas maneiras de obter uma fonte de dados mysql e assembly de reflexão
				// mysql
				// string tableName = "abpusers"; // nome da tabela
				// var metaTableInfoList = MetaTableInfo.GetMetaTableInfoListForMysql (tableName);

				// Reflete a montagem para gerar o código correspondente
				//跟类名保持一致
				var metaTableInfoList = MetaTableInfo.GetMetaTableInfoListForAssembly(className);

				#endregion

				//Obter o tipo de chave primária
				var propertyType = metaTableInfoList.FirstOrDefault(m => m.Name == "Id").PropertyType;


				if (options.CreateLookup)
				{
					CodeGeneratorHelper.AddLoockupMethodIntoService(className);
				}

				//break;
				if (options.CreateNavigationMenu)
				{
					CodeGeneratorHelper.AddNavigationMenu(className);
				}

				if (options.CreateServiceClass)
				{
					// Geração do lado do servidor  
					CodeGeneratorHelper.SetAppServiceIntercafeClass(className, propertyType);
					CodeGeneratorHelper.SetAppServiceClass(className, propertyType);
				}
				if (options.CreateDtos)
				{
					CodeGeneratorHelper.SetListDtoClass(className, metaTableInfoList);
					CodeGeneratorHelper.SetCreateOrEditInputClass(className, metaTableInfoList);
					CodeGeneratorHelper.SetGetForEditOutputClass(className);
					CodeGeneratorHelper.SetGetInputClass(className, metaTableInfoList);
				}
				if (options.CreatePageConsts)
				{
					CodeGeneratorHelper.GeneretePageNameConsts(className);
				}
				//CodeGeneratorHelper.SetExportingIntercafeClass(className);
				//CodeGeneratorHelper.SetExportingClass(className, metaTableInfoList);
				//CodeGeneratorHelper.SetConstsClass(className); Se você usa SetAppPermissions，SetAppAuthorizationProvider，SetZh_CN_LocalizationDictionary_Here, então pode usar este método
				if (options.SetPermissions)
				{
					CodeGeneratorHelper.SetAppPermissions(className);
					CodeGeneratorHelper.SetAppAuthorizationProvider(className);
				}
				if (options.CreateLoacalization)
				{
					CodeGeneratorHelper.setLocalizationKeys(className, metaTableInfoList);
				}
				if (options.CreateClientControllerAndViews)
				{
					CodeGeneratorHelper.SetControllerClass(className, propertyType);
					CodeGeneratorHelper.SetCreateOrEditHtmlTemplate(className, metaTableInfoList);
					CodeGeneratorHelper.SetCreateOrEditJs(className);
					CodeGeneratorHelper.SetCreateOrEditViewModelClass(className);
					CodeGeneratorHelper.SetListViewModelClass(className);
					CodeGeneratorHelper.SetIndexHtmlTemplate(className, metaTableInfoList);
					CodeGeneratorHelper.SetIndexJsTemplate(className, metaTableInfoList);
				}

				Console.WriteLine("Informe o classe Model ou Tabela");
				Console.WriteLine("ou S para sair?");
				className = Console.ReadLine();
			}
			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
		public static void gerarDataTableUtilClass()
		{
			CodeGeneratorHelper.CreateDataTableWrapper();
			throw new NotImplementedException();
		}
		private static GenerateOptions getOptionsFor(string className)
		{
			return new GenerateOptions
			{
				CreateLoacalization = false,
				CreateNavigationMenu = false,
				CreatePageConsts = false,
				SetPermissions = false,
				CreateDtos = false,
				CreateServiceClass = false,
				CreateClientControllerAndViews = false,
				CreateLookup = true
			};
		}
	}
}
