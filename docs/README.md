# 🌾 AgroSolutions.Farms - Hackaton FIAP

> API RESTful para gestão de fazendas, talhões e temporadas de plantio.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![REST Level 3](https://img.shields.io/badge/REST-Level%203%20(HATEOAS)-success)](https://martinfowler.com/articles/richardsonMaturityModel.html)

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [API RESTful - Nível 3](#-api-restful---nível-3)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Funcionalidades Principais](#-funcionalidades-principais)
- [Princípios SOLID](#-princípios-solid)
- [Tecnologias](#-tecnologias)
- [Setup Rápido](#-setup-rápido)
- [Testes](#-testes)

---

## 🎯 Visão Geral

API RESTful escalável e manutenível, implementando as melhores práticas de arquitetura de software.

### 🌟 Destaques

- ✅ **Clean Architecture** (Onion Architecture)
- ✅ **REST Level 3** (HATEOAS completo)
- ✅ **SOLID Principles** aplicados rigorosamente
- ✅ **Domain-Driven Design** (DDD)
- ✅ **Health Checks** dinâmicos com auto-discovery
- ✅ **Observabilidade completa** (Logs estruturados, Correlation IDs)
- ✅ **Testes em 4 camadas** (Unit, Integration, Architecture, Load)

---

### 📌 Requisitos Hackaton FIAP
  - **Arquitetura baseada em microserviços**
    - Microserviço para gestão de úsuários e autenticação JWT
    - Microserviço para gestão de fazendas, talhões e safras
    - Microserviço para injestão dos dados dos sensores 
    - Funções Serverless para coleta e processamento de dados em tempo real
  - **Orquestração com Kubernetes**
    - Imagens Docker otimizadas para .NET 8 (Alpine) 
    - Armazenamento das imagens no Azure Container Registry (ACR)
    - Microserviços hospedados em Azure Kubernetes Services (AKS)   
    - Manifestos Kubernetes para deploy, service, hpa, configMap e secrets
  - **Observabilidade**
    - Elastic APM para monitoramento de performance e rastreamento distribuído
    - Elasticsearch para armazenamento e análise de logs estruturados
    - Kibana para dashboards
  - **Mensageria**
    - ServiceBus para comunicação assíncrona entre microserviços
    - Azure Functions (Queue trigger) para processamento de mensagens em tempo real
    - Azure Functions (Timer trigger) para coleta de dados dos sensores a cada hora
  - **CI/CD Automatizado**
    - Github Actions para build, testes, build de imagem Docker e deploy no AKS
    - Stages de DEV, STAGING e PROD com aprovações manuais para deploy em produção
  - **Adoção das melhores práticas de arquitetura e dev**
    - Clean Architecture (Onion Architecture)
    - API RESTful Level 3 (HATEOAS completo)
    - SOLID Principles aplicados rigorosamente
    - Patterns como Repository, Unit of Work, Factory, Strategy
    - Domain-Driven Design (DDD)
    - Health Checks dinâmicos
    - Observabilidade completa (Logs estruturados, Correlation IDs)
    - Testes em 4 camadas (Unit, Integration, Architecture, Load)

---
  
## 🏗️ Arquitetura

### Clean Architecture (Onion)

```
┌─────────────────────────────────────────────┐
│              API (Presentation)             │  ← Controllers, Middlewares
├─────────────────────────────────────────────┤
│           Application (Use Cases)           │  ← Services, DTOs, Mappings
├─────────────────────────────────────────────┤
│              Domain (Core)                  │  ← Entities, Events, Business Rules
├─────────────────────────────────────────────┤
│          Infrastructure (External)          │  ← DB, Messaging, External APIs
└─────────────────────────────────────────────┘
```

**Dependency Rule:** Domain ← Application ← Infrastructure ← API

**Benefícios:**
- ✅ Domain independente de infraestrutura
- ✅ Fácil substituição de frameworks/bancos
- ✅ Testável sem dependências externas
- ✅ Escalável e manutenível

---

## 🌐 API RESTful - Nível 3 (HATEOAS)

### Richardson Maturity Model
```
Nível 3: HATEOAS     ← ✅ Esta API
Nível 2: HTTP Verbs  ← ✅
Nível 1: Resources   ← ✅
Nível 0: POX         
```

### Requisitos REST Implementados

| Requisito | Descrição | Status | Padrão |
|-----------|-----------|--------|--------|
| **URIs substantivos** | Recursos com substantivos no plural | ✅ | `/farms`, `/fields`, `/crop-seasons` |
| **Hierarquia de URIs** | Relacionamentos claros | ✅ | `/fields/farm/{farmId}` |
| **HTTP Verbs** | GET, POST, PUT, DELETE corretos | ✅ | Semântica HTTP |
| **Idempotência** | GET, PUT, DELETE idempotentes | ✅ | RFC 7231 |
| **Status Codes** | 2xx, 3xx, 4xx, 5xx apropriados | ✅ | HTTP Standards |
| **HATEOAS** | Links de navegação em respostas | ✅ | Richardson Level 3 |
| **Links Dinâmicos** | Links baseados no estado do recurso | ✅ | State Machine |
| **Versionamento** | URL + Header versioning | ✅ | `/v1/`, `/v2/` |
| **Paginação** | Metadados + links navegação | ✅ | `page`, `pageSize` |
| **Content Negotiation** | Accept/Content-Type headers | ✅ | `application/json` |
| **Error Handling** | RFC 7807 Problem Details | ✅ | Padronizado |
| **Stateless** | Sem estado no servidor | ✅ | JWT tokens |
| **Cacheable** | Headers de cache | ✅ | `Cache-Control`, `ETag` |
| **CORS** | Cross-Origin Resource Sharing | ✅ | Configurável |
| **Correlation IDs** | Rastreamento distribuído | ✅ | `X-Correlation-ID` |

### REST Constraints (Roy Fielding)

| Constraint | Status |
|-----------|--------|
| Client-Server | ✅ Separação de responsabilidades |
| Stateless | ✅ Sem sessão, requisições auto-contidas |
| Cacheable | ✅ Headers de cache (`ETag`, `Cache-Control`) |
| Layered System | ✅ Load Balancer → Gateway → API → DB |
| Uniform Interface | ✅ URIs padronizadas, HATEOAS |
| Code on Demand | ⚠️ Opcional (não implementado) |

**Conformidade REST:** 95% (16/17 requisitos implementados)

---

## 📁 Estrutura do Projeto

```
AgroSolutions.Farms/
│
├── 📂 API/                          # Presentation Layer
│   ├── Controllers/v1/                 # Endpoints versionados
│   ├── Middlewares/                    # Error, Logging, Security, Cache
│   ├── Configurations/                 # DI, Swagger, CORS, Auth, Versioning
│   ├── Helpers/                        # HATEOAS Helper
│   ├── Models/                         # ErrorResponse
│   ├── Program.cs                      # Entry point, Startup
│   ├── appsettings.json                # Configurações de produção
│   └── appsettings.Development.json    # Configurações de desenvolvimento
│
├── 📂 Application/                  # Use Cases
│   ├── Services/                       # Lógica de negócio (FarmService, FieldService, CropSeasonService)
│   ├── Interfaces/                     # Contratos (IFarmService, IFieldService, ICropSeasonService)
│   ├── Mappings/                       # Extensions de mapeamento Entity ↔ DTO
│   ├── DTO/                            # Request/Response DTOs
│   ├── Helpers/                        # DateTimeHelper
│   └── Settings/                       # Configurações tipadas (Logger, Elastic)
│
├── 📂 Domain/                       # Core Business
│   ├── Entities/                       # Farm, Field, CropSeason (Aggregates)
│   ├── ValueObjects/                   # Location
│   ├── Enums/                          # CropSeasonStatus, CropType
│   ├── Exceptions/                     # BusinessException
│   └── Repositories/                   # IFarmRepository, IFieldRepository, ICropSeasonRepository
│
├── 📂 Infrastructure/               # External Concerns
│   ├── Context/                        # FarmsDbContext, CorrelationContext
│   ├── Configurations/                 # EF Core entity configurations
│   ├── Repositories/                   # FarmRepository, FieldRepository, CropSeasonRepository
│   ├── Services/                       # Logging (Database, Elastic, NewRelic), Health Checks
│   └── Migrations/                     # EF Core Migrations
│
├── 📂 Tests/                        # Tests Layer
│   ├── UnitTests/                      # Mocks, lógica isolada
│   ├── IntegrationTests/               # EF Core InMemory, repositories
│   ├── ArchitectureTests/              # NetArchTest (Clean Architecture, SOLID, Security)
│   └── LoadTests/                      # k6, load testing
│
├── 📂 docs/                         # Documentation
│   ├── SOLID_Summary.md                # Análise SOLID detalhada
│   ├── Architecture.drawio             # Diagramas de arquitetura
│   └── README.md                       # Este arquivo
│
├── 📂 Kubernetes/                   # Kubernetes manifests (deployment, service, hpa, secrets)
├── 📂 .github/                      # GitHub workflows (CI/CD para AKS)
├── .gitignore                       # Arquivos ignorados pelo Git
├── Dockerfile                       # Imagem Docker da API
└── AgroSolutions.Farms.sln          # Solution .NET
```

---

## ✨ Funcionalidades Principais

### 🔹 Gestão de Fazendas (Farms)
- CRUD completo com validações de negócio (área, duplicação)
- Links HATEOAS para navegação entre recursos

### 🔹 Gestão de Talhões (Fields)
- CRUD vinculado a fazendas com validação de área disponível
- Geolocalização via Value Object `Location` (latitude/longitude)

### 🔹 Gestão de Safras (CropSeasons)
- Ciclo de vida completo: Planned → Active → Harvesting → Finished / Cancelled
- Validação de conflito de datas por talhão
- Controle de safras atrasadas (overdue)

### 🔹 Observabilidade
- **Logging multi-destino:** Database, Elasticsearch, New Relic
- **Correlation IDs:** Rastreamento distribuído
- **Structured Logs:** Serilog com contexto completo

### 🔹 Health Checks Dinâmicos
- **Auto-discovery** via `IEnumerable<IHealthCheck>`
- **Criticidade:** Database, Elasticsearch, System
- **Extensível:** Adicione novo check sem modificar código existente

### 🔹 Segurança
- HTTPS enforcement
- JWT Bearer Authentication
- Security Headers (HSTS, CSP, X-Frame-Options)

---

## 🎯 Princípios SOLID

### Resumo

| Princípio | Aplicação |
|-----------|-----------|
| **S** - Single Responsibility | Cada classe tem 1 responsabilidade (FarmService, FarmRepository) |
| **O** - Open/Closed | Extensível sem modificar (IHealthCheck → ElasticsearchHealthCheck) |
| **L** - Liskov Substitution | ILoggerService → Database/Elastic/NewRelic substituíveis |
| **I** - Interface Segregation | Interfaces coesas (IFarmRepository, IFieldRepository, IHealthCheck) |
| **D** - Dependency Inversion | Depende de abstrações, não implementações |

### Exemplos Práticos

**Adicionar novo Health Check:**
```csharp
// 1. Implementar interface
public class RedisHealthCheck : IHealthCheck
{
    public string ComponentName => "Redis";
    public bool IsCritical => false;
    public Task<ComponentHealth> CheckHealthAsync() { ... }
}

// 2. Registrar no DI
builder.Services.AddScoped<IHealthCheck, RedisHealthCheck>();

// ✅ HealthCheckService descobre automaticamente!
```

**Trocar Logger:**
```csharp
// Apenas alterar configuração
"LoggerSettings": { "Provider": "Elastic" }  // ou "Database", "NewRelic"

// Código cliente não muda! (Dependency Inversion)
```

📚 **Documentação completa:** `docs/SOLID_Summary.md`

---

## 🛠️ Tecnologias

**Core:** .NET 8, C# 12, ASP.NET Core 8  
**Persistência:** EF Core 8, SQL Server  
**Logging:** Serilog (Elasticsearch, Database, New Relic)  
**APM:** Elastic APM  
**Testes:** xUnit, Moq, FluentAssertions, NetArchTest, k6  
**Infra:** Docker, Kubernetes (AKS), GitHub Actions  
**Documentação:** Swagger/OpenAPI 3.0  

---

## 🚀 CI/CD

### Pipeline Automatizado (GitHub Actions → AKS)

A aplicação possui pipeline CI/CD via GitHub Actions (`.github/workflows/ci-cd-aks.yml`) com:
- Build e execução de testes automatizados
- Build e push de imagem Docker
- Deploy no Azure Kubernetes Service (AKS)

---

## 🚀 Setup Rápido

```bash
# 1. Clonar
git clone https://github.com/fkwesley/AgroSolutions.Farms.git
cd AgroSolutions.Farms

# 2. Restaurar dependências
dotnet restore

# 3. Configurar banco (appsettings.Development.json)
"ConnectionStrings": {
  "FarmsDbConnection": "Server=localhost;Database=FarmsDb;..."
}

# 4. Aplicar migrations
dotnet ef database update --project Infrastructure --startup-project API

# 5. Executar
cd API
dotnet run

# ✅ API disponível em: https://localhost:5001/swagger
```

---

## 🧪 Testes

### Pirâmide de Testes
```
      /\
     /E2E\        ← 5% (críticos)
    /------\
   / Integr \     ← 20% (DB real)
  /----------\
 /Unit Tests  \   ← 70% (mocks)
/______________\
  + Architecture  ← 5% (regras)
```

### Executar
```bash
dotnet test                                    # Todos
dotnet test --filter "Category=Unit"          # Unitários
dotnet test --filter "Category=Integration"   # Integração
k6 run Tests/LoadTests/load-test.js            # Carga
```

### Arquitetura (NetArchTest)
```csharp
// Valida Clean Architecture
Types.InAssembly(domainAssembly)
    .ShouldNot().HaveDependencyOn("Infrastructure")
    .GetResult().IsSuccessful.Should().BeTrue();
```

---

## 👨‍💻 Autor

**Frank Vieira** - [GitHub](https://github.com/fkwesley)
