namespace FinData.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Category()
    {
    }

    public Category(Guid userId, string name, string? description)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "O usuário é obrigatório.", nameof(userId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "O nome da categoria é obrigatório.", nameof(name));

        name = name.Trim();

        if (name.Length > 100)
            throw new ArgumentException(
                "O nome deve ter no máximo 100 caracteres.", nameof(name));

        description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        if (description?.Length > 500)
            throw new ArgumentException(
                "A descrição deve ter no máximo 500 caracteres.",
                nameof(description));

        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }
}



