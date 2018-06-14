# CodeGenerator

Gerador de código para o [AspNetBoilerPlate](http://aspnetboilerplate.com).

Esta versão é um Fork do [CodeGenerator de HisKingdom](https://github.com/HisKingdom/CodeGenerator)  e trabalha com a versão gratuita do AspNetBoilerPlate e para auxiliar na construção telas não modais para visualização e edição.

## Como utilizar 

O executável da aplicação orienta o processo da geração dos arquivos, entretanto, para que ele consiga gerar o código é necessário que sejam adicionadas algumas *tags* na solução para que sejam incluídos os códigos e ajustado o arquivo de configuração, informando os diretórios que possuem os *códigos base*.

## Passos

 1. Modificar o arquivo de configurações *appsettings.json*
	
	- *SourceSolution*: Diretório báse da aplicação;
	- *SourceAssembly*: DLL que contém as entidades (devem ser definidas utilizando o EntityFramework);
	- *RootDirectory*: Pasta que contém os templates a serem lidos
		> Em uma versão futura a chave será alterada para *TemplateDirectory*
	- *Namespace_Here*: Namespace utilizado no projeto
	- *Application_AppServiceBase*: Classe base e abstrata que disponibiliza o *Service* para a aplicação
	- *Application_Directory*: Diretório que contém a camada de aplicação
	- *AppPermissions_Path*: Caminho da classe que disponibiliza os nomes utilizados no modelo de permissão. Geralmente _"<Namespace>.Core\\Authorization\\PermissionNames.cs"_
	- *AppAuthorizationProvider_Path*: Caminho da classe que disponibiliza o SystemAuthorizationProvider.cs
	- *Zh_CN_LocalizationDictionary_Path*: Arquivo de idioma base
		> Em uma versão futura a chave será alterada para *LocalizationDictionary_Path*
	- *Web_Mvc_Directory*: Diretório da solução Web.MVC
	- *App_Area_Name*: Nome da área
	- *Controller_Base_Class*: Nome do controller base

 1. Adicionar a chave *//{{AppPermissions_Here}}* no arquivo *AppPermissions_Path* 
 1. Adicionar a chave *//{{Item_Menu_Template}* na classe que implementa *NavigationProvider*, disponível no projeto *Web/Startup*
 1. Adicionar a chave *//{{Template_Page_Name_Consts}}* no arquivo PageNames que fica dentro do projeto *Web/Startup*
 1. No arquivo AppAuthorizationProvider_Path incluir no *SetPermission*:
 
	
```csharp

	public override void SetPermissions(IPermissionDefinitionContext context)
	{
		//Código omitido...
		var administration = context.CreatePermission("Administration");
		//{{AppAuthorizationProvider_Here}}
	}
```

 1. No arquivo de localização adicionar a marca *<!--LocalizationDictionary_Here-->*
 1. Executar o aplicativo de gerar código com a opção de criar data table
 1. No arquivo de contexto adicione o código abaixo para que a validação seja realizada:

```csharp

	public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
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

- Configure os DbSets necessários
- Execute o código conforme as instruções da tela ou -h para exibir a ajuda. 
- Para cada Entity que foi gerada faça:
   > No Service criado, substitua *<<ChangeThisPropertyField>>* pelo campo de busca
   > Adicione o namespace que contém o Model (entity) (Usar a compilação para detectar as ocorrências)
   > Ajuste o *@using* do *_CreateOrEdit* gerado
   > Ajuste o Entity para criar as constantes de validação, principalmente em relação ao tamanho do campo
  
 
## Outros ajustes interessantes

- Modificar a *DefaultPassPhrase* em *Application.AppConsts.DefaultPassPhrase*
- Copiar o arquivo _ViewImports.cshtml e _ViewStart.cshtml para a pasta da área, caso não exista.