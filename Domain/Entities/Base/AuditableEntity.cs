namespace Domain.Entities.Base
{
    /// <summary>
    /// Base class for entities that require audit tracking.
    /// Implements common audit fields following DDD principles.
    /// </summary>
    public abstract class AuditableEntity
    {
        /// <summary>
        /// User ID who created the entity
        /// </summary>
        public required string CreatedBy { get; set; }

        /// <summary>
        /// Timestamp when the entity was created (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// User ID who last updated the entity
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Timestamp when the entity was last updated (UTC)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Sets audit information for entity creation
        /// </summary>
        public void SetCreatedAudit(string userId)
        {
            CreatedBy = userId;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets audit information for entity update
        /// </summary>
        public void SetUpdatedAudit(string userId)
        {
            UpdatedBy = userId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
