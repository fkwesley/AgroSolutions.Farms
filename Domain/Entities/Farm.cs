using Domain.Entities.Base;
using Domain.Exceptions;
using Domain.ValueObjects;
using System.Diagnostics;

namespace Domain.Entities
{
    [DebuggerDisplay("FarmId: {FarmId}, ProducerId: {ProducerId}, IsActive: {IsActive}")]
    public class Farm : AuditableEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ProducerId { get; set; }

        private decimal _totalAreaHectares;
        public required decimal TotalAreaHectares 
        {  
            get => _totalAreaHectares;
            set
            {
                if (value <= 0)
                    throw new BusinessException("Farm total area must be greater than zero.");
                _totalAreaHectares = value;
            }
        }

        public bool IsActive { get; set; } = true;
        public required Location Location { get; set; }

        public ICollection<Field> Fields { get; set; } = new List<Field>();

        #region Business Methods

        /// <summary>
        /// Calcula a área total ocupada pelos campos
        /// </summary>
        public decimal GetTotalFieldsArea()
        {
            return Fields.Sum(f => f.AreaHectares);
        }

        /// <summary>
        /// Calcula a área disponível para novos campos
        /// </summary>
        public decimal GetAvailableArea()
        {
            return TotalAreaHectares - GetTotalFieldsArea();
        }

        /// <summary>
        /// Valida se há área disponível para adicionar um novo campo
        /// </summary>
        public void ValidateAreaForNewField(decimal fieldArea)
        {
            if (fieldArea > GetAvailableArea())
                throw new BusinessException($"Insufficient area. Available: {GetAvailableArea()} ha, Required: {fieldArea} ha.");
        }

        /// <summary>
        /// Desativa a fazenda e todos os seus campos
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            SetUpdatedAudit(UpdatedBy ?? CreatedBy); // Mantém último usuário ou usa criador

            foreach (var field in Fields)
                field.Deactivate();
        }

        #endregion
    }
}
