using API.Helpers;
using API.Models;
using Application.DTO.CropSeason;
using Application.Interfaces;
using Asp.Versioning;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1
{
    /// <summary>
    /// Crop Seasons Controller V1
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [Route("v{version:apiVersion}/crop-seasons")]
    public class CropSeasonsController : ControllerBase
    {
        private readonly ICropSeasonService _cropSeasonService;

        public CropSeasonsController(ICropSeasonService cropSeasonService)
        {
            _cropSeasonService = cropSeasonService;
        }

        #region GETS

        /// <summary>
        /// Returns all crop seasons registered.
        /// </summary>
        /// <returns>List of Crop Seasons</returns>
        [HttpGet(Name = "GetAllCropSeasons")]
        [ProducesResponseType(typeof(IEnumerable<CropSeasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var cropSeasons = await _cropSeasonService.GetAllCropSeasonsAsync();
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeasons(cropSeasons, Url, version);
            return Ok(cropSeasons);
        }

        /// <summary>
        /// Returns a crop season by id.
        /// </summary>
        /// <param name="cropSeasonId">Crop Season ID</param>
        /// <returns>Object Crop Season</returns>
        [HttpGet("{cropSeasonId}", Name = "GetCropSeasonById")]
        [ProducesResponseType(typeof(CropSeasonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string cropSeasonId)
        {
            var cropSeason = await _cropSeasonService.GetCropSeasonByIdAsync(cropSeasonId);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeason(cropSeason, Url, version);
            return Ok(cropSeason);
        }

        /// <summary>
        /// Returns all crop seasons from a specific field.
        /// </summary>
        /// <param name="fieldId">Field ID</param>
        /// <returns>List of Crop Seasons</returns>
        [HttpGet("field/{fieldId}", Name = "GetCropSeasonsByField")]
        [ProducesResponseType(typeof(IEnumerable<CropSeasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByFieldId(string fieldId)
        {
            var cropSeasons = await _cropSeasonService.GetCropSeasonsByFieldIdAsync(fieldId);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeasons(cropSeasons, Url, version);
            return Ok(cropSeasons);
        }

        /// <summary>
        /// Returns all crop seasons by status.
        /// </summary>
        /// <param name="status">Status (Planned, Active, Finished)</param>
        /// <returns>List of Crop Seasons</returns>
        [HttpGet("status/{status}", Name = "GetCropSeasonsByStatus")]
        [ProducesResponseType(typeof(IEnumerable<CropSeasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByStatus(string status)
        {
            if (!Enum.TryParse<CropSeasonStatus>(status, true, out var statusEnum))
                return BadRequest(new ErrorResponse 
                { 
                    Message = $"Invalid status. Valid values are: {string.Join(", ", Enum.GetNames<CropSeasonStatus>())}" 
                });

            var cropSeasons = await _cropSeasonService.GetCropSeasonsByStatusAsync(statusEnum);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeasons(cropSeasons, Url, version);
            return Ok(cropSeasons);
        }

        /// <summary>
        /// Returns all overdue crop seasons (expected harvest date passed).
        /// </summary>
        /// <returns>List of Overdue Crop Seasons</returns>
        [HttpGet("overdue", Name = "GetOverdueCropSeasons")]
        [ProducesResponseType(typeof(IEnumerable<CropSeasonResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOverdue()
        {
            var cropSeasons = await _cropSeasonService.GetOverdueCropSeasonsAsync();
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeasons(cropSeasons, Url, version);
            return Ok(cropSeasons);
        }

        #endregion

        #region POST

        /// <summary>
        /// Add a crop season.
        /// </summary>
        /// <param name="request">Crop Season data</param>
        /// <returns>Object crop season added</returns>
        [HttpPost(Name = "AddCropSeason")]
        [ProducesResponseType(typeof(CropSeasonResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add([FromBody] AddCropSeasonRequest request)
        {
            var addedCropSeason = await _cropSeasonService.AddCropSeasonAsync(request);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeason(addedCropSeason, Url, version);

            return CreatedAtRoute("GetCropSeasonById", 
                new { cropSeasonId = addedCropSeason.Id, version }, 
                addedCropSeason);
        }

        /// <summary>
        /// Start planting for a crop season (changes status from Planned to Active).
        /// </summary>
        /// <param name="cropSeasonId">Crop Season ID</param>
        /// <returns>Updated crop season</returns>
        [HttpPost("{cropSeasonId}/start-planting", Name = "StartPlanting")]
        [ProducesResponseType(typeof(CropSeasonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartPlanting(string cropSeasonId)
        {
            var updatedCropSeason = await _cropSeasonService.StartPlantingAsync(cropSeasonId);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeason(updatedCropSeason, Url, version);
            return Ok(updatedCropSeason);
        }

        /// <summary>
        /// Finish harvest for a crop season (changes status from Active to Finished).
        /// </summary>
        /// <param name="cropSeasonId">Crop Season ID</param>
        /// <param name="request">Harvest date</param>
        /// <returns>Updated crop season</returns>
        [HttpPost("{cropSeasonId}/finish-harvest", Name = "FinishHarvest")]
        [ProducesResponseType(typeof(CropSeasonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FinishHarvest(string cropSeasonId, [FromBody] FinishHarvestRequest request)
        {
            var updatedCropSeason = await _cropSeasonService.FinishHarvestAsync(cropSeasonId, request.HarvestDate);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeason(updatedCropSeason, Url, version);
            return Ok(updatedCropSeason);
        }

        /// <summary>
        /// Cancel a crop season (resets status to Planned).
        /// </summary>
        /// <param name="cropSeasonId">Crop Season ID</param>
        /// <returns>Updated crop season</returns>
        [HttpPost("{cropSeasonId}/cancel", Name = "CancelCropSeason")]
        [ProducesResponseType(typeof(CropSeasonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cancel(string cropSeasonId)
        {
            var updatedCropSeason = await _cropSeasonService.CancelCropSeasonAsync(cropSeasonId);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeason(updatedCropSeason, Url, version);
            return Ok(updatedCropSeason);
        }

        #endregion

        #region PUT

        /// <summary>
        /// Update a crop season (only for Planned status).
        /// </summary>
        /// <param name="cropSeasonId">Crop Season ID</param>
        /// <param name="request">Crop Season data to update</param>
        /// <returns>Object crop season updated</returns>
        [HttpPut("{cropSeasonId}", Name = "UpdateCropSeason")]
        [ProducesResponseType(typeof(CropSeasonResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string cropSeasonId, [FromBody] UpdateCropSeasonRequest request)
        {
            if (cropSeasonId.ToUpper() != request.Id.ToUpper())
                return BadRequest(new ErrorResponse 
                { 
                    Message = "Crop Season ID in URL does not match Crop Season ID in request body." 
                });

            var updatedCropSeason = await _cropSeasonService.UpdateCropSeasonAsync(request);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToCropSeason(updatedCropSeason, Url, version);

            return Ok(updatedCropSeason);
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Delete a crop season.
        /// </summary>
        /// <param name="cropSeasonId">Crop Season ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{cropSeasonId}", Name = "DeleteCropSeason")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string cropSeasonId)
        {
            await _cropSeasonService.DeleteCropSeasonAsync(cropSeasonId);
            return NoContent();
        }

        #endregion
    }
}
