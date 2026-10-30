using Brecho.Api.Comum;
using Brecho.Api.Dados;
using Brecho.Api.Dtos;
using Brecho.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Brecho.Api.Servicos;

public class VendaServico
{
    private readonly BrechoDbContext _db;
    private readonly ILogger<VendaServico> _logger;

    public VendaServico(BrechoDbContext db, ILogger<VendaServico> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VendaRespostaDto>> ListarAsync(int usuarioId, string papel)
    {
        var query = _db.Vendas
            .AsNoTracking()
            .Include(v => v.Peca)
            .Include(v => v.Usuario)
            .AsQueryable();

        if (papel != "admin")
        {
            query = query.Where(v => v.UsuarioId == usuarioId);
        }

        var vendas = await query
            .OrderByDescending(v => v.DataVenda)
            .ThenByDescending(v => v.Id)
            .ToListAsync();

        return vendas.Select(MapearDto).ToList();
    }

    public async Task<Resultado<VendaRespostaDto>> ObterPorIdAsync(int id, int usuarioId, string papel)
    {
        var venda = await _db.Vendas
            .AsNoTracking()
            .Include(v => v.Peca)
            .Include(v => v.Usuario)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venda is null)
        {
            return Resultado<VendaRespostaDto>.Falha(
                TipoErro.NaoEncontrado,
                $"Venda com identificador {id} não foi encontrada.");
        }

        if (papel != "admin" && venda.UsuarioId != usuarioId)
        {
            _logger.LogWarning("Acesso negado (403): Usuário {UsuarioId} tentou visualizar a venda {VendaId} pertencente ao usuário {DonoId}",
                usuarioId, id, venda.UsuarioId);
            return Resultado<VendaRespostaDto>.Falha(
                TipoErro.Proibido,
                "Você não tem permissão para visualizar a compra de outro cliente.");
        }

        return Resultado<VendaRespostaDto>.Ok(MapearDto(venda));
    }

    public async Task<Resultado<VendaRespostaDto>> CriarAsync(int usuarioId, CriarVendaDto dto)
    {
        await using var transacao = await _db.Database.BeginTransactionAsync();

        var peca = await _db.Pecas.FirstOrDefaultAsync(p => p.Id == dto.PecaId);
        if (peca is null)
        {
            return Resultado<VendaRespostaDto>.Falha(
                TipoErro.NaoEncontrado,
                $"Peça com identificador {dto.PecaId} não foi encontrada no brechó.");
        }

        if (!peca.Ativa)
        {
            _logger.LogWarning("Conflito (409): Tentativa de compra da peça inativa {PecaId} pelo usuário {UsuarioId}", dto.PecaId, usuarioId);
            return Resultado<VendaRespostaDto>.Falha(
                TipoErro.Conflito,
                "Esta peça não está disponível para venda no momento.");
        }

        var vendaConcluidaExistente = await _db.Vendas
            .AnyAsync(v => v.PecaId == dto.PecaId && v.Situacao == "Concluida");

        if (peca.Vendida || vendaConcluidaExistente)
        {
            _logger.LogWarning("Conflito de negócio (409): Peça {PecaId} ({Nome}) já foi vendida. Tentativa do usuário {UsuarioId}",
                peca.Id, peca.Nome, usuarioId);
            return Resultado<VendaRespostaDto>.Falha(
                TipoErro.Conflito,
                $"A peça '{peca.Nome}' já foi vendida (estoque de peça única no brechó).");
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario is null)
        {
            return Resultado<VendaRespostaDto>.Falha(
                TipoErro.NaoEncontrado,
                "Usuário autenticado não foi encontrado.");
        }

        peca.MarcarComoVendida();

        var venda = new Venda(
            peca.Id,
            usuarioId,
            dto.DataVenda.UtcDateTime,
            dto.FormaPagamento,
            dto.Observacoes);

        _db.Vendas.Add(venda);
        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        _logger.LogInformation("Venda criada com sucesso: Venda {VendaId} da peça {PecaId} para o cliente {UsuarioId}",
            venda.Id, peca.Id, usuarioId);

        venda = await _db.Vendas
            .AsNoTracking()
            .Include(v => v.Peca)
            .Include(v => v.Usuario)
            .FirstAsync(v => v.Id == venda.Id);

        return Resultado<VendaRespostaDto>.Ok(MapearDto(venda));
    }

    public async Task<Resultado<VendaRespostaDto>> AtualizarAsync(int id, int usuarioId, string papel, AtualizarVendaDto dto)
    {
        var venda = await _db.Vendas
            .Include(v => v.Peca)
            .Include(v => v.Usuario)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venda is null)
        {
            return Resultado<VendaRespostaDto>.Falha(
                TipoErro.NaoEncontrado,
                $"Venda com identificador {id} não foi encontrada.");
        }

        if (papel != "admin" && venda.UsuarioId != usuarioId)
        {
            _logger.LogWarning("Tentativa de IDOR bloqueada (403): Usuário {UsuarioId} tentou alterar a venda {VendaId} do usuário {DonoId}",
                usuarioId, id, venda.UsuarioId);
            return Resultado<VendaRespostaDto>.Falha(
                TipoErro.Proibido,
                "Você não tem permissão para alterar a compra de outro cliente.");
        }

        if (venda.Situacao == "Cancelada")
        {
            return Resultado<VendaRespostaDto>.Falha(
                TipoErro.Conflito,
                "Não é possível alterar uma venda que já foi cancelada.");
        }

        _db.Entry(venda).Property(v => v.FormaPagamento).CurrentValue = dto.FormaPagamento.Trim();
        _db.Entry(venda).Property(v => v.Observacoes).CurrentValue =
            string.IsNullOrWhiteSpace(dto.Observacoes) ? null : dto.Observacoes.Trim();

        await _db.SaveChangesAsync();

        _logger.LogInformation("Venda {VendaId} atualizada pelo usuário {UsuarioId}", venda.Id, usuarioId);

        return Resultado<VendaRespostaDto>.Ok(MapearDto(venda));
    }

    public async Task<Resultado<bool>> CancelarOuExcluirAsync(int id, int usuarioId, string papel)
    {
        await using var transacao = await _db.Database.BeginTransactionAsync();

        var venda = await _db.Vendas
            .Include(v => v.Peca)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venda is null)
        {
            return Resultado<bool>.Falha(
                TipoErro.NaoEncontrado,
                $"Venda com identificador {id} não foi encontrada.");
        }

        if (papel != "admin" && venda.UsuarioId != usuarioId)
        {
            _logger.LogWarning("Tentativa de IDOR bloqueada (403): Usuário {UsuarioId} tentou cancelar/excluir a venda {VendaId} do usuário {DonoId}",
                usuarioId, id, venda.UsuarioId);
            return Resultado<bool>.Falha(
                TipoErro.Proibido,
                "Você não tem permissão para cancelar ou excluir a compra de outro cliente.");
        }

        if (venda.Peca is not null && venda.Situacao == "Concluida")
        {
            venda.Peca.LiberarParaVenda();
        }

        venda.Cancelar();
        _db.Vendas.Remove(venda);

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        _logger.LogInformation("Venda {VendaId} cancelada/excluída pelo usuário {UsuarioId}; peça {PecaId} liberada no estoque.",
            id, usuarioId, venda.PecaId);

        return Resultado<bool>.Ok(true);
    }

    private static VendaRespostaDto MapearDto(Venda v) => new()
    {
        Id = v.Id,
        PecaId = v.PecaId,
        NomePeca = v.Peca?.Nome ?? string.Empty,
        TamanhoPeca = v.Peca?.Tamanho ?? string.Empty,
        PrecoPeca = v.Peca?.Preco ?? 0m,
        UsuarioId = v.UsuarioId,
        NomeCliente = v.Usuario?.Nome ?? string.Empty,
        DataVenda = v.DataVenda,
        FormaPagamento = v.FormaPagamento,
        Observacoes = v.Observacoes,
        Situacao = v.Situacao
    };
}

