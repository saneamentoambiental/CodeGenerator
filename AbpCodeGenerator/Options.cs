using AbpCodeGenerator.Lib;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
namespace AbpCodeGenerator
{
    /// <summary>
    /// <see cref="https://www.codeproject.com/Articles/19869/Powerful-and-simple-command-line-parsing-in-C"/>
    /// <see cref="https://github.com/adamabdelhamed/PowerArgs"/>
    /// <see cref="http://adrianaisemberg.github.io/CLAP/#what"/>
    /// <see cref="https://github.com/commandlineparser/commandline"/>
    /// <see cref="https://codereview.stackexchange.com/questions/142627/optimizing-simple-menu-in-console-app?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa"></see>
    /// </summary>
    public class Options
    {
        public string GetUsage()
        {
            return "Please read user manual!" + Environment.NewLine;
        }

        [Usage(ApplicationAlias = "AbpCodeGenerator")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Default scenario", new Options { entityName = "EntityName", AllOptions = true });

                yield return new Example("Generate Services to Entity 'EntityName'", new Options { entityName = "EntityName", CreateServiceClass = true });
                                
                yield return new Example("Add required classes to the grid (Used once)", new Options { entityName = "EntityName", CreateDataTable = true });
                yield return new Example("See all options <EntityName> <Options>","");

            }
        }

        [Option('a', HelpText = "Create all objects")]
        public bool AllOptions { get; set; }

        [Option('t', HelpText = "Create DataTable Extensions")]
        public bool CreateDataTable { get; set; }

        [Option('m', HelpText = "Add Menu Navigation")]
        public bool CreateNavigationMenu { get; set; }
        [Option('s', HelpText = "Create Service")]
        public bool CreateServiceClass { get; set; }
        [Option('d', HelpText = "Create DTO")]
        public bool CreateDtos { get; set; }
        [Option('c', HelpText = "Create Consts used into permissions and menu")]
        public bool CreatePageConsts { get; set; }
        [Option('p', HelpText = "Set permissions")]
        public bool SetPermissions { get; set; }
        [Option('l', HelpText = "Add localizations keys")]
        public bool CreateLoacalization { get; set; }
        [Option('w', HelpText = "Create Client Controller And Views")]
        public bool CreateClientControllerAndViews { get; set; }
        [Option('k', HelpText = "Create Lookup")]
        public bool CreateLookup { get; set; }

        private string entityName { get; set; }

        [Value(0, MetaName = "EntityName", HelpText = "Specifies the Entity to generate", Required = true)]
        public string EntityName
        {
            get => entityName; set
            {
                this.entityName = value;
                MetaTableInfoList = MetaTableInfo.GetMetaTableInfoListForAssembly(entityName);
                if (MetaTableInfoList.Count == 0)
                {
                    throw new InvalidOperationException($"The entity {value} not exists or properties for {value} not found!");
                }
            }
        }

        public void ExecuteCommand()
        {
            EntityName = EntityName.Trim();
            Console.WriteLine("You entered string '{0}'", EntityName);
            //Obter o tipo de chave primária
            var propertyType = MetaTableInfoList.FirstOrDefault(m => m.Name == "Id").PropertyType;

            if (CreateDataTable)
            {
                CodeGeneratorHelper.CreateDataTableWrapper();
            }
            if (AllOptions || CreateLookup)
            {
                CodeGeneratorHelper.AddLoockupMethodIntoService(EntityName);
            }

            if (AllOptions || CreateNavigationMenu)
            {
                CodeGeneratorHelper.AddNavigationMenu(EntityName);
            }

            if (AllOptions || CreateServiceClass)
            {
                // Geração do lado do servidor  
                CodeGeneratorHelper.SetAppServiceIntercafeClass(EntityName, propertyType);
                CodeGeneratorHelper.SetAppServiceClass(EntityName, propertyType);
            }
            if (AllOptions || CreateDtos)
            {
                CodeGeneratorHelper.SetListDtoClass(EntityName, MetaTableInfoList);
                CodeGeneratorHelper.SetCreateOrEditInputClass(EntityName, MetaTableInfoList);
                CodeGeneratorHelper.SetGetForEditOutputClass(EntityName);
                CodeGeneratorHelper.SetGetInputClass(EntityName, MetaTableInfoList);
            }
            if (AllOptions || CreatePageConsts)
            {
                CodeGeneratorHelper.GeneretePageNameConsts(EntityName);
            }
            if (AllOptions || SetPermissions)
            {
                CodeGeneratorHelper.SetAppPermissions(EntityName);
                CodeGeneratorHelper.SetAppAuthorizationProvider(EntityName);
            }
            if (AllOptions || CreateLoacalization)
            {
                CodeGeneratorHelper.setLocalizationKeys(EntityName, MetaTableInfoList);
            }

            if (AllOptions || CreateClientControllerAndViews)
            {
                CodeGeneratorHelper.SetCreateOrEditHtmlTemplate(EntityName, MetaTableInfoList);
                CodeGeneratorHelper.SetControllerClass(EntityName, propertyType);
                CodeGeneratorHelper.SetCreateOrEditJs(EntityName);
                CodeGeneratorHelper.SetCreateOrEditViewModelClass(EntityName);
                CodeGeneratorHelper.SetListViewModelClass(EntityName);
                CodeGeneratorHelper.SetIndexHtmlTemplate(EntityName, MetaTableInfoList);
                CodeGeneratorHelper.SetIndexJsTemplate(EntityName, MetaTableInfoList);
            }
        }
        protected List<MetaTableInfo> MetaTableInfoList
        {
            get; set;

        }
    }
}
