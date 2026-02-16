using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Context;

namespace Tests.IntegrationTests.Common;

/// <summary>
/// Helper para popular o banco de dados de teste com dados iniciais.
/// </summary>
public static class DatabaseSeeder
{
    public static void SeedTestData(FarmsDbContext context)
    {
        // Limpa dados existentes
        context.Farms.RemoveRange(context.Farms);
        context.SaveChanges();

        // Seed Farms
        var farm1 = new Farm
        {
            ProducerId = "TEST-PRODUCER-1",
            Name = "Test Farm 1",
            TotalAreaHectares = 1000m,
            IsActive = true,
            Location = new Location("São Paulo", "SP", "Brazil"),
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var farm2 = new Farm
        {
            ProducerId = "TEST-PRODUCER-2",
            Name = "Test Farm 2",
            TotalAreaHectares = 1500m,
            IsActive = true,
            Location = new Location("Minas Gerais", "MG", "Brazil"),
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };

        var farm3 = new Farm
        {
            ProducerId = "TEST-PRODUCER-3",
            Name = "Test Farm 3 (Inactive)",
            TotalAreaHectares = 2000m,
            IsActive = false,
            Location = new Location("Paraná", "PR", "Brazil"),
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        context.Farms.AddRange(farm1, farm2, farm3);
        context.SaveChanges();
    }

    public static Farm CreateTestFarm(
        string producerId = "TEST-PRODUCER", 
        string name = "Test Farm",
        decimal totalAreaHectares = 1000m,
        bool isActive = true,
        string city = "São Paulo",
        string state = "SP",
        string country = "Brazil")
    {
        return new Farm
        {
            ProducerId = producerId,
            Name = name,
            TotalAreaHectares = totalAreaHectares,
            IsActive = isActive,
            Location = new Location(city, state, country),
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
        };
    }
}

