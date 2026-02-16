using Domain.Entities.Base;
using Domain.Exceptions;
using System.Diagnostics;

namespace Domain.Entities
{
    [DebuggerDisplay("Id: {Id}, Name: {Name}, FarmId: {FarmId}, IsActive: {IsActive}")]
    public class Field : AuditableEntity
    {
        public int Id { get; set; }
        public int FarmId { get; set; }
        public required string Name { get; set; }

        private decimal _areaHectares;
        public required decimal AreaHectares 
        { 
            get => _areaHectares;
            set
            {
                if (value <= 0)
                    throw new BusinessException("Field area must be greater than zero.");
                _areaHectares = value;
            }
        }

        private decimal _latitude;
        public required decimal Latitude 
        { 
            get => _latitude;
            set
            {
                if (value < -90 || value > 90)
                    throw new BusinessException("Latitude must be between -90 and 90 degrees.");
                _latitude = value;
            }
        }

        private decimal _longitude;
        public required decimal Longitude 
        { 
            get => _longitude;
            set
            {
                if (value < -180 || value > 180)
                    throw new BusinessException("Longitude must be between -180 and 180 degrees.");
                _longitude = value;
            }
        }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Farm Farm { get; set; } = null!;
        public ICollection<CropSeason> CropSeasons { get; set; } = new List<CropSeason>();

        #region Business Methods

        /// <summary>
        /// Valida se a área do campo não excede a área total da fazenda
        /// </summary>
        public void ValidateAreaAgainstFarm(decimal farmTotalArea, decimal otherFieldsArea)
        {
            if (AreaHectares + otherFieldsArea > farmTotalArea)
                throw new BusinessException($"Total field area ({AreaHectares + otherFieldsArea} ha) exceeds farm total area ({farmTotalArea} ha).");
        }

        /// <summary>
        /// Desativa o campo e todas as suas safras ativas
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            SetUpdatedAudit(UpdatedBy ?? CreatedBy);
        }

        #endregion
    }
}