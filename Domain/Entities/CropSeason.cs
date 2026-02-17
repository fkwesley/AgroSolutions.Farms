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

        private DateOnly _plantingDate;
        public required DateOnly PlantingDate 
        { 
            get => _plantingDate;
            set
            {
                if (value > DateOnly.FromDateTime(DateTime.Today.AddYears(1)))
                    throw new BusinessException("Planting date cannot be more than 1 year in the future.");
                _plantingDate = value;
                UpdateStatusBasedOnDates();
            }
        }

        private DateOnly _expectedHarvestDate;
        public required DateOnly ExpectedHarvestDate 
        { 
            get => _expectedHarvestDate;
            set
            {
                if (value <= PlantingDate)
                    throw new BusinessException("Expected harvest date must be after planting date.");
                _expectedHarvestDate = value;
                UpdateStatusBasedOnDates();
            }
        }

        private DateOnly? _harvestDate;
        public DateOnly? HarvestDate 
        { 
            get => _harvestDate;
            set
            {
                if (value.HasValue)
                {
                    var today = DateOnly.FromDateTime(DateTime.UtcNow);

                    if (value.Value > today)
                        throw new BusinessException("Harvest date cannot be in the future. Only current or past dates are allowed.");

                    if (value.Value < PlantingDate)
                        throw new BusinessException("Harvest date cannot be before planting date.");
                }

                _harvestDate = value;
                UpdateStatusBasedOnDates();
            }
        }

        private CropSeasonStatus _status = CropSeasonStatus.Planned;
        public CropSeasonStatus Status 
        { 
            get => _status;
            set => _status = value;
        }

        // Navigation Properties
        public Field Field { get; set; } = null!;

        #region Business Methods
        /// <summary>
        /// Atualiza o status da safra automaticamente com base nas datas
        /// </summary>
        private void UpdateStatusBasedOnDates()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Se tem data de colheita realizada e está no passado ou hoje → Finished
            if (HarvestDate.HasValue && HarvestDate.Value <= today)
            {
                _status = CropSeasonStatus.Finished;
                return;
            }

            // Se a data de plantio é FUTURA (amanhã em diante) → Planned
            if (PlantingDate > today)
            {
                _status = CropSeasonStatus.Planned;
                return;
            }

            // Se o plantio é hoje ou passou (hoje/passado) mas ainda não colheu → Active
            if (PlantingDate <= today && !HarvestDate.HasValue)
            {
                _status = CropSeasonStatus.Active;
                return;
            }

            // Se tem data de colheita no futuro → Active
            if (HarvestDate.HasValue && HarvestDate.Value > today)
                _status = CropSeasonStatus.Active;
        }

        /// <summary>
        /// Força atualização do status baseado nas datas (útil após carregar do banco)
        /// </summary>
        public void RecalculateStatus()
        {
            UpdateStatusBasedOnDates();
        }

        /// <summary>
        /// Inicia o plantio da safra (pode ser antecipado se PlantingDate for amanhã ou hoje)
        /// </summary>
        public void StartPlanting()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (Status != CropSeasonStatus.Planned)
                throw new BusinessException("Only planned crop seasons can be started.");

            // Permite iniciar no dia do plantio ou até 1 dia antes (antecipação)
            if (PlantingDate > today.AddDays(1))
                throw new BusinessException("Cannot start planting more than 1 day before the planned planting date.");

            _status = CropSeasonStatus.Active;
            SetUpdatedAudit(UpdatedBy ?? CreatedBy);
        }

        /// <summary>
        /// Registra a colheita da safra
        /// </summary>
        public void RegisterHarvest(DateOnly harvestDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (Status == CropSeasonStatus.Finished)
                throw new BusinessException("Crop season is already finished.");

            if (harvestDate < PlantingDate)
                throw new BusinessException("Harvest date cannot be before planting date.");

            if (harvestDate > today)
                throw new BusinessException("Harvest date cannot be in the future.");

            _harvestDate = harvestDate;
            _status = CropSeasonStatus.Finished;
            SetUpdatedAudit(UpdatedBy ?? CreatedBy);
        }

        /// <summary>
        /// Finaliza a safra (realiza a colheita) - Sobrecarga para compatibilidade
        /// </summary>
        public void FinishHarvest(DateOnly harvestDate)
        {
            RegisterHarvest(harvestDate);
        }

        /// <summary>
        /// Cancela uma safra e define status como Canceled
        /// </summary>
        public void Cancel()
        {
            if (Status == CropSeasonStatus.Finished)
                throw new BusinessException("Cannot cancel a finished crop season.");

            if (Status == CropSeasonStatus.Canceled)
                throw new BusinessException("Crop season is already canceled.");

            _harvestDate = null;
            _status = CropSeasonStatus.Canceled;
            SetUpdatedAudit(UpdatedBy ?? CreatedBy);
        }

        /// <summary>
        /// Calcula a duração do ciclo da safra em dias
        /// </summary>
        public int GetCycleDurationInDays()
        {
            if (HarvestDate.HasValue)
                return HarvestDate.Value.DayNumber - PlantingDate.DayNumber;

            return ExpectedHarvestDate.DayNumber - PlantingDate.DayNumber;
        }

        /// <summary>
        /// Verifica se a safra está atrasada
        /// </summary>
        public bool IsOverdue()
        {
            // Safras finalizadas ou canceladas não estão atrasadas
            if (Status == CropSeasonStatus.Finished || Status == CropSeasonStatus.Canceled)
                return false;

            return DateOnly.FromDateTime(DateTime.UtcNow) > ExpectedHarvestDate;
        }

        #endregion
    }
}