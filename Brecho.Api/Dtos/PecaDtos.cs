using System.ComponentModel.DataAnnotations;

namespace Brecho.Api.Dtos;

public class CriarPecaDto
{
    [Required(ErrorMessage = "O nome da peça é obrigatório.")]
    [MinLength(3, ErrorMessage = "O nome da peça deve ter pelo menos 3 caracteres.")]
    [MaxLength(150, ErrorMessage = "O nome da peça deve ter no máximo 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição da peça é obrigatória.")]
    [MinLength(5, ErrorMessage = "A descrição da peça deve ter pelo menos 5 caracteres.")]
    [MaxLength(600, ErrorMessage = "A descrição da peça deve ter no máximo 600 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tamanho da peça é obrigatório.")]
    [MaxLength(20, ErrorMessage = "O tamanho deve ter no máximo 20 caracteres.")]
    public string Tamanho { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria da peça é obrigatória.")]
    [MaxLength(80, ErrorMessage = "A categoria deve ter no máximo 80 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "A condição da peça é obrigatória.")]
    [MaxLength(80, ErrorMessage = "A condição deve ter no máximo 80 caracteres.")]
    public string Condicao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço da peça é obrigatório.")]
    [Range(0.01, 99999.99, ErrorMessage = "O preço da peça deve ser maior que zero.")]
    public decimal Preco { get; set; }
}

public class AtualizarPecaDto
{
    [Required(ErrorMessage = "O nome da peça é obrigatório.")]
    [MinLength(3, ErrorMessage = "O nome da peça deve ter pelo menos 3 caracteres.")]
    [MaxLength(150, ErrorMessage = "O nome da peça deve ter no máximo 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição da peça é obrigatória.")]
    [MinLength(5, ErrorMessage = "A descrição da peça deve ter pelo menos 5 caracteres.")]
    [MaxLength(600, ErrorMessage = "A descrição da peça deve ter no máximo 600 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tamanho da peça é obrigatório.")]
    [MaxLength(20, ErrorMessage = "O tamanho deve ter no máximo 20 caracteres.")]
    public string Tamanho { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria da peça é obrigatória.")]
    [MaxLength(80, ErrorMessage = "A categoria deve ter no máximo 80 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "A condição da peça é obrigatória.")]
    [MaxLength(80, ErrorMessage = "A condição deve ter no máximo 80 caracteres.")]
    public string Condicao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O preço da peça é obrigatório.")]
    [Range(0.01, 99999.99, ErrorMessage = "O preço da peça deve ser maior que zero.")]
    public decimal Preco { get; set; }

    public bool Ativa { get; set; } = true;
}

public class PecaRespostaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Condicao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public bool Vendida { get; set; }
    public bool Ativa { get; set; }
    public DateTime CriadaEm { get; set; }
}

public class ListaPaginadaDto<T>
{
    public IReadOnlyList<T> Itens { get; set; } = Array.Empty<T>();
    public int Pagina { get; set; }
    public int Tamanho { get; set; }
    public int Total { get; set; }
    public int TotalPaginas { get; set; }
}

