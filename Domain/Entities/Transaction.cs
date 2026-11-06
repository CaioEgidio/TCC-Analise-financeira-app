namespace FinData.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateOnly Date { get; private set; }
    public TransactionType Type { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Usado pelo EF Core ao carregar os dados.
    private Transaction()
    {
    }

    public Transaction(
        string description,
        decimal amount,
        DateOnly date,
        TransactionType type,
        Guid categoryId)
    {
        Update(description, amount, date, type, categoryId);

        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string description,
        decimal amount,
        DateOnly date,
        TransactionType type,
        Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                "A descrição é obrigatória.", nameof(description));

        description = description.Trim();

        if (description.Length > 250)
            throw new ArgumentException(
                "A descrição deve ter no máximo 250 caracteres.",
                nameof(description));

        if (amount <= 0)
            throw new ArgumentException(
                "O valor deve ser maior que zero.", nameof(amount));

        if (decimal.Round(amount, 2) != amount)
            throw new ArgumentException(
                "O valor deve ter no máximo duas casas decimais.",
                nameof(amount));

        if (amount > 9999999999999999.99m)
            throw new ArgumentException(
                "O valor excede o limite permitido.", nameof(amount));

        if (date == default)
            throw new ArgumentException(
                "A data é obrigatória.", nameof(date));

        if (type != TransactionType.Income &&
            type != TransactionType.Expense)
            throw new ArgumentException(
                "O tipo deve ser receita ou despesa.", nameof(type));

        if (categoryId == Guid.Empty)
            throw new ArgumentException(
                "A categoria é obrigatória.", nameof(categoryId));

        Description = description;
        Amount = amount;
        Date = date;
        Type = type;
        CategoryId = categoryId;
    }
}