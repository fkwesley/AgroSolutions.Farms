
using Application.Interfaces;
using Application.Services;
using Application.Settings;
using Domain.Repositories;
using Infrastructure.Context;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services.HealthCheck;
using Infrastructure.Services.Logging;
using Microsoft.EntityFrameworkCore;

namespace API.Configurations;

// #SOLID - Dependency Inversion Principle (DIP)
// Esta classe de configuração registra as abstrações (interfaces) com suas implementações concretas.
// O código cliente sempre depende de interfaces, nunca de implementações.

// #SOLID - Single Responsibility Principle (SRP)
// Esta classe tem uma única responsabilidade: configurar a injeção de dependências.

// #SOLID - Open/Closed Principle (OCP)
// Para adicionar novos serviços, basta registrá-los aqui sem modificar o código existente.
// Por exemplo: adicionar novo logger ou message publisher não requer mudanças em outras classes.

public static class DependencyInjectionConfiguration
{
    public static WebApplicationBuilder AddDependencyInjection(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("AgroDbConnection")
            ?? throw new ArgumentNullException("Connection string 'AgroDbConnection' not found.");

        var jwtKey = builder.Configuration.GetValue<string>("Jwt:Key");
        var jwtIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer");

        // Settings
        builder.Services.Configure<LoggerSettings>(builder.Configuration.GetSection("LoggerSettings"));
        builder.Services.Configure<NewRelicLoggerSettings>(builder.Configuration.GetSection("NewRelic"));
        builder.Services.Configure<ElasticLoggerSettings>(builder.Configuration.GetSection("ElasticLogs"));

        // Domain Services
        builder.Services.AddScoped<IFarmService, FarmService>();
        builder.Services.AddScoped<IFieldService, FieldService>();
        builder.Services.AddScoped<ICropSeasonService, CropSeasonService>();

        // Health Check Services
        // #SOLID - Open/Closed Principle (OCP)
        // Para adicionar novo health check:
        // 1. Criar classe implementando IHealthCheck
        // 2. Registrar aqui: builder.Services.AddScoped<IHealthCheck, MeuNovoHealthCheck>()
        // 3. HealthCheckService descobre automaticamente via IEnumerable<IHealthCheck>
        builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
        builder.Services.AddScoped<IHealthCheck, DatabaseHealthCheck>();
        builder.Services.AddScoped<IHealthCheck, ElasticsearchHealthCheck>();
        builder.Services.AddScoped<IHealthCheck, SystemHealthCheck>();

        // Logger Services
        ConfigureLoggerService(builder);

        // Repositories
        builder.Services.AddScoped<IFarmRepository, FarmRepository>();
        builder.Services.AddScoped<IFieldRepository, FieldRepository>();
        builder.Services.AddScoped<ICropSeasonRepository, CropSeasonRepository>();

        // Database Context
        builder.Services.AddDbContext<FarmsDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                );
            });
        }, ServiceLifetime.Scoped);

        // Other Services
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Serializa todos os enums como string ao invés de número
                // Exemplo: "CropType": "Soja" ao invés de "CropType": 1
                options.JsonSerializerOptions.Converters.Add(
                    new System.Text.Json.Serialization.JsonStringEnumConverter());
                
                // Permite valores case-insensitive (aceita "soja", "SOJA", "Soja")
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });
        builder.Services.AddEndpointsApiExplorer();

        // Elastic APM
        if (builder.Configuration.GetValue<bool>("ElasticApm:Enabled"))
            builder.Services.AddAllElasticApm();

        return builder;
    }

    private static void ConfigureLoggerService(WebApplicationBuilder builder)
    {
        // #SOLID - Liskov Substitution Principle (LSP)
        // DatabaseLoggerService, ElasticLoggerService e NewRelicLoggerService podem ser substituídos
        // entre si sem quebrar o código, pois todos implementam ILoggerService com o mesmo contrato.
        
        // #SOLID - Open/Closed Principle (OCP)
        // Para adicionar novo provider (ex: Azure Monitor), basta:
        // 1. Criar classe implementando ILoggerService
        // 2. Adicionar case no switch
        // Nenhum código cliente precisa ser alterado.
        var loggerProvider = builder.Configuration.GetValue<string>("LoggerSettings:Provider") ?? "Database";

        switch (loggerProvider)
        {
            case "NewRelic":
                builder.Services.AddScoped<ILoggerService, NewRelicLoggerService>();
                break;

            case "Elastic":
                builder.Services.AddScoped<ILoggerService, ElasticLoggerService>();
                break;

            case "Database":
            default:
                builder.Services.AddScoped<ILoggerService, DatabaseLoggerService>();
                builder.Services.AddScoped<IDatabaseLoggerRepository, DatabaseLoggerRepository>();
                break;
        }
    }
}
