using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Configurations;

/// <summary>
/// Swagger Document Filter para ordenar controladores conforme regras de negócio
/// </summary>
public class SwaggerOrderDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Definir ordem dos controladores (Tags)
        var tagOrder = new Dictionary<string, int>
        {
            { "Health", 0 }, 
            { "Farms", 1 },
            { "Fields", 2 },
            { "CropSeasons", 3 }
        };

        // Ordenar Tags (Controladores)
        if (swaggerDoc.Tags != null)
        {
            swaggerDoc.Tags = swaggerDoc.Tags
                .OrderBy(tag => tagOrder.ContainsKey(tag.Name) ? tagOrder[tag.Name] : 50)
                .ThenBy(tag => tag.Name)
                .ToList();
        }

        // Ordenar Paths de acordo com a ordem dos controladores
        var orderedPaths = swaggerDoc.Paths
            .OrderBy(path =>
            {
                // Pega a primeira operação do path para identificar a tag
                var firstOperation = path.Value.Operations.FirstOrDefault();
                var tag = firstOperation.Value?.Tags.FirstOrDefault()?.Name ?? "Other";
                return tagOrder.ContainsKey(tag) ? tagOrder[tag] : 50;
            })
            .ThenBy(path => path.Key) // Depois ordem alfabética do path
            .ToDictionary(x => x.Key, x => x.Value);

        swaggerDoc.Paths.Clear();
        foreach (var path in orderedPaths)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
}
