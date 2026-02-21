using Application.DTO.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Helpers
{
    /// <summary>
    /// Helper para gerar links HATEOAS consistentes.
    /// 
    /// ?? OBJETIVO:
    /// Centralizar a lógica de criação de links HATEOAS,
    /// garantindo URLs corretas e consistentes.
    /// 
    /// ?? USO:
    /// var links = HateoasHelper.CreateUserLinks(urlHelper, userId, version);
    /// userResponse.Links = links;
    /// </summary>
    public static class HateoasHelper
    {
        /// <summary>
        /// Cria links HATEOAS para um usuário específico.
        /// </summary>
        /// <param name="urlHelper">Helper para gerar URLs</param>
        /// <param name="userId">ID do usuário</param>
        /// <param name="version">Versão da API (ex: "1.0")</param>
        /// <param name="isActive">Status atual do usuário (para links condicionais)</param>
        /// <returns>Lista de links HATEOAS</returns>
        public static List<Link> CreateUserLinks(IUrlHelper urlHelper, string userId, string version, bool? isActive = null)
        {
            var links = new List<Link>
            {
                // Self - Link para o próprio recurso
                new Link(
                    href: urlHelper.Link("GetUserById", new { userId = userId, version }) ?? string.Empty,
                    rel: "self",
                    method: "GET"
                ),

                // Update - Link para atualizar o usuário
                new Link(
                    href: urlHelper.Link("UpdateUser", new { userId = userId, version }) ?? string.Empty,
                    rel: "update",
                    method: "PUT"
                ),

                // Delete - Link para deletar
                new Link(
                    href: urlHelper.Link("DeleteUser", new { userId = userId, version }) ?? string.Empty,
                    rel: "delete",
                    method: "DELETE"
                ),

                // All - Link para lista de todos os usuários
                new Link(
                    href: urlHelper.Link("GetAllUsers", new { version }) ?? string.Empty,
                    rel: "all",
                    method: "GET"
                )
            };

            // Links condicionais baseados no status
            if (isActive == false)
            {
                links.Add(new Link(
                    href: urlHelper.Link("UpdateUser", new { userId = userId, version }) ?? string.Empty,
                    rel: "activate",
                    method: "PUT"
                ));
            }

            if (isActive == true)
            {
                links.Add(new Link(
                    href: urlHelper.Link("UpdateUser", new { userId = userId, version }) ?? string.Empty,
                    rel: "deactivate",
                    method: "PUT"
                ));
            }

            return links;
        }

        /// <summary>
        /// Gera links HATEOAS para Health Check endpoints.
        /// </summary>
        /// <param name="httpContext">HTTP Context para gerar URLs</param>
        /// <param name="version">Versão da API</param>
        /// <returns>Lista de links HATEOAS para health endpoints</returns>
        public static List<Link> GenerateHealthLinks(HttpContext httpContext, string version)
        {
            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

            var links = new List<Link>
            {
                // Self - Health check completo
                new Link(
                    href: $"{baseUrl}/v{version}/health",
                    rel: "self",
                    method: "GET"
                ),

                // Users - Link para endpoints principais
                new Link(
                    href: $"{baseUrl}/api/v{version}/users",
                    rel: "users",
                    method: "GET"
                ),

                // Swagger Documentation
                new Link(
                    href: $"{baseUrl}/swagger/index.html",
                    rel: "documentation",
                    method: "GET"
                )
            };

            return links;
        }

        #region Farm HATEOAS

        public static void AddLinksToFarm(Application.DTO.Farm.FarmResponse farm, IUrlHelper urlHelper, string version)
        {
            farm.Links = new List<Link>
            {
                new Link(
                    href: urlHelper.Link("GetFarmById", new { farmId = farm.FarmId, version }) ?? string.Empty,
                    rel: "self",
                    method: "GET"
                ),
                new Link(
                    href: urlHelper.Link("UpdateFarm", new { farmId = farm.FarmId, version }) ?? string.Empty,
                    rel: "update",
                    method: "PUT"
                ),
                new Link(
                    href: urlHelper.Link("DeleteFarm", new { farmId = farm.FarmId, version }) ?? string.Empty,
                    rel: "delete",
                    method: "DELETE"
                ),
                new Link(
                    href: urlHelper.Link("GetFieldsByFarm", new { farmId = farm.FarmId, version }) ?? string.Empty,
                    rel: "fields",
                    method: "GET"
                ),
                new Link(
                    href: urlHelper.Link("GetAllFarms", new { version }) ?? string.Empty,
                    rel: "all-farms",
                    method: "GET"
                )
            };
        }

        public static void AddLinksToFarms(IEnumerable<Application.DTO.Farm.FarmResponse> farms, IUrlHelper urlHelper, string version)
        {
            foreach (var farm in farms)
            {
                AddLinksToFarm(farm, urlHelper, version);
            }
        }

        #endregion

        #region Field HATEOAS

        public static void AddLinksToField(Application.DTO.Field.FieldResponse field, IUrlHelper urlHelper, string version)
        {
            field.Links = new List<Link>
            {
                new Link(
                    href: urlHelper.Link("GetFieldById", new { fieldId = field.Id, version }) ?? string.Empty,
                    rel: "self",
                    method: "GET"
                ),
                new Link(
                    href: urlHelper.Link("UpdateField", new { fieldId = field.Id, version }) ?? string.Empty,
                    rel: "update",
                    method: "PUT"
                ),
                new Link(
                    href: urlHelper.Link("DeleteField", new { fieldId = field.Id, version }) ?? string.Empty,
                    rel: "delete",
                    method: "DELETE"
                ),
                new Link(
                    href: urlHelper.Link("GetFarmById", new { farmId = field.FarmId, version }) ?? string.Empty,
                    rel: "farm",
                    method: "GET"
                ),
                new Link(
                    href: urlHelper.Link("GetCropSeasonsByField", new { fieldId = field.Id, version }) ?? string.Empty,
                    rel: "crop-seasons",
                    method: "GET"
                ),
                new Link(
                    href: urlHelper.Link("GetAllFields", new { version }) ?? string.Empty,
                    rel: "all-fields",
                    method: "GET"
                )
            };
        }

        public static void AddLinksToFields(IEnumerable<Application.DTO.Field.FieldResponse> fields, IUrlHelper urlHelper, string version)
        {
            foreach (var field in fields)
            {
                AddLinksToField(field, urlHelper, version);
            }
        }

        #endregion

        #region CropSeason HATEOAS

        public static void AddLinksToCropSeason(Application.DTO.CropSeason.CropSeasonResponse cropSeason, IUrlHelper urlHelper, string version)
        {
            var links = new List<Link>
            {
                new Link(
                    href: urlHelper.Link("GetCropSeasonById", new { cropSeasonId = cropSeason.Id, version }) ?? string.Empty,
                    rel: "self",
                    method: "GET"
                ),
                new Link(
                    href: urlHelper.Link("GetFieldById", new { fieldId = cropSeason.FieldId, version }) ?? string.Empty,
                    rel: "field",
                    method: "GET"
                ),
                new Link(
                    href: urlHelper.Link("GetAllCropSeasons", new { version }) ?? string.Empty,
                    rel: "all-crop-seasons",
                    method: "GET"
                )
            };

            // Links condicionais baseados no status
            if (cropSeason.Status == "Planned")
            {
                links.Add(new Link(
                    href: urlHelper.Link("UpdateCropSeason", new { cropSeasonId = cropSeason.Id, version }) ?? string.Empty,
                    rel: "update",
                    method: "PUT"
                ));
                links.Add(new Link(
                    href: urlHelper.Link("StartPlanting", new { cropSeasonId = cropSeason.Id, version }) ?? string.Empty,
                    rel: "start-planting",
                    method: "POST"
                ));
                links.Add(new Link(
                    href: urlHelper.Link("DeleteCropSeason", new { cropSeasonId = cropSeason.Id, version }) ?? string.Empty,
                    rel: "delete",
                    method: "DELETE"
                ));
            }

            if (cropSeason.Status == "Active")
            {
                links.Add(new Link(
                    href: urlHelper.Link("FinishHarvest", new { cropSeasonId = cropSeason.Id, version }) ?? string.Empty,
                    rel: "finish-harvest",
                    method: "POST"
                ));
                links.Add(new Link(
                    href: urlHelper.Link("CancelCropSeason", new { cropSeasonId = cropSeason.Id, version }) ?? string.Empty,
                    rel: "cancel",
                    method: "POST"
                ));
            }

            if (cropSeason.Status == "Finished")
            {
                links.Add(new Link(
                    href: urlHelper.Link("DeleteCropSeason", new { cropSeasonId = cropSeason.Id, version }) ?? string.Empty,
                    rel: "delete",
                    method: "DELETE"
                ));
            }

            cropSeason.Links = links;
        }

        public static void AddLinksToCropSeasons(IEnumerable<Application.DTO.CropSeason.CropSeasonResponse> cropSeasons, IUrlHelper urlHelper, string version)
        {
            foreach (var cropSeason in cropSeasons)
            {
                AddLinksToCropSeason(cropSeason, urlHelper, version);
            }
        }

        #endregion
    }
}


