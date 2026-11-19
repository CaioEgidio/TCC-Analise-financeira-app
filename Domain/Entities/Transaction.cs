// Representa uma receita ou despesa e mantém as regras dos lançamentos.
namespace FinData.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    // O valor é positivo; o tipo informa se a movimentação é receita ou despesa.
    public decimal Amount { get; private set; }
    // Representa o dia da movimentação, sem horário.
    public DateOnly Date { get; private set; }
    public TransactionType Type { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public DateTime CreatedAt { get; private set; }
    // Mantém compatibilidade com registros antigos sem proprietário.
    public Guid? UserId { get; private set; }

    // Usado pelo EF Core ao carregar os dados.
    private Transaction()
    {
    }

    // Cria o lançamento e reutiliza Update para aplicar as mesmas regras da edição.
    public Transaction(
    Guid userId,
    string description,
    decimal amount,
    DateOnly date,
    TransactionType type,
    Guid categoryId)
{
    if (userId == Guid.Empty)
        throw new ArgumentException(
            "O usuário é obrigatório.", nameof(userId));

    Update(description, amount, date, type, categoryId);

    Id = Guid.NewGuid();
    UserId = userId;
    CreatedAt = DateTime.UtcNow;
}

    // Só altera os campos depois que todas as validações passam; preserva proprietário e data de criação.
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

        // Recusa valores que precisariam de arredondamento para caber em duas casas decimais.
        if (decimal.Round(amount, 2) != amount)
            throw new ArgumentException(
                "O valor deve ter no máximo duas casas decimais.",
                nameof(amount));

        // Respeita o limite do campo numeric(18,2) definido no banco.
        if (amount > 9999999999999999.99m)
            throw new ArgumentException(
                "O valor excede o limite permitido.", nameof(amount));

        if (date == default)
            throw new ArgumentException(
                "A data é obrigatória.", nameof(date));

        // Impede valores do enum que não correspondam a receita ou despesa.
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