namespace Domain.Enums
{
    public enum CropSeasonStatus
    {
        Planned = 1,    // Planejada - ainda não iniciou o plantio
        Active = 2,     // Ativa - plantio realizado, aguardando colheita
        Finished = 3,   // Finalizada - colheita realizada
        Canceled = 4    // Cancelada - safra foi cancelada
    }
}
