using FinData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FinData.Domain.Interfaces;
using FinData.Infrastructure.Repositories;
using FinData.Application.Commands.CreateCategory;
using FinData.Application.Commands.CreateTransaction;
using FinData.Application.Queries.GetCategories;
using FinData.Application.Queries.GetTransactions;
using FinData.Application.Queries.GetFinancialSummary;
using FinData.Application.Queries.GetExpensesByCategory;
using FinData.Application.Commands.UpdateTransaction;
using FinData.Application.Commands.DeleteTransaction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<FinDataDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("FinData")));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<UpdateTransactionHandler>();

builder.Services.AddScoped<CreateCategoryHandler>();
builder.Services.AddScoped<CreateTransactionHandler>();
builder.Services.AddScoped<DeleteTransactionHandler>();

builder.Services.AddScoped<GetCategoriesHandler>();
builder.Services.AddScoped<GetTransactionsHandler>();
builder.Services.AddScoped<GetFinancialSummaryHandler>();
builder.Services.AddScoped<GetExpensesByCategoryHandler>();

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

app.MapControllers();

app.Run();

