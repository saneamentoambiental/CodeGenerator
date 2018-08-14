# Sobre o CodeGenerator

Gerador de código para o [AspNetBoilerPlate](http://aspnetboilerplate.com) v.3.7.2.

Esta versão é um Fork do [CodeGenerator de HisKingdom](https://github.com/HisKingdom/CodeGenerator)  e trabalha com a versão gratuita do AspNetBoilerPlate (v.3.7.2) para auxiliar na construção telas não modais para visualização e edição de dados.

-------

Nesta página você encontrará:

<!-- @import "[TOC]" {cmd="toc" depthFrom=2 depthTo=6 orderedList=false} -->

<!-- code_chunk_output -->

* [Como utilizar](#como-utilizar)
	* [Passo-a-passo](#passo-a-passo)
* [Geração de código](#geração-de-código)
* [Outros ajustes interessantes](#outros-ajustes-interessantes)
* [FAQ](#faq)

<!-- /code_chunk_output -->

-------

## Como utilizar

O executável da aplicação orienta o processo da geração dos arquivos, entretanto, para que ele consiga gerar o código é necessário que sejam adicionadas algumas *tags* na solução para que sejam incluídos os códigos e ajustado o arquivo de configuração, informando os diretórios que possuem os *códigos base*.

### Passo-a-passo

1. Criar o site/projeto ABP utilizando o _template_ disponível [aqui](https://aspnetboilerplate.com/Templates);

2. Modificar o arquivo de configurações *appsettings.json*

   * *SourceSolution*: Diretório báse da aplicação;
   * *SourceAssembly*: DLL que contém as entidades (devem ser definidas utilizando o EntityFramework);
   * *RootDirectory*: Pasta que contém os templates a serem lidos
      > Em uma versão futura a chave será alterada para *TemplateDirectory*
   * *Namespace_Here*: Namespace utilizado no projeto
   * *Application_AppServiceBase*: Classe base e abstrata que disponibiliza o *Service* para a aplicação, localizado em ``<Namespace>.Application``
   * *Application_Directory*: Diretório que contém a camada de aplicação, geralmente o nome do diretório é ``<Namespace>.Application``
   * *AppPermissions_Path*: Caminho da classe que disponibiliza os nomes utilizados no modelo de permissão. Geralmente _``<Namespace>.Core\\Authorization\\PermissionNames.cs``_
   * *AppAuthorizationProvider_Path*: Caminho da classe que disponibiliza o SystemAuthorizationProvider.cs, localizado em ``<Namespace>.Core\\Authorization\\``
   * *Zh_CN_LocalizationDictionary_Path*: Arquivo de idioma base, geralmente localizado em ``<Namespace>.Core\\Localization\\SourceFiles\\``
      > Em uma versão futura a chave será alterada para *LocalizationDictionary_Path*
   * *Web_Mvc_Directory*: Diretório da solução Web.MVC
   * *App_Area_Name*: Nome da área
   * *Controller_Base_Class*: Nome do controller base

3. Adicionar a chave *//{{AppPermissions_Here}}* no arquivo _``<Namespace>.Core\Authorization\PermissionNames.cs``_

4. Adicionar a chave *//{{Item_Menu_Template}* na classe que implementa *NavigationProvider*, disponível em _``<Namespace>.Web\Startup\<Namespace>NavigationProvider.cs``_, dentro da função _``SetNavigation()``_, como no exemplo abaixo:

	```csharp
		public override void SetNavigation(INavigationProviderContext context)
        {
            context.Manager.MainMenu
			//{{Item_Menu_Template}
			...
		}	
	```
	
5. Adicionar a chave *//{{Template_Page_Name_Consts}}* no arquivo localizado em _``<Namespace>.Web\Startup\PageNames.cs``_
6. No arquivo localizado em _``<Namespace>.Core\Authorization\<Namespace>AuthorizationProvider.cs``_ incluir na função *SetPermission()*:

   ```csharp
	public override void SetPermissions(IPermissionDefinitionContext context)
	{
		//Código omitido...
		var administration = context.CreatePermission("Administration");
		//{{AppAuthorizationProvider_Here}}
	}
   ```

1. No arquivo de localização adicionar a marca _``<!--LocalizationDictionary_Here-->``_, localizado em _``<Namespace>.Core\Localization\<Namespace>-pt-BR.xml``_. Caso este arquivo não exista, copiar um dos xmls existentes e modificar o seu nome
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

* Configure os ``DbSets`` necessários
* Execute o código conforme as instruções da tela ou ``-h`` para exibir a ajuda.
  * Geralmente é executado ``NomeEntidade -all`` ou ``NomeEntidade -h``
* Para cada Entity que foi gerada faça:
  
  * No Service criado, substitua ``<<ChangeThisPropertyField>>`` pelo campo de busca
  * Adicione o namespace que contém o Model (entity) (Usar a compilação para detectar as ocorrências)
  * Ajuste o ``@using`` do ``_CreateOrEdit`` gerado
  * Ajuste o ``Entity`` para criar as constantes de validação, principalmente em relação ao tamanho do campo.
    * Sugestão: ``public const int <Campo>MaxLength = 50;``
  

## Outros ajustes interessantes

* Modificar a ``DefaultPassPhrase`` em ``Application.AppConsts.DefaultPassPhrase``
* Copiar o arquivo ``_ViewImports.cshtml`` e ``_ViewStart.cshtml`` para a pasta da área, caso não exista.

## FAQ

Caso sua dúvida ainda persista, consulte nosso [FAQ](faq.md).