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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
});

builder.Services.AddDbContext<FinDataDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("FinData")));

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

builder.Services
    .AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<CreateCategoryHandler>();
builder.Services.AddScoped<CreateTransactionHandler>();
builder.Services.AddScoped<DeleteTransactionHandler>();
builder.Services.AddScoped<LoginUserHandler>();
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<UpdateTransactionHandler>();

builder.Services.AddScoped<GetCategoriesHandler>();
builder.Services.AddScoped<GetExpensesByCategoryHandler>();
builder.Services.AddScoped<GetFinancialSummaryHandler>();
builder.Services.AddScoped<GetTransactionsHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
