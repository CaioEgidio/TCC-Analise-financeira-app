

namespace FinData.Domain.Entities
{
    public class Category
    {
        public Guid Id {get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Usado pelo EF core ao carregar dados
        private Category()
        {
        }
        public Category(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome da categoria é obrigatório.", nameof(name));

            name = name.Trim();

            if (name.Length > 100)
                throw new ArgumentException ("O nome deve ter no máximo 100 caracteres.", nameof(name));

            description = string.IsNullOrWhiteSpace(description)
                 ? null
                : description.Trim();

            if (description?.Length > 500)
            throw new ArgumentException("A descrição deve ter no máximo 500 caracteres.",nameof(Description));

            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }
    }
}



