using API.Helpers;
using API.Models;
using Application.DTO.Field;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1
{
    /// <summary>
    /// Fields Controller V1
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    [Route("v{version:apiVersion}/fields")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class FieldsController : ControllerBase
    {
        private readonly IFieldService _fieldService;

        public FieldsController(IFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        #region GETS
        /// <summary>
        /// Returns all fields registered.
        /// </summary>
        /// <returns>List of Fields</returns>
        [HttpGet(Name = "GetAllFields")]
        [ProducesResponseType(typeof(IEnumerable<FieldResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var fields = await _fieldService.GetAllFieldsAsync();
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToFields(fields, Url, version);
            return Ok(fields);
        }

        /// <summary>
        /// Returns a field by id.
        /// </summary>
        /// <param name="fieldId">Field ID</param>
        /// <returns>Object Field</returns>
        [HttpGet("{fieldId}", Name = "GetFieldById")]
        [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int fieldId)
        {
            var field = await _fieldService.GetFieldByIdAsync(fieldId);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToField(field, Url, version);
            return Ok(field);
        }

        /// <summary>
        /// Returns all fields from a specific farm.
        /// </summary>
        /// <param name="farmId">Farm ID</param>
        /// <returns>List of Fields</returns>
        [HttpGet("farm/{farmId}", Name = "GetFieldsByFarm")]
        [ProducesResponseType(typeof(IEnumerable<FieldResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByFarmId(int farmId)
        {
            var fields = await _fieldService.GetFieldsByFarmIdAsync(farmId);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToFields(fields, Url, version);
            return Ok(fields);
        }
        #endregion

        #region POST
        /// <summary>
        /// Add a field.
        /// </summary>
        /// <param name="request">Field data</param>
        /// <returns>Object field added</returns>
        [HttpPost(Name = "AddField")]
        [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Add([FromBody] AddFieldRequest request)
        {
            request.CreatedBy = HttpContext.User?.FindFirst("user_id")?.Value ?? "anonymous";

            var addedField = await _fieldService.AddFieldAsync(request);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToField(addedField, Url, version);

            return CreatedAtRoute("GetFieldById", 
                new { fieldId = addedField.Id, version }, 
                addedField);
        }
        #endregion

        #region PUT
        /// <summary>
        /// Update a field.
        /// </summary>
        /// <param name="fieldId">Field ID</param>
        /// <param name="request">Field data to update</param>
        /// <returns>Object field updated</returns>
        [HttpPut("{fieldId}", Name = "UpdateField")]
        [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int fieldId, [FromBody] UpdateFieldRequest request)
        {
            request.Id = fieldId; 
            request.UpdatedBy = HttpContext.User?.FindFirst("user_id")?.Value ?? "anonymous";

            var updatedField = await _fieldService.UpdateFieldAsync(request);
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToField(updatedField, Url, version);

            return Ok(updatedField);
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Delete a field.
        /// </summary>
        /// <param name="fieldId">Field ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{fieldId}", Name = "DeleteField")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int fieldId)
        {
            await _fieldService.DeleteFieldAsync(fieldId);
            return NoContent();
        }
        #endregion
    }
}
