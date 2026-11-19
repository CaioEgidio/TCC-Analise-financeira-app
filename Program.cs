// Inicializa a API e configura os serviços, a autenticação e as rotas do FinData.
using FinData.Application.Commands.CreateCategory;
using FinData.Application.Commands.CreateTransaction;
using FinData.Application.Commands.DeleteTransaction;
using FinData.Application.Commands.LoginUser;
using FinData.Application.Commands.RegisterUser;
using FinData.Application.Commands.UpdateTransaction;
using FinData.Application.Queries.GetCategories;
using FinData.Application.Queries.GetExpensesByCategory;
using FinData.Application.Queries.GetFinancialSummary;
using FinData.Application.Queries.GetTransactions;
using FinData.Domain.Interfaces;
using FinData.Infrastructure.Identity;
using FinData.Infrastructure.Persistence;
using FinData.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FinData.Application.Interfaces;
using FinData.Infrastructure.OpenApi;

// Carrega configurações e prepara o contêiner de injeção de dependência.
var builder = WebApplication.CreateBuilder(args);

// Plataformas como Render informam a porta HTTP na variável PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    if (!int.TryParse(port, out var portNumber) || portNumber < 1 || portNumber > 65535)
        throw new InvalidOperationException("PORT deve ser uma porta válida.");

    builder.WebHost.UseUrls($"http://0.0.0.0:{portNumber}");
}

// Ativa os controllers e prepara a documentação da API.
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
});

// Configura o EF Core para usar PostgreSQL com a conexão chamada FinData.
builder.Services.AddDbContext<FinDataDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("FinData")));

// Configura contas, regras de senha e bloqueio temporário após falhas de login.
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<FinDataDbContext>()
    .AddSignInManager();

// Habilita os tokens Bearer opacos do Identity; não utiliza JWT neste projeto.
builder.Services
    .AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();

// Liga os contratos dos repositórios às implementações usadas em cada requisição.
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Registra os handlers dos comandos, que coordenam as alterações de dados.
builder.Services.AddScoped<CreateCategoryHandler>();
builder.Services.AddScoped<CreateTransactionHandler>();
builder.Services.AddScoped<DeleteTransactionHandler>();
builder.Services.AddScoped<LoginUserHandler>();
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<UpdateTransactionHandler>();

// Registra os handlers das consultas e o acesso ao usuário autenticado.
builder.Services.AddScoped<GetCategoriesHandler>();
builder.Services.AddScoped<GetExpensesByCategoryHandler>();
builder.Services.AddScoped<GetFinancialSummaryHandler>();
builder.Services.AddScoped<GetTransactionsHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Monta a aplicação com os serviços registrados acima.
var app = builder.Build();

// Opt-in para o ambiente de demonstração com uma única instância.
if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    using var scope = app.Services.CreateScope();
    var database = scope.ServiceProvider.GetRequiredService<FinDataDbContext>();
    await database.Database.MigrateAsync();
}

// Disponibiliza OpenAPI e Swagger apenas durante o desenvolvimento.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

// No Render, HTTPS e o redirecionamento são tratados pelo proxy da plataforma.
if (!builder.Configuration.GetValue<bool>("Hosting:HttpsTerminatedAtProxy"))
    app.UseHttpsRedirection();

// Serve o frontend compilado a partir de wwwroot quando esses arquivos estão presentes.
app.UseDefaultFiles();
app.UseStaticFiles();

// Primeiro reconhece a identidade; depois verifica se ela pode acessar a rota.
app.UseAuthentication();
app.UseAuthorization();

// Publica as rotas dos controllers.
app.MapControllers();
// Confirma que o processo está ativo; esta rota não verifica a conexão com o banco.
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

// Uma rota de API inexistente deve continuar retornando 404, sem receber o HTML.
app.MapFallback("/api/{**path}", () => Results.NotFound());
// Entrega a página React para rotas de navegação que não correspondem a arquivos ou à API.
app.MapFallbackToFile("index.html");

// Inicia o servidor e mantém a aplicação atendendo requisições.
app.Run();
