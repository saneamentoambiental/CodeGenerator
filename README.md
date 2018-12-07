---
title: ABP CodeGenerator
---

# Sobre o CodeGenerator

Gerador de código para o [AspNetBoilerPlate](http://aspnetboilerplate.com) v.3.9.0.

| Repositorio |  Status                                                     |
| :---------: | :---------------------------------------------------------: |
|   GitHub    | [![Build status](https://ci.appveyor.com/api/projects/status/tv4ap6797k4nmtfh?svg=true)](https://ci.appveyor.com/project/saneamentoambiental/codegenerator) |


Esta versão é um Fork do [CodeGenerator de HisKingdom](https://github.com/HisKingdom/CodeGenerator)  e trabalha com a versão gratuita do AspNetBoilerPlate para auxiliar na construção telas não modais para visualização e edição de dados.

-------

Nesta página você encontrará:

<!-- @import "[TOC]" {cmd="toc" depthFrom=2 depthTo=6 orderedList=false} -->

<!-- code_chunk_output -->

- [Sobre o CodeGenerator](#sobre-o-codegenerator)
	- [Como utilizar](#como-utilizar)
		- [Passo-a-passo](#passo-a-passo)
	- [Geração de código](#gera%C3%A7%C3%A3o-de-c%C3%B3digo)
	- [Outros ajustes interessantes](#outros-ajustes-interessantes)
	- [Exibição de grids](#exibi%C3%A7%C3%A3o-de-grids)
	- [FAQ](#faq)

<!-- /code_chunk_output -->

-------

## Como utilizar

O executável da aplicação orienta o processo da geração dos arquivos, entretanto, para que ele consiga gerar o código é necessário que sejam adicionadas algumas *tags* na solução para que sejam incluídos os códigos e ajustado o arquivo de configuração, informando os diretórios que possuem os *códigos base*.

Comando:

```
dotnet AbpCodeGenerator.dll
```

### Passo-a-passo

1. Criar o site/projeto ABP utilizando o _template_ disponível [aqui](https://aspnetboilerplate.com/Templates);

2. Modificar o arquivo de configurações *appsettings.json*

   * *SourceSolution*: Diretório báse da aplicação;
   * *SourceAssembly*: DLL que contém as entidades (devem ser definidas utilizando o EntityFramework);  
   * *Namespace_Here*: Namespace utilizado no projeto
   * *Application_AppServiceBase*: Classe base e abstrata que disponibiliza o *Service* para a aplicação, localizado em `<Namespace>.Application`
   * *Application_Directory*: Diretório que contém a camada de aplicação, geralmente o nome do diretório é `<Namespace>.Application`
   * *AppPermissions_Path*: Caminho da classe que disponibiliza os nomes utilizados no modelo de permissão. Geralmente `<Namespace>.Core\\Authorization\\PermissionNames.cs`
   * *AppAuthorizationProvider_Path*: Caminho da classe que disponibiliza o SystemAuthorizationProvider.cs, localizado em `<Namespace>.Core\\Authorization\\`
   * *Zh_CN_LocalizationDictionary_Path*: Arquivo de idioma base, geralmente localizado em `<Namespace>.Core\\Localization\\SourceFiles\\`
      > Em uma versão futura a chave será alterada para *LocalizationDictionary_Path*
   * *Web_Mvc_Directory*: Diretório da solução Web.MVC
   * *App_Area_Name*: Nome da área
   * *Controller_Base_Class*: Nome do controller base

3. Criar a classe base para a geração de `CRUD` chamada `<App_Area_Name>AsyncCrudAppServiceBase.cs`
	* Substituir o * <App_Area_Name> pelo nome da área
	* Colocar o código abaixo realizando os devidos ajustes de `namespace` e nomes de classes
	```cs
	using Abp.Application.Services;
	using Abp.Application.Services.Dto;
	using Abp.Domain.Entities;
	using Abp.Domain.Repositories;
	using System;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Identity;
	using Abp.IdentityFramework;
	using Abp.Runtime.Session;

	using GPSA.ETESystem.Authorization.Users;
	using GPSA.ETESystem.MultiTenancy;

	namespace GPSA.ETESystem
	{
		public partial interface IETESystemAsyncCrudAppService<TCreateOrEditInput, TGetAllInputDto> : IAsyncCrudAppService<TCreateOrEditInput, int, TGetAllInputDto>
			where TCreateOrEditInput : IEntityDto<int>
			where TGetAllInputDto : PagedAndSortedResultRequestDto
		{}

		public partial class ETESystemAsyncCrudAppServiceBase<TEntity, TCreateOrEditInput, TGetAllInput> : AsyncCrudAppService<TEntity, TCreateOrEditInput, int, TGetAllInput>
			, IETESystemAsyncCrudAppService<TCreateOrEditInput, TGetAllInput>
			where TEntity : class, IEntity<int>
			where TCreateOrEditInput : IEntityDto<int>
			where TGetAllInput : PagedAndSortedResultRequestDto
		{
			public ETESystemAsyncCrudAppServiceBase(IRepository<TEntity, int> repository, string CreatePermission, string EditPermission,
				string DeletePermission, string ViewPermission
				) : this(repository)
			{
				this.CreatePermissionName = CreatePermissionName;
				this.DeletePermissionName = DeletePermissionName;
				this.UpdatePermissionName = UpdatePermissionName;
				this.GetPermissionName = ViewPermission;
				this.GetAllPermissionName = ViewPermission;
				this.LocalizationSourceName = ETESystemConsts.LocalizationSourceName;
			}

			public ETESystemAsyncCrudAppServiceBase(IRepository<TEntity, int> repository) : base(repository){}

			public async Task CreateOrEdit(TCreateOrEditInput input)
			{
				if (input.Id == 0){ await Create(input); }
				else{await Update(input);}
			}
			public TenantManager TenantManager { get; set; }
			public UserManager UserManager { get; set; }
			protected virtual Task<User> GetCurrentUserAsync()
			{
				var user = UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
				if (user == null)
				{
					throw new Exception("There is no current user!");
				}
				return user;
			}
			protected virtual Task<Tenant> GetCurrentTenantAsync()
			{
				return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
			}
			protected virtual void CheckErrors(IdentityResult identityResult)
			{
				identityResult.CheckErrors(LocalizationManager);
			}
		}
	}
	```

2. Adicionar a chave *`//{{AppPermissions_Here}}`* no arquivo `<Namespace>.Core\Authorization\PermissionNames.cs`

3. Adicionar a chave *`//{{Item_Menu_Template}}`* na classe que implementa *NavigationProvider*, disponível em `<Namespace>.Web\Startup\<Namespace>NavigationProvider.cs`, dentro da função `SetNavigation()`, como no exemplo abaixo:

	```csharp
	public override void SetNavigation(INavigationProviderContext context)
    {
    	context.Manager.MainMenu
		//{{Item_Menu_Template}}
		//...
	}	
	```
1. Adicionar as chaves `//{{JS_index}}` e `//{{JS_CreateOrEdit}}` no final do arquivo `bundle.config` localizado na pasta raíz do projeto Web, como exibido abaixo.
	
	```
	[
	...,
	{
		"outputFileName": "wwwroot/view-resources/Views/Shared/Components/TenantChange/Default.min.js",
		"inputFiles": [
		"wwwroot/view-resources/Views/Shared/Components/TenantChange/Default.js"
		]
	},  
	//{{JS_Index}}
	//{{JS_CreateOrEdit}}
	]
	```

1. Adicionar a chave `//{{Template_Page_Name_Consts}}` no arquivo localizado em `<Namespace>.Web\Startup\PageNames.cs`
2. No arquivo localizado em `<Namespace>.Core\Authorization\<Namespace>AuthorizationProvider.cs` incluir na função *SetPermission()*:

   ```csharp
	public override void SetPermissions(IPermissionDefinitionContext context)
	{
		//Código omitido...
		var administration = context.CreatePermission("Administration");
		//{{AppAuthorizationProvider_Here}}
	}
   ```

1. No arquivo de localização adicionar a marca `<!--LocalizationDictionary_Here-->`, localizado em `<Namespace>.Core\Localization\<Namespace>-pt-BR.xml`. Caso este arquivo não exista, copiar um dos xmls existentes e modificar o seu nome
 1. Executar o aplicativo de gerar código com a opção de criar data table
 2. No arquivo de contexto adicione o código abaixo para que a validação seja realizada:

    ```csharp

	public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
			CancellationToken cancellationToken = default(CancellationToken))
	{
		ValidateEntries();
		return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
	}

	public override int SaveChanges(bool acceptAllChangesOnSuccess)
	{
		ValidateEntries();
		return base.SaveChanges(acceptAllChangesOnSuccess);
	}

	public void ValidateEntries()
	{
		var serviceProvider = IocManager.Instance.Resolve<IServiceProvider>();
		var items = new Dictionary<object, object>();

		foreach (var entry in this.ChangeTracker.Entries().Where(
					e => (e.State == EntityState.Added) || (e.State == EntityState.Modified)))
		{

			var entity = entry.Entity;
			if (entity is ISoftDelete && ((ISoftDelete)entity).IsNullOrDeleted())
			{
				var oldValueDeleted = entry.OriginalValues["IsDeleted"];
				if (oldValueDeleted.Equals(false))
					continue;

			}
			var context = new ValidationContext(entity, serviceProvider, items);
			var results = new List<ValidationResult>();
			if (Validator.TryValidateObject(entity, context, results, true) == false)
			{
				foreach (var result in results)
				{
					if (result != ValidationResult.Success)
					{
						throw new AbpValidationException(result.ErrorMessage);
					}
				}
			}
		}
	}
    ```

## Geração de código

* Configure os `DbSets` necessários
* Execute o código conforme as instruções da tela ou `-h` para exibir a ajuda.
  * Geralmente é executado `NomeEntidade -all` ou `NomeEntidade -h`
* Para cada Entity que foi gerada faça:
  
  * No Service criado, substitua `<<ChangeThisPropertyField>>` pelo campo de busca
  * Adicione o namespace que contém o Model (entity) (Usar a compilação para detectar as ocorrências)
  * Ajuste o `@using` do `_CreateOrEdit` gerado
  * Ajuste o `Entity` para criar as constantes de validação, principalmente em relação ao tamanho do campo.
    * Sugestão: `public const int <Campo>MaxLength = 50;`
  

## Outros ajustes interessantes

* Modificar a `DefaultPassPhrase` em `Application.AppConsts.DefaultPassPhrase`
* Copiar o arquivo `_ViewImports.cshtml` e `_ViewStart.cshtml` para a pasta da área, caso não exista.

## Exibição de grids
  
As telas que necessitam de `grid` deverão indicar na `ViewBag`, através da propriedade `HasGrid`, que há necessidade dos arquivos do [Jquery DataTable](https://www.datatables.net). Assim, nas telas é indicado que a  ```ViewBag.HasGrid = true``` seja declarada próxima ao título, como abaixo:

```cs
@using GPSA.ETESystem.Authorization
@using GPSA.ETESystem.Web.Startup

@model GPSA.ETESystem.Web.Areas.ETESystem.Models.CaracteristicaMonitoramentos.CaracteristicaMonitoramentoListViewModel
@{
    ViewBag.ActiveMenu = PageNames.CaracteristicaMonitoramento.Index;
    ViewBag.Title = L("CaracteristicaMonitoramento");
    ViewBag.CurrentPageName = PageNames.CaracteristicaMonitoramento.Index; // The menu item will be active for this page.
    ViewBag.HasGrid = true;
}
@section scripts{
	....
}
```

O arquivo de layout verifica, então, a necessidade dos `scripts` e faz a inclusão dos elementos necessários.

```cs
@if (ViewBag.HasGrid != null && ViewBag.HasGrid == true)
{
	<link rel="stylesheet" type="text/css" href="https://cdn.datatables.net/1.10.16/css/jquery.dataTables.min.css" />
	<script type="text/javascript" src="https://cdn.datatables.net/1.10.16/js/jquery.dataTables.min.js"></script>
	<script src="~/js/script.js" asp-append-version="true"></script>
}
```

O conteúdo do arquivo `script.js` deve ter o seguinte comando:

```JS
(function ($) {
    if ($.fn.dataTable != null && $.fn.dataTable.defaults != null) {
        $.extend($.fn.dataTable.defaults, {
            paging: true,
            responsive: true,
            serverSide: true,
            processing: true,
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.10.19/i18n/Portuguese-Brasil.json'
            }
        }); 
    }
})(jQuery);

```

## FAQ

Caso sua dúvida ainda persista, consulte nosso [FAQ](faq.md).