# FinData - Memoria do Projeto

## Objetivo

FinData e o projeto de TCC: uma aplicacao web de controle financeiro pessoal. O MVP permitira registrar receitas e despesas e visualizar resumos e indicadores financeiros. O frontend sera construido depois com React + TypeScript; neste momento, o foco e o backend.

## Stack definida

- Backend: C# com ASP.NET Core no .NET 10 (net10.0).
- Banco de dados: PostgreSQL, executado localmente com Docker Desktop.
- Persistencia: Entity Framework Core e migrations.
- Documentacao e teste da API: OpenAPI com interface Swagger (NSwag).
- Frontend futuro: React + TypeScript, web primeiro e possibilidade de aplicativo movel depois.

## Arquitetura

Adotamos DDD e CQRS simplificados, inspirados no guia da Microsoft. E um monolito modular: ha uma API e um banco PostgreSQL, sem microsservicos, event sourcing ou bancos separados neste MVP.

```text
Controllers (Input API)
|
Application
|- Commands -> Domain -> Infrastructure -> PostgreSQL
`- Queries  -------------> Infrastructure -> PostgreSQL
```

- Commands alteram o estado da aplicacao e usam entidades e regras de negocio do dominio.
- Queries apenas consultam dados e retornam DTOs/ViewModels para listas, resumos e dashboard.
- O Domain concentra regras financeiras; consultas nao precisam carregar entidades completas quando um DTO e suficiente.

## Estrutura atual

```text
FinData/
|- Application/
|  |- Commands/
|  `- Queries/
|- Controllers/
|- Domain/
|  |- Entities/
|  `- Interfaces/
|- Infrastructure/
|  |- Persistence/
|  `- Repositories/
|- Properties/
|- Program.cs
|- appsettings.json
`- FinData.csproj
```

bin/ e obj/ sao diretorios gerados pela compilacao e nao devem receber codigo manualmente.

## Configuracao ja concluida

- Projeto criado como ASP.NET Core Web API em .NET 10.
- Controllers ativados em Program.cs.
- OpenAPI ativado.
- Interface Swagger via NSwag ativada apenas no ambiente de desenvolvimento.
- API executada com sucesso e Swagger disponivel em /swagger.

## Pacotes instalados

| Pacote | Versao | Finalidade |
| --- | --- | --- |
| Microsoft.AspNetCore.OpenApi | 10.0.12 | Geracao do documento OpenAPI |
| NSwag.AspNetCore | 14.7.1 | Interface Swagger |
| Microsoft.EntityFrameworkCore.Design | 10.0.12 | Ferramentas de migrations do EF Core |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | Provedor EF Core para PostgreSQL |

Nao usar versoes rc, preview ou 11.x desses pacotes enquanto o projeto estiver em .NET 10.

## Escopo do MVP

1. Fundacao de conta/usuario (autenticacao pode entrar apos o fluxo financeiro basico).
2. Categorias de receita e despesa.
3. Transacoes: valor, data, descricao, categoria e tipo (receita/despesa).
4. Consultas de transacoes com filtros.
5. Resumo financeiro mensal: receitas, despesas e saldo.
6. Indicadores e gastos agrupados por categoria para o dashboard.
7. Validacoes, migrations, Swagger e testes das regras principais.

## Proximos passos

1. Instalar e iniciar Docker Desktop usando o mecanismo WSL 2.
2. Criar um docker-compose.yml para o PostgreSQL local do FinData.
3. Configurar a connection string em appsettings.Development.json (sem versionar senhas reais).
4. Criar FinDataDbContext em Infrastructure/Persistence.
5. Criar as primeiras entidades: Category e Transaction.
6. Gerar e aplicar a primeira migration.
7. Implementar o primeiro fluxo vertical: CreateTransaction.

## Convencoes iniciais

- Rotas da API ficarao sob /api.
- Valores financeiros usam decimal, nunca float ou double.
- Datas de transacao serao armazenadas com DateOnly ou DateTime conforme a necessidade definida ao implementar.
- Arquivos de configuracao com segredos locais nao devem ser enviados ao repositorio.
- Atualizar este arquivo sempre que uma decisao arquitetural, pacote, convencao ou marco importante mudar.
