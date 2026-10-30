using Brecho.Api.Comum;
using Brecho.Api.Dados;
using Brecho.Api.Dtos;
using Brecho.Api.Modelos;
using Brecho.Api.Servicos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Brecho.Testes;

public class ServicosTestes : IDisposable
{
    private readonly SqliteConnection _conexao;
    private readonly BrechoDbContext _db;
    private readonly PecaServico _pecaServico;
    private readonly VendaServico _vendaServico;
    private readonly AuthServico _authServico;

    public ServicosTestes()
    {
        _conexao = new SqliteConnection("DataSource=:memory:");
        _conexao.Open();

        var options = new DbContextOptionsBuilder<BrechoDbContext>()
            .UseSqlite(_conexao)
            .Options;

        _db = new BrechoDbContext(options);
        _db.Database.EnsureCreated();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Segredo"] = "chave-secreta-para-testes-automatizados-com-32-bytes"
            })
            .Build();

        _pecaServico = new PecaServico(_db, NullLogger<PecaServico>.Instance);
        _vendaServico = new VendaServico(_db, NullLogger<VendaServico>.Instance);
        _authServico = new AuthServico(_db, config, NullLogger<AuthServico>.Instance);
    }

    public void Dispose()
    {
        _db.Dispose();
        _conexao.Dispose();
    }

    private async Task<(Usuario Admin, Usuario ClienteA, Usuario ClienteB, Peca Peca1)> PopularBaseTesteAsync()
    {
        var agora = DateTime.UtcNow;
        var admin = new Usuario("Admin Brechó", "admin@exemplo.com", BCrypt.Net.BCrypt.HashPassword("Admin@2026"), "admin", agora);
        var clienteA = new Usuario("Ana Cliente", "ana@exemplo.com", BCrypt.Net.BCrypt.HashPassword("Cliente@2026"), "cliente", agora);
        var clienteB = new Usuario("Bruno Cliente", "bruno@exemplo.com", BCrypt.Net.BCrypt.HashPassword("Cliente@2026"), "cliente", agora);

        var peca1 = new Peca(
            "Jaqueta Couro Vintage Anos 80",
            "Jaqueta original em couro legítimo marrom.",
            "G",
            "Casacos e Jaquetas",
            "Excelente",
            190.00m,
            agora);

        _db.Usuarios.AddRange(admin, clienteA, clienteB);
        _db.Pecas.Add(peca1);
        await _db.SaveChangesAsync();

        return (admin, clienteA, clienteB, peca1);
    }

    [Fact]
    public async Task Venda_CriarParaPecaDisponivel_DeveTerSucessoEMarcarPecaComoVendida()
    {
        var (_, clienteA, _, peca) = await PopularBaseTesteAsync();

        var resultado = await _vendaServico.CriarAsync(clienteA.Id, new CriarVendaDto
        {
            PecaId = peca.Id,
            DataVenda = DateTimeOffset.UtcNow,
            FormaPagamento = "PIX",
            Observacoes = "Retirada na loja no sábado"
        });

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(peca.Id, resultado.Valor!.PecaId);
        Assert.Equal(clienteA.Id, resultado.Valor.UsuarioId);

        var pecaNoBanco = await _db.Pecas.FindAsync(peca.Id);
        Assert.True(pecaNoBanco!.Vendida);
    }

    [Fact]
    public async Task Venda_PecaJaVendida_DeveDevolverErroConflito409()
    {
        var (_, clienteA, clienteB, peca) = await PopularBaseTesteAsync();

        var primeiraVenda = await _vendaServico.CriarAsync(clienteA.Id, new CriarVendaDto
        {
            PecaId = peca.Id,
            DataVenda = DateTimeOffset.UtcNow,
            FormaPagamento = "PIX"
        });
        Assert.True(primeiraVenda.Sucesso);

        var segundaTentativa = await _vendaServico.CriarAsync(clienteB.Id, new CriarVendaDto
        {
            PecaId = peca.Id,
            DataVenda = DateTimeOffset.UtcNow,
            FormaPagamento = "Cartão de Crédito"
        });

        Assert.False(segundaTentativa.Sucesso);
        Assert.Equal(TipoErro.Conflito, segundaTentativa.Erro);
        Assert.Contains("já foi vendida", segundaTentativa.Mensagem);
    }

    [Fact]
    public async Task Autorizacao_ClienteBTentandoCancelarVendaDoClienteA_DeveDevolverProibido403()
    {
        var (_, clienteA, clienteB, peca) = await PopularBaseTesteAsync();

        var vendaCriada = await _vendaServico.CriarAsync(clienteA.Id, new CriarVendaDto
        {
            PecaId = peca.Id,
            DataVenda = DateTimeOffset.UtcNow,
            FormaPagamento = "PIX"
        });

        var tentativaIdor = await _vendaServico.CancelarOuExcluirAsync(
            vendaCriada.Valor!.Id,
            clienteB.Id,
            "cliente");

        Assert.False(tentativaIdor.Sucesso);
        Assert.Equal(TipoErro.Proibido, tentativaIdor.Erro);
    }

    [Fact]
    public async Task Autorizacao_ClienteBTentandoAlterarVendaDoClienteA_DeveDevolverProibido403()
    {
        var (_, clienteA, clienteB, peca) = await PopularBaseTesteAsync();

        var vendaCriada = await _vendaServico.CriarAsync(clienteA.Id, new CriarVendaDto
        {
            PecaId = peca.Id,
            DataVenda = DateTimeOffset.UtcNow,
            FormaPagamento = "PIX"
        });

        var tentativaAlteracao = await _vendaServico.AtualizarAsync(
            vendaCriada.Valor!.Id,
            clienteB.Id,
            "cliente",
            new AtualizarVendaDto
            {
                FormaPagamento = "Boleto",
                Observacoes = "Tentativa indevida"
            });

        Assert.False(tentativaAlteracao.Sucesso);
        Assert.Equal(TipoErro.Proibido, tentativaAlteracao.Erro);
    }

    [Fact]
    public async Task ListagemPrivada_ClienteDeveVerApenasSuasPropriasCompras()
    {
        var (_, clienteA, clienteB, peca1) = await PopularBaseTesteAsync();
        var peca2 = new Peca("Camisa Linho", "Camisa de linho puro bege.", "M", "Camisas", "Seminova", 95.00m, DateTime.UtcNow);
        _db.Pecas.Add(peca2);
        await _db.SaveChangesAsync();

        await _vendaServico.CriarAsync(clienteA.Id, new CriarVendaDto
        {
            PecaId = peca1.Id,
            DataVenda = DateTimeOffset.UtcNow,
            FormaPagamento = "PIX"
        });

        await _vendaServico.CriarAsync(clienteB.Id, new CriarVendaDto
        {
            PecaId = peca2.Id,
            DataVenda = DateTimeOffset.UtcNow,
            FormaPagamento = "Cartão"
        });

        var listaClienteA = await _vendaServico.ListarAsync(clienteA.Id, "cliente");
        var listaAdmin = await _vendaServico.ListarAsync(1, "admin");

        Assert.Single(listaClienteA);
        Assert.Equal(clienteA.Id, listaClienteA[0].UsuarioId);
        Assert.Equal(2, listaAdmin.Count);
    }

    [Fact]
    public async Task BuscaEPaginacao_TermoInexistente_DeveRetornarListaVaziaETotalZero()
    {
        await PopularBaseTesteAsync();

        var resultadoComTermo = await _pecaServico.ListarAsync("couro", pagina: 1, tamanho: 5);
        Assert.Equal(1, resultadoComTermo.Total);
        Assert.Single(resultadoComTermo.Itens);

        var resultadoAbsurdo = await _pecaServico.ListarAsync("zzzz-inexistente-999", pagina: 1, tamanho: 5);
        Assert.Equal(0, resultadoAbsurdo.Total);
        Assert.Empty(resultadoAbsurdo.Itens);
    }

    [Fact]
    public async Task Auth_RegistroComEmailRepetido_DeveDevolverConflito409()
    {
        await PopularBaseTesteAsync();

        var resultado = await _authServico.RegistrarAsync(new RegistroDto
        {
            Nome = "Outra Ana",
            Email = "ana@exemplo.com",
            Senha = "SenhaForte123"
        });

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErro.Conflito, resultado.Erro);
    }

    [Fact]
    public async Task Peca_ExcluirPecaComVendaRegistrada_DeveDevolverConflito409()
    {
        var (_, clienteA, _, peca) = await PopularBaseTesteAsync();

        await _vendaServico.CriarAsync(clienteA.Id, new CriarVendaDto
        {
            PecaId = peca.Id,
            DataVenda = DateTimeOffset.UtcNow,
            FormaPagamento = "PIX"
        });

        var resultadoExclusao = await _pecaServico.ExcluirAsync(peca.Id);

        Assert.False(resultadoExclusao.Sucesso);
        Assert.Equal(TipoErro.Conflito, resultadoExclusao.Erro);
    }
}
