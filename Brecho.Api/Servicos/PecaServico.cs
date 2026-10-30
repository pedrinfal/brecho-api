using Brecho.Api.Comum;
using Brecho.Api.Dados;
using Brecho.Api.Dtos;
using Brecho.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Brecho.Api.Servicos;

public class PecaServico
{
    private readonly BrechoDbContext _db;
    private readonly ILogger<PecaServico> _logger;

    public PecaServico(BrechoDbContext db, ILogger<PecaServico> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ListaPaginadaDto<PecaRespostaDto>> ListarAsync(
        string? busca,
        int pagina = 1,
        int tamanho = 10,
        bool incluirInativas = false)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho < 1) tamanho = 10;
        if (tamanho > 50) tamanho = 50;

        var query = _db.Pecas.AsNoTracking().AsQueryable();

        if (!incluirInativas)
        {
            query = query.Where(p => p.Ativa);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                p.Descricao.ToLower().Contains(termo) ||
                p.Categoria.ToLower().Contains(termo) ||
                p.Tamanho.ToLower().Contains(termo));
        }

        var total = await query.CountAsync();
        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)tamanho);

        var itens = await query
            .OrderByDescending(p => p.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(p => MapearDto(p))
            .ToListAsync();

        return new ListaPaginadaDto<PecaRespostaDto>
        {
            Itens = itens,
            Pagina = pagina,
            Tamanho = tamanho,
            Total = total,
            TotalPaginas = totalPaginas
        };
    }

    public async Task<Resultado<PecaRespostaDto>> ObterPorIdAsync(int id)
    {
        var peca = await _db.Pecas.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (peca is null)
        {
            return Resultado<PecaRespostaDto>.Falha(
                TipoErro.NaoEncontrado,
                $"Peça com identificador {id} não foi encontrada no acervo.");
        }

        return Resultado<PecaRespostaDto>.Ok(MapearDto(peca));
    }

    public async Task<Resultado<PecaRespostaDto>> CriarAsync(CriarPecaDto dto)
    {
        var peca = new Peca(
            dto.Nome,
            dto.Descricao,
            dto.Tamanho,
            dto.Categoria,
            dto.Condicao,
            dto.Preco,
            DateTime.UtcNow);

        _db.Pecas.Add(peca);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Peça cadastrada no acervo: {PecaId} ({Nome}) por R$ {Preco}", peca.Id, peca.Nome, peca.Preco);

        return Resultado<PecaRespostaDto>.Ok(MapearDto(peca));
    }

    public async Task<Resultado<PecaRespostaDto>> AtualizarAsync(int id, AtualizarPecaDto dto)
    {
        var peca = await _db.Pecas.FirstOrDefaultAsync(p => p.Id == id);
        if (peca is null)
        {
            return Resultado<PecaRespostaDto>.Falha(
                TipoErro.NaoEncontrado,
                $"Peça com identificador {id} não foi encontrada.");
        }

        peca.Atualizar(
            dto.Nome,
            dto.Descricao,
            dto.Tamanho,
            dto.Categoria,
            dto.Condicao,
            dto.Preco,
            dto.Ativa);

        await _db.SaveChangesAsync();

        _logger.LogInformation("Peça atualizada no acervo: {PecaId} ({Nome})", peca.Id, peca.Nome);

        return Resultado<PecaRespostaDto>.Ok(MapearDto(peca));
    }

    public async Task<Resultado<bool>> ExcluirAsync(int id)
    {
        var peca = await _db.Pecas
            .Include(p => p.Vendas)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (peca is null)
        {
            return Resultado<bool>.Falha(
                TipoErro.NaoEncontrado,
                $"Peça com identificador {id} não foi encontrada.");
        }

        if (peca.Vendas.Any())
        {
            _logger.LogWarning("Conflito ao tentar excluir peça {PecaId} que já possui histórico de vendas.", id);
            return Resultado<bool>.Falha(
                TipoErro.Conflito,
                "Não é possível excluir esta peça porque existem vendas registradas para ela. Desative a peça em vez de excluí-la.");
        }

        _db.Pecas.Remove(peca);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Peça excluída do acervo: {PecaId}", id);

        return Resultado<bool>.Ok(true);
    }

    private static PecaRespostaDto MapearDto(Peca p) => new()
    {
        Id = p.Id,
        Nome = p.Nome,
        Descricao = p.Descricao,
        Tamanho = p.Tamanho,
        Categoria = p.Categoria,
        Condicao = p.Condicao,
        Preco = p.Preco,
        Vendida = p.Vendida,
        Ativa = p.Ativa,
        CriadaEm = p.CriadaEm
    };
}

