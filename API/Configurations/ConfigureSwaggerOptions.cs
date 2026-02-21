using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace API.Configurations
{
    /// <summary>
    /// Configuração DINÂMICA do Swagger para suporte a múltiplas versões de API.
    /// 
    /// ?? OBJETIVO:
    /// - Detecta AUTOMATICAMENTE todas as versões da API (v1, v2, v3...) baseado nos controllers
    /// - Cria um documento Swagger separado para cada versão descoberta
    /// - Marca automaticamente versões deprecated na documentação
    /// - Adiciona metadados específicos para cada versão
    /// 
    /// ?? POR QUE É NECESSÁRIO:
    /// Sem esta classe, você teria que configurar MANUALMENTE cada versão no Program.cs:
    ///   c.SwaggerDoc("v1", new OpenApiInfo { Title = "...", Version = "v1" });
    ///   c.SwaggerDoc("v2", new OpenApiInfo { Title = "...", Version = "v2" });
    ///   c.SwaggerDoc("v3", new OpenApiInfo { Title = "...", Version = "v3" });
    ///   
    /// Com esta classe:
    /// - Ao criar OrdersV3Controller com [ApiVersion("3.0")], o Swagger detecta automaticamente
    /// - Não é necessário modificar Program.cs para adicionar novas versões
    /// - Mantém o código DRY (Don't Repeat Yourself)
    /// 
    /// ?? COMO FUNCIONA:
    /// 1. IApiVersionDescriptionProvider descobre todas as versões através dos atributos [ApiVersion] nos controllers
    /// 2. Para cada versão encontrada, cria um documento Swagger com metadados específicos
    /// 3. Verifica se a versão está marcada como Deprecated e adiciona aviso na descrição
    /// 4. Gera dropdown no Swagger UI: "FCG.Orders.API V1", "FCG.Orders.API V2", etc
    /// 
    /// ?? REGISTRADO EM:
    /// Program.cs ? builder.Services.ConfigureOptions&lt;ConfigureSwaggerOptions&gt;();
    /// 
    /// ?? SEPARAÇÃO DE RESPONSABILIDADES:
    /// - Program.cs: Configurações ESTÁTICAS (JWT, XML Comments, Security global)
    /// - ConfigureSwaggerOptions.cs: Configurações DINÂMICAS (versões da API)
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Configura o Swagger para cada versão da API descoberta automaticamente.
        /// Este método é chamado automaticamente pelo framework durante a inicialização.
        /// </summary>
        public void Configure(SwaggerGenOptions options)
        {
            // Descobre todas as versões da API através dos atributos [ApiVersion] nos controllers
            // Ex: [ApiVersion("1.0")], [ApiVersion("2.0", Deprecated = true)]
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                // Cria um documento Swagger separado para cada versão
                // GroupName = "v1", "v2", "v3", etc
                options.SwaggerDoc(
                    description.GroupName,
                    CreateInfoForApiVersion(description));
            }
        }

        /// <summary>
        /// Cria as informações de metadados para uma versão específica da API.
        /// </summary>
        /// <param name="description">Descrição da versão fornecida pelo IApiVersionDescriptionProvider</param>
        /// <returns>Objeto OpenApiInfo com título, versão, descrição e contato</returns>
        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = "AgroSolutions.Farms.API",
                Version = description.ApiVersion.ToString(),
                Description = "Farms, Fields and Crop Seasons Management",
                Contact = new OpenApiContact
                {
                    Name = "AgroSolutions Team",
                    Email = "support@agrosolutions.com"
                }
            };

            // Adiciona aviso visual para versões marcadas como deprecated
            // Exemplo: v1 com [ApiVersion("1.0", Deprecated = true)]
            if (description.IsDeprecated)
            {
                info.Description += " - ?? This API version has been deprecated.";
            }

            return info;
        }
    }
}
