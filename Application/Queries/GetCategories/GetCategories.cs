// DTO: contém apenas os dados da categoria enviados na resposta da consulta.
namespace FinData.Application.Queries.GetCategories;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description);