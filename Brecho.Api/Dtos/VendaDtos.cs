using System.ComponentModel.DataAnnotations;

namespace Brecho.Api.Dtos;

public class CriarVendaDto
{
    [Required(ErrorMessage = "O identificador da peça é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "Informe um identificador de peça válido.")]
    public int PecaId { get; set; }

    [Required(ErrorMessage = "A data da venda/retirada é obrigatória.")]
    public DateTimeOffset DataVenda { get; set; }

    [Required(ErrorMessage = "A forma de pagamento é obrigatória.")]
    [MinLength(2, ErrorMessage = "A forma de pagamento deve ter pelo menos 2 caracteres.")]
    [MaxLength(60, ErrorMessage = "A forma de pagamento deve ter no máximo 60 caracteres.")]
    public string FormaPagamento { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres.")]
    public string? Observacoes { get; set; }
}

public class AtualizarVendaDto
{
    [Required(ErrorMessage = "A forma de pagamento é obrigatória.")]
    [MinLength(2, ErrorMessage = "A forma de pagamento deve ter pelo menos 2 caracteres.")]
    [MaxLength(60, ErrorMessage = "A forma de pagamento deve ter no máximo 60 caracteres.")]
    public string FormaPagamento { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres.")]
    public string? Observacoes { get; set; }
}

public class VendaRespostaDto
{
    public int Id { get; set; }
    public int PecaId { get; set; }
    public string NomePeca { get; set; } = string.Empty;
    public string TamanhoPeca { get; set; } = string.Empty;
    public decimal PrecoPeca { get; set; }
    public int UsuarioId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public DateTime DataVenda { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public string Situacao { get; set; } = string.Empty;
}

