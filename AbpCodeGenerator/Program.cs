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
			className = Console.ReadLine();
			while (!className.Equals("S", StringComparison.InvariantCultureIgnoreCase))
			{
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

				CodeGeneratorHelper.SetListViewModelClass(className);
				CodeGeneratorHelper.SetIndexHtmlTemplate(className, metaTableInfoList);
				CodeGeneratorHelper.SetListDtoClass(className, metaTableInfoList);
				
				CodeGeneratorHelper.SetIndexJsTemplate(className, metaTableInfoList);
				break;
				CodeGeneratorHelper.AddNavigationMenu(className);
				

				// Geração do lado do servidor  
				CodeGeneratorHelper.SetAppServiceIntercafeClass(className, propertyType);
				CodeGeneratorHelper.SetAppServiceClass(className, propertyType);
				CodeGeneratorHelper.SetCreateOrEditInputClass(className, metaTableInfoList);
				CodeGeneratorHelper.SetGetForEditOutputClass(className);
				CodeGeneratorHelper.SetGetInputClass(className);
				
				CodeGeneratorHelper.SetCreateOrEditInputClass(className, metaTableInfoList);

				CodeGeneratorHelper.GeneretePageNameConsts(className);
				
				//CodeGeneratorHelper.SetExportingIntercafeClass(className);
				//CodeGeneratorHelper.SetExportingClass(className, metaTableInfoList);
				//CodeGeneratorHelper.SetConstsClass(className); Se você usa SetAppPermissions，SetAppAuthorizationProvider，SetZh_CN_LocalizationDictionary_Here, então pode usar este método

				CodeGeneratorHelper.SetAppPermissions(className);
				CodeGeneratorHelper.SetAppAuthorizationProvider(className);
				CodeGeneratorHelper.SetZh_CN_LocalizationDictionary_Here(className, metaTableInfoList[0].ClassAnnotation);

				////client
				CodeGeneratorHelper.SetControllerClass(className, propertyType);
				CodeGeneratorHelper.SetCreateOrEditHtmlTemplate(className, metaTableInfoList);
				CodeGeneratorHelper.SetCreateOrEditJs(className);
				CodeGeneratorHelper.SetCreateOrEditViewModelClass(className);
				

				Console.WriteLine("Informe o classe Model ou Tabela");
				Console.WriteLine("ou S para sair?");
				className = Console.ReadLine();
			}
			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
	}
}
