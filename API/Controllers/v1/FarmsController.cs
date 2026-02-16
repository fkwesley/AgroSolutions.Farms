using API.Helpers;
using API.Models;
using Application.DTO.Farm;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers.v1
{
    /// <summary>
    /// Farms Controller V1
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [Route("v{version:apiVersion}/farms")]
    public class FarmsController : ControllerBase
    {
        private readonly IFarmService _farmService;

        public FarmsController(IFarmService farmService)
        {
            _farmService = farmService;
        }

        #region GETS
        /// <summary>
        /// Returns all farms registered.
        /// </summary>
        /// <returns>List of Farms</returns>
        [HttpGet(Name = "GetAllFarms")]
        [ProducesResponseType(typeof(IEnumerable<FarmResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var farms = await _farmService.GetAllFarmsAsync();
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToFarms(farms, Url, version);
            return Ok(farms);
        }

        /// <summary>
        /// Returns a farm by id.
        /// </summary>
        /// <param name="farmId">Farm ID</param>
        /// <returns>Object Farm</returns>
        [HttpGet("{farmId}", Name = "GetFarmById")]
        [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int farmId)
        {
            var farm = await _farmService.GetFarmByIdAsync(farmId);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToFarm(farm, Url, version);
            return Ok(farm);
        }
        #endregion

        #region POST
        /// <summary>
        /// Add a farm.
        /// </summary>
        /// <param name="request">Farm data</param>
        /// <returns>Object farm added</returns>
        [HttpPost(Name = "AddFarm")]
        [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add([FromBody] AddFarmRequest request)
        {
            // Extrai o UserId do token JWT e seta como ProducerId
            request.ProducerId = HttpContext.User?.FindFirst("user_id")?.Value ?? "anonymous"; // getting user_id from context (provided by token)

            var addedFarm = await _farmService.AddFarmAsync(request);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToFarm(addedFarm, Url, version);

            return CreatedAtRoute("GetFarmById", 
                new { farmId = addedFarm.FarmId, version }, 
                addedFarm);
        }
        #endregion

        #region PUT
        /// <summary>
        /// Update a farm.
        /// </summary>
        /// <param name="farmId">Farm ID</param>
        /// <param name="request">Farm data to update</param>
        /// <returns>Object farm updated</returns>
        [HttpPut("{farmId}", Name = "UpdateFarm")]
        [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int farmId, [FromBody] UpdateFarmRequest request)
        {
            // Extrai o UserId do token JWT e seta como ProducerId
            request.FarmId = farmId;
            request.UpdatedBy = HttpContext.User?.FindFirst("user_id")?.Value ?? "anonymous"; // getting user_id from context (provided by token)

            var updatedFarm = await _farmService.UpdateFarmAsync(request);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToFarm(updatedFarm, Url, version);

            return Ok(updatedFarm);
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Delete a farm.
        /// </summary>
        /// <param name="farmId">Farm ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{farmId}", Name = "DeleteFarm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int farmId)
        {
            await _farmService.DeleteFarmAsync(farmId);
            return NoContent();
        }
        #endregion
    }
}
