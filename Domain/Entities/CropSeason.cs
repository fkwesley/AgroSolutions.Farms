using Domain.Entities.Base;
using Domain.Enums;
using Domain.Exceptions;
using System.Diagnostics;

namespace Domain.Entities
{
    [DebuggerDisplay("Id: {Id}, CropType: {CropType}, Status: {Status}, FieldId: {FieldId}")]
    public class CropSeason : AuditableEntity
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public required CropType CropType { get; set; }

        private DateTime _plantingDate;
        public required DateTime PlantingDate 
        { 
            get => _plantingDate;
            set
            {
                if (value > DateTime.UtcNow.AddYears(1))
                    throw new BusinessException("Planting date cannot be more than 1 year in the future.");
                _plantingDate = value;
            }
        }

        private DateTime _expectedHarvestDate;
        public required DateTime ExpectedHarvestDate 
        { 
            get => _expectedHarvestDate;
            set
            {
                if (value <= PlantingDate)
                    throw new BusinessException("Expected harvest date must be after planting date.");
                _expectedHarvestDate = value;
            }
        }

        public DateTime? HarvestDate { get; set; }
        public CropSeasonStatus Status { get; set; } = CropSeasonStatus.Planned;

        // Navigation Properties
        public Field Field { get; set; } = null!;

        #region Business Methods

        /// <summary>
        /// Inicia a safra (realiza o plantio)
        /// </summary>
        public void StartPlanting()
        {
            if (Status != CropSeasonStatus.Planned)
                throw new BusinessException("Only planned crop seasons can be started.");

            if (PlantingDate > DateTime.UtcNow)
                throw new BusinessException("Cannot start planting before the planned planting date.");

            Status = CropSeasonStatus.Active;
            SetUpdatedAudit(UpdatedBy ?? CreatedBy);
        }

        /// <summary>
        /// Finaliza a safra (realiza a colheita)
        /// </summary>
        public void FinishHarvest(DateTime harvestDate)
        {
            if (Status != CropSeasonStatus.Active)
                throw new BusinessException("Only active crop seasons can be harvested.");

            if (harvestDate < PlantingDate)
                throw new BusinessException("Harvest date cannot be before planting date.");

            if (harvestDate > DateTime.UtcNow)
                throw new BusinessException("Harvest date cannot be in the future.");

            HarvestDate = harvestDate;
            Status = CropSeasonStatus.Finished;
            SetUpdatedAudit(UpdatedBy ?? CreatedBy);
        }

        /// <summary>
        /// Cancela uma safra planejada
        /// </summary>
        public void Cancel()
        {
            if (Status == CropSeasonStatus.Finished)
                throw new BusinessException("Cannot cancel a finished crop season.");

            Status = CropSeasonStatus.Planned;
            HarvestDate = null;
            SetUpdatedAudit(UpdatedBy ?? CreatedBy);
        }

        /// <summary>
        /// Calcula a duração do ciclo da safra em dias
        /// </summary>
        public int GetCycleDurationInDays()
        {
            if (HarvestDate.HasValue)
                return (HarvestDate.Value - PlantingDate).Days;

            return (ExpectedHarvestDate - PlantingDate).Days;
        }

        /// <summary>
        /// Verifica se a safra está atrasada
        /// </summary>
        public bool IsOverdue()
        {
            if (Status == CropSeasonStatus.Finished)
                return false;

            return DateTime.UtcNow > ExpectedHarvestDate;
        }

        #endregion
    }
}