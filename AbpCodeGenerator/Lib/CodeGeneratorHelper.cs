using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using CLAP;

namespace AbpCodeGenerator.Lib
{
	public class CodeGeneratorHelper
	{
		


		#region Show data Grid 

		/// <summary>
		/// 生成CreateOrEditViewModelClass
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetListViewModelClass(string className)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Client\Mvc\ViewModelClass\ListViewModel.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);
			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 ;
			Write(Path.Combine(Configuration.Web_Mvc_Directory, "Areas", Configuration.App_Area_Name, "Models", className + "s", className + "ListViewModel.cs"), templateContent);
		}

		#endregion

		#region client

		[Verb]
		public static void AddNavigationMenu(string className)
		{
			//{{Item_Menu_Template}

			string template = Configuration.RootDirectory + @"\Client\Mvc\Startup\ItemMenuTemplate.txt";
			var templateContent = Read(template);

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{Permission_Name_Here}}", $"Pages_{Configuration.App_Area_Name}_{className}")
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 .Replace("{{Project_Name_Here}}", Configuration.Controller_Base_Class)
											 .Replace("{{entity_Name_Plural_Here}}", GetFirstToLowerStr(className))
											 ;
			var path = Path.Combine(Configuration.Web_Mvc_Directory, "Startup", Configuration.Controller_Base_Class + "NavigationProvider.cs");
			var menuFile = Read(path);
			menuFile = menuFile.Replace("//{{Item_Menu_Template}", templateContent);
			Write(path, menuFile);

		}
		/// <summary>
		/// 生成ControllerClass
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetControllerClass(string className, string primary_Key_Here)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Client\Mvc\ControllerClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{Permission_Name_Here}}", $"Pages_{Configuration.App_Area_Name}_{className}")
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 .Replace("{{Primary_Key_Here}}", primary_Key_Here)
											 .Replace("{{Project_Name_Here}}", Configuration.Controller_Base_Class)
											 .Replace("{{entity_Name_Plural_Here}}", GetFirstToLowerStr(className))
											 ;
			Write(Path.Combine(Configuration.Web_Mvc_Directory, "Areas", Configuration.App_Area_Name, "Controllers", className + "Controller.cs"), templateContent);
		}

		[Verb]
		public static void CreateDataTableWrapper()
		{
			string dest = Path.Combine(Configuration.SourceSolution, "GPSA.ETESystem.Core", "Web", "DataTableNet", "DataTableWrapperExtensions.cs");
			string templatePath = Path.Combine(Configuration.RootDirectory, "DataTableWrapperExtensions.txt");
			var templateContent = Read(templatePath);

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 .Replace("{{Project_Name_Here}}", Configuration.Controller_Base_Class)
											 ;
			Write(dest, templateContent);
		}


		/// <summary>
		/// 生成CreateOrEditHtmlTemplate
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetCreateOrEditHtmlTemplate(string className, List<MetaTableInfo> metaTableInfoList)
		{
			var dir = Configuration.RootDirectory + @"\Client\Mvc\CreateOrEditHtmlTemplate\";
			string templateFilePath = dir + "MainTemplate.txt";
			var templateContent = Read(templateFilePath);

			StringBuilder sb = new StringBuilder();

			foreach (var field in metaTableInfoList)
			{
				if (field.IsAuditableField || field.IsIdField)
					continue;


				var fieldTemplatePath = "SelectTemplateField.txt";
				if (field.PropertyInfo.PropertyType.IsSimpleType())
				{
					fieldTemplatePath = "FieldTemplate.txt";
				}
				fieldTemplatePath = Path.Combine(dir, fieldTemplatePath);
				var fieldTemplate = Read(fieldTemplatePath);
				var required = getRequiredsFieldList(className, field);
				var cssClass = getCssClass(className, field);
				sb.Append(fieldTemplate
					.Replace("{{Field_Name}}", field.Name)
					.Replace("{{Field_Attributes}}", required)
					.Replace("{{CSS}}", cssClass)
					);
			}


			var property_Looped_Template_Here = sb.ToString();

			templateContent = templateContent.Replace("{{Property_Looped_Template_Here}}", property_Looped_Template_Here)
											 .Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 .Replace("{{entity_Name_Plural_Here}}", GetFirstToLowerStr(className))
											 ;
			Write(Path.Combine(Configuration.Web_Mvc_Directory, "Areas", Configuration.App_Area_Name, "Views", className, "_CreateOrEdit.cshtml"), templateContent);
		}


		private static string getCssClass(string className, MetaTableInfo field)
		{
			var type = field.PropertyInfo.PropertyType;
			List<String> items = new List<string>();
			if (type.IsDate())
			{
				items.Add("date");
			}
			if (type.IsTime())
			{
				items.Add("time24");
			}
			if (type.IsDecimal())
			{
				items.Add("number");
			}
			if (field.Annotation.ToLower().Contains("email"))
			{
				items.Add("email");
			}
			return string.Join(' ', items);

		}

		private static string getRequiredsFieldList(string className, MetaTableInfo field)
		{
			List<String> validation = new List<string>();
			var keys = new String[] { "Required", "MinLength", "MaxLength", "EmailAddress", "Phone", "StringLength" };
			var values = new String[] { "required", "min=\"@{0}.{1}MinLength\"", "max=\"@{0}.{1}MaxLength\"", "email", "phone", "maxlength=\"@{0}.{1}MaxLength\"" };
			for (int i = 0; i < keys.Length; i++)
			{
				if (field.Annotation.Contains(keys[i]))
				{
					validation.Add(String.Format(values[i], className, field.Name));
				}
			}
			if (field.PropertyInfo.Equals(typeof(DateTime)))
			{
				validation.Add("date");
			}
			if (field.PropertyInfo.PropertyType.IsDecimal())
			{
				validation.Add("numeric");
			}
			if (field.PropertyInfo.PropertyType.IsInteger())
			{
				validation.Add("digits");
			}
			return string.Join(' ', validation);
		}


		/// <summary>
		/// 生成CreateOrEditJs
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetCreateOrEditJs(string className)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Client\Mvc\CreateOrEditJs\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{entity_Name_Here}}", GetFirstToLowerStr(className))
											 .Replace("{{entity_Name_Plural_Here}}", GetFirstToLowerStr(className))
											 ;
			Write(Path.Combine(Configuration.Web_Mvc_Directory, "wwwroot\\view-resources\\Areas", Configuration.App_Area_Name, "Views", className, "_CreateOrEdit.js"), templateContent);
		}



		/// <summary>
		/// 生成CreateOrEditViewModelClass
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetCreateOrEditViewModelClass(string className)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Client\Mvc\CreateOrEditViewModelClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);
			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 ;
			Write(Path.Combine(Configuration.Web_Mvc_Directory, "Areas", Configuration.App_Area_Name, "Models", className + "s", "CreateOrEdit" + className + "ModalViewModel.cs"), templateContent);
		}


		[Verb]
		public static void GeneretePageNameConsts(string className)
		{
			var path = Path.Combine(Configuration.Web_Mvc_Directory, "Startup", "PageNames.cs");
			if (!File.Exists(path))
			{
				if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.Create(path).Close();
				string pageNamesConsts_CreateClassTemplate = Configuration.RootDirectory + @"\Client\Mvc\Startup\PagaNames_CreateClassTemplate.txt";
				var templateContentFile = Read(pageNamesConsts_CreateClassTemplate);
				templateContentFile = templateContentFile.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
												 .Replace("{{Namespace_Relative_Full_Here}}", className)
												 .Replace("{{Entity_Name_Plural_Here}}", className)
												 .Replace("{{Entity_Name_Here}}", className)
												 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
												 ;
				Write(path, templateContentFile);
			}

			string pageNames_ItemPageTemplate = Configuration.RootDirectory + @"\Client\Mvc\Startup\PageNames_ItemPageTemplate.txt";
			var pageNames_ItemPageTemplateFile = Read(pageNames_ItemPageTemplate);
			pageNames_ItemPageTemplateFile = pageNames_ItemPageTemplateFile.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 ;
			var templateContent = Read(path);
			templateContent = templateContent.Replace("//{{Template_Page_Name_Consts}}", pageNames_ItemPageTemplateFile);
			Write(path, templateContent);

		}

		/// <summary>
		/// 生成IndexHtmlTemplate
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetIndexHtmlTemplate(string className, List<MetaTableInfo> metaTableInfoList)
		{
			var directory = Configuration.RootDirectory + @"\Client\Mvc\IndexHtmlTemplate\";
			string templatePath = Path.Combine(directory, "MainTemplate.txt");
			var templateContent = Read(templatePath);

			string templateColumnTitle = Read(Path.Combine(directory, "Show_Columns_Title_Template.txt"));
			string templateColumnField = Read(Path.Combine(directory, "Show_Columns_Field_Template.txt"));

			StringBuilder sbColumnTitle = new StringBuilder();
			StringBuilder sbColumnField = new StringBuilder();

			foreach (var item in metaTableInfoList)
			{
				if (item.IsAuditableField)
					continue;
				var title = templateColumnTitle.Replace("{{Entity_Field_Name}}", item.Name);
				var field = templateColumnField.Replace("{{Entity_Field_Name}}", item.Name);
				if (item.IsIdField)
				{
					sbColumnTitle.Insert(0, title);
					sbColumnField.Insert(0, field);
				}
				else
				{
					sbColumnTitle.AppendLine(title);
					sbColumnField.AppendLine(field);
				}
			}
			var property_Looped_Template_Here = sbColumnTitle.ToString();

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 .Replace("{{Property_Looped_Template_Here}}", property_Looped_Template_Here)
											 .Replace("{{Permission_Name_Here}}", $"Pages_{Configuration.App_Area_Name}_{className}")
											 .Replace("{{Show_Columns_Title_Here}}", sbColumnTitle.ToString())
											 .Replace("{{Show_Columns_Field_Here}}", sbColumnField.ToString())
											 ;
			Write(Path.Combine(Configuration.Web_Mvc_Directory, "Areas", Configuration.App_Area_Name, "Views", className, "Index.cshtml"), templateContent);
		}


		/// <summary>
		/// 生成IndexJsTemplate
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetIndexJsTemplate(string className, List<MetaTableInfo> metaTableInfoList)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Client\Mvc\IndexJsTemplate\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);

			StringBuilder sb = new StringBuilder();
			var i = 2;
			foreach (var item in metaTableInfoList)
			{
				if (item.IsAuditableField)
					continue;
				//TODO: Verificar se o json é gerado na forma { target: i, data: "coluna" }
				string column = Abp.Json.JsonExtensions.ToJsonString(new
				{
					targets = item.IsIdField? 1: i,
					data = item.Name.ToLower()
				});
				sb.AppendLine(column + ",");
				i++;
			}
			var property_Looped_Template_Here = sb.ToString();
			templateContent = templateContent
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{entity_Name_Here}}", GetFirstToLowerStr(className))
											 .Replace("{{entity_Name_Plural_Here}}", GetFirstToLowerStr(className))
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 .Replace("{{Property_Looped_Template_Here}}", property_Looped_Template_Here)
											 .Replace("{{Permission_Value_Here}}", $"Pages.{Configuration.App_Area_Name}.{className}")
											 ;
			Write(Path.Combine(Configuration.Web_Mvc_Directory, "wwwroot", "view-resources", "Areas", Configuration.App_Area_Name, "Views", className, "Index.js"), templateContent);
		}

		#endregion


		#region Server

		#region Lookup

		private static string GetLookupServiceFilePath(string className, bool replace = false)
		{
			string templateFilePath = Configuration.RootDirectory + @"\Server\LookupService\TemplateMaster.txt";
			var templateContent = Read(templateFilePath);
			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)
											 ;
			var destPath = Path.Combine(Configuration.Application_Directory, "ILookupAppService.cs");
			if (!File.Exists(destPath) || replace)
				Write(destPath, templateContent);

			return destPath;
		}

		[Verb]
		public static void AddLoockupMethodIntoService(string className)
		{
			string keyMethod = "Task<ListResultDto<ComboboxItemDto>> Get{{Entity_Name_Here}}ComboboxItems()";

			StringBuilder sbIntefaceMethod = new StringBuilder();
			sbIntefaceMethod.AppendLine(keyMethod + ";");
			sbIntefaceMethod.Append("//{{Interface_Method_Lookup}}");

			StringBuilder sbMethod = new StringBuilder();

			sbMethod.AppendLine($"public async "+keyMethod);
			sbMethod.AppendLine("{\n");
			sbMethod.AppendLine("\t var result = await {{entity_Name_Here}}Repository.GetAllListAsync();");
			sbMethod.AppendLine("\t return new ListResultDto<ComboboxItemDto>(");
			sbMethod.AppendLine("\t\t result.Select(r => new ComboboxItemDto(r.Id.ToString(), r.ToString())).ToList()");
			sbMethod.AppendLine("\t\t );");
			sbMethod.AppendLine("\t }");
			sbMethod.Append("//{{Method}}");

			StringBuilder sbRepositoryField = new StringBuilder();
			sbRepositoryField.AppendLine("private readonly IRepository<{{Entity_Name_Here}}, int> {{entity_Name_Here}}Repository;");
			sbRepositoryField.Append("//{{Repository_Field}}");

			StringBuilder sbRepositoryConstructorParameter = new StringBuilder();
			sbRepositoryConstructorParameter.AppendLine(", IRepository<{{Entity_Name_Here}}, int> {{entity_Name_Here}}Repository");
			sbRepositoryConstructorParameter.Append("//{{Repository_Constructor_Parameter}}");


			StringBuilder sbRepositoryConstructorBody = new StringBuilder();
			sbRepositoryConstructorBody.AppendLine("this.{{entity_Name_Here}}Repository = {{entity_Name_Here}}Repository;");
			sbRepositoryConstructorBody.Append("//{{Repository_Constructor_Body}}");

			var destPath = GetLookupServiceFilePath(className);
			var templateContent = Read(destPath);
			if (templateContent.Contains(keyMethod))
			{
				Console.WriteLine($"Não foi possível gerar o Lookup Service para o {className}, pois ele já existe.");
			}
			else
			{
				templateContent = templateContent.Replace("//{{Interface_Method_Lookup}}", sbIntefaceMethod.ToString())
												 .Replace("//{{Method}}", sbMethod.ToString())
												 .Replace("//{{Repository_Field}}", sbRepositoryField.ToString())
												 .Replace("//{{Repository_Constructor_Parameter}}", sbRepositoryConstructorParameter.ToString())
												 .Replace("//{{Repository_Constructor_Body}}", sbRepositoryConstructorBody.ToString())
												 ;

				templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
												 .Replace("{{Namespace_Relative_Full_Here}}", className)
												 .Replace("{{Entity_Name_Plural_Here}}", className)
												 .Replace("{{Entity_Name_Here}}", className)
												 .Replace("{{entity_Name_Here}}", GetFirstToLowerStr(className))
												 .Replace("{{App_Area_Name_Here}}", Configuration.App_Area_Name)												 
												 ;
				Write(destPath, templateContent);
			}
		}



		#endregion Lookup

		/// <summary>
		/// 生成接口信息
		/// </summary>
		/// <param name="className"></param>
		/// <param name="primary_Key_Inside_Tag_Here">主键类型</param>
		[Verb]
		public static void SetAppServiceIntercafeClass(string className, string primary_Key_Inside_Tag_Here)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Server\AppServiceIntercafeClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{Primary_Key_Inside_Tag_Here}}", primary_Key_Inside_Tag_Here)
											 ;
			Write(Path.Combine(Configuration.Application_Directory, className + "s"), "I" + className + "AppService.cs", templateContent);
		}

		/// <summary>
		/// 生成接口实现类信息
		/// </summary>
		/// <param name="className"></param>
		/// <param name="Primary_Key_Inside_Tag_Here">主键类型</param>
		[Verb]
		public static void SetAppServiceClass(string className, string Primary_Key_Inside_Tag_Here)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Server\AppServiceClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);
			var Primary_Key_With_Comma_Here = Primary_Key_Inside_Tag_Here;
			if (Primary_Key_Inside_Tag_Here != "int")
			{
				Primary_Key_With_Comma_Here = "," + Primary_Key_Inside_Tag_Here;
			}
			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{Primary_Key_Inside_Tag_Here}}", Primary_Key_Inside_Tag_Here)
											 .Replace("{{entity_Name_Here}}", GetFirstToLowerStr(className))
											 .Replace("{{Permission_Name_Here}}", $"Pages_{Configuration.App_Area_Name}_{className}")
											 .Replace("{{Project_Name_Here}}", Configuration.Controller_Base_Class)
											 .Replace("{{Application_AppServiceBase}}", Configuration.Application_AppServiceBase)
											 .Replace("{{Primary_Key_With_Comma_Here}}", Primary_Key_With_Comma_Here)
											 ;
			Write(Path.Combine(Configuration.Application_Directory, className + "s"), className + "AppService.cs", templateContent);
		}

		/// <summary>
		/// 生成Exporting接口信息
		/// </summary>
		/// <param name="className"></param>
		/// <param name="primary_Key_Inside_Tag_Here">主键类型</param>
		[Verb]
		public static void SetExportingIntercafeClass(string className)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Server\ExportingIntercafeClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Plural_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{entity_Name_Here}}", GetFirstToLowerStr(className))
											 ;
			Write(Path.Combine(Configuration.Application_Directory, className + "s\\Exporting\\"), "I" + className + "ListExcelExporter.cs", templateContent);
		}

		/// <summary>
		/// 生成ExportingClass
		/// </summary>
		/// <param name="className"></param>
		/// <param name="Primary_Key_Inside_Tag_Here">主键类型</param>
		[Verb]
		public static void SetExportingClass(string className, List<MetaTableInfo> metaTableInfoList)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Server\ExportingClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);
			StringBuilder excel_Header = new StringBuilder();
			StringBuilder excel_Objects = new StringBuilder();

			for (int i = 0; i < metaTableInfoList.Count; i++)
			{
				if (i == 0)
				{
					excel_Header.AppendLine($"\"{metaTableInfoList[i].Annotation }\",");
					excel_Objects.AppendLine($"_ => _.{metaTableInfoList[i].Name },");
				}
				else
				{
					var comma = string.Empty;
					if (i + 1 < metaTableInfoList.Count)
					{
						comma = ",";
					}
					//空格是为了排版 强迫症
					excel_Header.AppendLine($"                        \"{metaTableInfoList[i].Annotation }\"" + comma);
					excel_Objects.AppendLine($"                        _ => _.{metaTableInfoList[i].Name }" + comma);

				}
			}

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{entity_Name_Here}}", GetFirstToLowerStr(className))
											 .Replace("{{Permission_Name_Here}}", $"Pages_{Configuration.App_Area_Name}_{className}")
											 .Replace("{{Excel_Header}}", excel_Header.ToString())
											 .Replace("{{Excel_Objects}}", excel_Objects.ToString())
											 ;
			Write(Path.Combine(Configuration.Application_Directory, className + "s\\Exporting\\"), className + "ListExcelExporter.cs", templateContent);
		}

		#region Dtos

		private static string GetPropertiesForDTO(List<MetaTableInfo> metaTableInfoList, bool addIdField = true, bool addAuditableField = false)
		{
			StringBuilder sb = new StringBuilder();
			foreach (var item in metaTableInfoList)
			{
				if (!addAuditableField && item.IsAuditableField)
					continue;
				if (!addIdField && item.IsIdField)
					continue;
				sb.AppendLine(item.Annotation);
				sb.AppendLine("public " + item.PropertyType + " " + item.Name + " { get; set; }");
			}
			return sb.ToString();
		}

		/// <summary>
		/// 生成GetInputClass
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetGetInputClass(string className, List<MetaTableInfo> metaTableInfoList)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Server\Dtos\GetInputClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 ;
			Write(Path.Combine(Configuration.Application_Directory, className + "s", "Dtos"), "Get" + className + "Input.cs", templateContent);
		}


		/// <summary>
		/// 生成GetForEditOutputClass
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetGetForEditOutputClass(string className)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Server\Dtos\GetForEditOutputClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);

			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 ;
			Write(Path.Combine(Configuration.Application_Directory, className + "s", "Dtos"), "Get" + className + "ForEditOutput.cs", templateContent);
		}


		/// <summary>
		/// 生成ListDtoClass
		/// </summary>
		/// <param name="className"></param>
		/// <param name="metaTableInfoList"></param>
		[Verb]
		public static void SetListDtoClass(string className, List<MetaTableInfo> metaTableInfoList)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Server\Dtos\ListDtoClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);
			StringBuilder sb = new StringBuilder();

			foreach (var item in metaTableInfoList)
			{
				if (item.IsAuditableField)
					continue;
				sb.AppendLine("public " + item.PropertyType + " " + item.Name + " { get; set; }");
			}
			var property_Looped_Template_Here = sb.ToString();
			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{Property_Looped_Template_Here}}", property_Looped_Template_Here)
											 ;
			Write(Path.Combine(Configuration.Application_Directory, className + "s\\Dtos"), className + "ListDto.cs", templateContent);
		}


		/// <summary>
		/// 生成CreateOrEditInput
		/// </summary>
		/// <param name="className"></param>
		/// <param name="metaTableInfoList"></param>
		[Verb]
		public static void SetCreateOrEditInputClass(string className, List<MetaTableInfo> metaTableInfoList)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Server\Dtos\CreateOrEditInputClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("public int? Id { get; set; }");
			sb.Append(GetPropertiesForDTO(metaTableInfoList, false, false));
			var property_Looped_Template_Here = sb.ToString();
			templateContent = templateContent.Replace("{{Namespace_Here}}", Configuration.Namespace_Here)
											 .Replace("{{Namespace_Relative_Full_Here}}", className)
											 .Replace("{{Entity_Name_Here}}", className)
											 .Replace("{{Property_Looped_Template_Here}}", property_Looped_Template_Here)
											 ;
			Write(Path.Combine(Configuration.Application_Directory, className + "s", "Dtos"), "CreateOrEdit" + className + "Input.cs", templateContent);
		}


		/// <summary>
		/// 生成ConstsClass
		/// </summary>
		/// <param name="className"></param>
		[Obsolete("See GeneretePageNameConsts Method")]
		[Verb]
		public static void SetConstsClass(string className)
		{

			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\Server\ConstsClass\MainTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);

			templateContent = templateContent.Replace("{{entity_Name_Here}}", GetFirstToLowerStr(className))
											 .Replace("{{Entity_Name_Here}}", className)
											 ;
			Write(Path.Combine(Configuration.Application_Directory, className + "s\\"), className + "ConstsClass.txt", templateContent);
		}

		/// <summary>
		/// 生成AppPermissions
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetAppPermissions(string className)
		{
			StringBuilder sbAppPermissions_Here = new StringBuilder();
			sbAppPermissions_Here.AppendLine($"#region {className}");
			sbAppPermissions_Here.AppendLine($"public const string Pages_{Configuration.App_Area_Name}_{className}			= \"{Configuration.App_Area_Name}.{className}\";");
			sbAppPermissions_Here.AppendLine($"public const string Pages_{Configuration.App_Area_Name}_{className}_Create	= \"{Configuration.App_Area_Name}.{className}.Create\";");
			sbAppPermissions_Here.AppendLine($"public const string Pages_{Configuration.App_Area_Name}_{className}_Edit		= \"{Configuration.App_Area_Name}.{className}.Edit\";");
			sbAppPermissions_Here.AppendLine($"public const string Pages_{Configuration.App_Area_Name}_{className}_Delete	= \"{Configuration.App_Area_Name}.{className}.Delete\";");
			sbAppPermissions_Here.AppendLine(" #endregion");
			sbAppPermissions_Here.AppendLine("                         ");
			sbAppPermissions_Here.AppendLine(" //{{AppPermissions_Here}}");

			var appPermissionsTemplateContent = Read(Configuration.AppPermissions_Path);
			if (!appPermissionsTemplateContent.Contains($"Pages_{Configuration.App_Area_Name}_{className}"))

			{
				appPermissionsTemplateContent = appPermissionsTemplateContent.Replace("//{{AppPermissions_Here}}", sbAppPermissions_Here.ToString());
				Write(Configuration.AppPermissions_Path, appPermissionsTemplateContent);
			}
		}

		/// <summary>
		/// 生成AppPermissions
		/// </summary>
		/// <param name="className"></param>
		[Verb]
		public static void SetAppAuthorizationProvider(string className)
		{
			StringBuilder sbAppAuthorizationProvider_Here = new StringBuilder();
			sbAppAuthorizationProvider_Here.AppendLine($"#region {className}");
			sbAppAuthorizationProvider_Here.AppendLine($" var {className} = administration.CreateChildPermission(PermissionNames.Pages_{Configuration.App_Area_Name}_{className}, L(\"{ className }\"));");

			sbAppAuthorizationProvider_Here.AppendLine($"{className}.CreateChildPermission(PermissionNames.Pages_{Configuration.App_Area_Name}_{ className }_Create, L(\"CreatingNew{className}\"));");
			sbAppAuthorizationProvider_Here.AppendLine($"{className}.CreateChildPermission(PermissionNames.Pages_{Configuration.App_Area_Name}_{className}_Edit, L(\"Editing{className}\"));");
			sbAppAuthorizationProvider_Here.AppendLine($"{className}.CreateChildPermission(PermissionNames.Pages_{Configuration.App_Area_Name}_{className}_Delete, L(\"Deleting{className}\"));");

			sbAppAuthorizationProvider_Here.AppendLine(" #endregion");
			sbAppAuthorizationProvider_Here.AppendLine("                         ");
			sbAppAuthorizationProvider_Here.AppendLine(" //{{AppAuthorizationProvider_Here}}");

			var appAuthorizationProviderTemplateContent = Read(Configuration.AppAuthorizationProvider_Path);
			if (!appAuthorizationProviderTemplateContent.Contains($"PermissionNames.Pages_{Configuration.App_Area_Name}_{className}"))

			{
				appAuthorizationProviderTemplateContent = appAuthorizationProviderTemplateContent.Replace("//{{AppAuthorizationProvider_Here}}", sbAppAuthorizationProvider_Here.ToString());
				Write(Configuration.AppAuthorizationProvider_Path, appAuthorizationProviderTemplateContent);
			}
		}

		[Verb]
		public static void setLocalizationKeys(string className, List<MetaTableInfo> metaTableInfoList)
		{
			string appServiceIntercafeClassDirectory = Configuration.RootDirectory + @"\LocalizationsTemplate.txt";
			var templateContent = Read(appServiceIntercafeClassDirectory);

			templateContent = templateContent.Replace("{{entity_Name_Here}}", GetFirstToLowerStr(className))
											 .Replace("{{Entity_Name_Here}}", className)
											 ;


			var LocalizationDictionaryTemplateContent = Read(Configuration.LocalizationDictionary_Path);
			var sbFields = new StringBuilder();
			foreach (var item in metaTableInfoList)
			{
				if (!LocalizationDictionaryTemplateContent.Contains($"<text name=\"{className}.{item.Name}\">"))
				{

					sbFields.Append($"<text name=\"{className}.{item.Name}\">{item.Name}</text>\n");
					sbFields.Append($"<text name=\"{className}.{item.Name}.placeholder\">{item.Name}</text>\n");
				}
			}
			sbFields.Insert(0, templateContent);
			LocalizationDictionaryTemplateContent = LocalizationDictionaryTemplateContent.Replace("<!--LocalizationDictionary_Here-->", sbFields.ToString());
			Write(Configuration.LocalizationDictionary_Path, LocalizationDictionaryTemplateContent);
		}

		/// <summary>
		/// 生成本地化语言 xml 文档
		/// </summary>
		/// <param name="className"></param>
		[Obsolete("See setLocalizationKeys")]
		[Verb]
		public static void SetZh_CN_LocalizationDictionary_Here(string className, string classAnnotation)
		{
			//zh_CN_LocalizationDictionary_Here

			StringBuilder sbzh_CN_LocalizationDictionary_Here = new StringBuilder();
			sbzh_CN_LocalizationDictionary_Here.AppendLine($"<!-- {classAnnotation}-->");
			sbzh_CN_LocalizationDictionary_Here.AppendLine($"<text name=\"{className }\">{classAnnotation}</text>");
			sbzh_CN_LocalizationDictionary_Here.AppendLine($"<text name=\"CreatingNew{ className}\">创建{classAnnotation}</text>");
			sbzh_CN_LocalizationDictionary_Here.AppendLine($"<text name=\"Editing{className}\">编辑{classAnnotation}</text>");
			sbzh_CN_LocalizationDictionary_Here.AppendLine($"<text name=\"Deleting{className}\">删除{classAnnotation}</text>");


			sbzh_CN_LocalizationDictionary_Here.AppendLine("<!--zh_CN_LocalizationDictionary_Here-->");

			var zh_CN_LocalizationDictionaryTemplateContent = Read(Configuration.LocalizationDictionary_Path);
			if (!zh_CN_LocalizationDictionaryTemplateContent.Contains($"<text name=\"{className }\">"))
			{
				zh_CN_LocalizationDictionaryTemplateContent = zh_CN_LocalizationDictionaryTemplateContent.Replace("<!--zh_CN_LocalizationDictionary_Here-->", sbzh_CN_LocalizationDictionary_Here.ToString());
				Write(Configuration.LocalizationDictionary_Path, zh_CN_LocalizationDictionaryTemplateContent);
			}
		}

		#endregion

		#endregion





		#region 文件读取
		public static string Read(string path)
		{
			using (StreamReader sr = new StreamReader(path, Encoding.Default))
			{
				return sr.ReadToEnd();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath">文件保存路径</param>
		/// <param name="fileName">文件名</param>
		/// <param name="templateContent">模板内容</param>
		[Verb]
		public static void Write(string filePath, string fileName, string templateContent)
		{
			Console.WriteLine($"Saving {filePath}...");
			Console.WriteLine($"\t Checking directory...");
			if (!Directory.Exists(filePath))
			{
				Console.WriteLine($"\t Creating directory...");
				Directory.CreateDirectory(filePath);
			}
			Console.WriteLine($"\t Writing file...");
			using (FileStream fs = new FileStream(Path.Combine(filePath, fileName), FileMode.Create))
			{
				//获得字节数组
				byte[] data = Encoding.Default.GetBytes(templateContent);
				//开始写入
				fs.Write(data, 0, data.Length);
			}
			Console.WriteLine($"\t Finished with sucess.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath">文件保存路径</param>
		/// <param name="templateContent">模板内容</param>
		[Verb]
		public static void Write(string filePath, string templateContent)
		{
			Console.WriteLine($"Saving {filePath}...");
			Console.WriteLine($"\t Checking directory...");
			if (!Directory.Exists(Path.GetDirectoryName(filePath)))
			{
				Console.WriteLine($"\t Creating directory...");
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

			}
			Console.WriteLine($"\t Writing file...");
			using (FileStream fs = new FileStream(filePath, FileMode.Create))
			{
				//获得字节数组
				byte[] data = Encoding.Default.GetBytes(templateContent);
				//开始写入
				fs.Write(data, 0, data.Length);
			}
			Console.WriteLine($"\t Finished with sucess.");

		}
		#endregion

		#region 首字母小写
		/// <summary>
		/// 首字母小写
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string GetFirstToLowerStr(string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				if (str.Length > 1)
				{
					return char.ToLower(str[0]) + str.Substring(1);
				}
				return char.ToLower(str[0]).ToString();
			}
			return null;
		}
		#endregion
	}
}
