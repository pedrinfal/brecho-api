using Brecho.Api.Modelos;

namespace Brecho.Testes;

public class DominioTestes
{
    [Fact]
    public void Peca_DeveSerCriadaComDadosValidos()
    {
        var peca = new Peca(
            "Jaqueta Jeans Anos 90",
            "Jaqueta original em excelente estado de conservação.",
            "m",
            "Casacos",
            "Seminova",
            120.00m,
            DateTime.UtcNow);

        Assert.Equal("Jaqueta Jeans Anos 90", peca.Nome);
        Assert.Equal("M", peca.Tamanho);
        Assert.Equal(120.00m, peca.Preco);
        Assert.False(peca.Vendida);
        Assert.True(peca.Ativa);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-15.50)]
    public void Peca_ComPrecoZeroOuNegativo_DeveLancarArgumentException(decimal precoInvalido)
    {
        Assert.Throws<ArgumentException>(() => new Peca(
            "Camisa Retrô",
            "Camisa estampada vintage anos 80.",
            "G",
            "Camisas",
            "Ótimo estado",
            precoInvalido,
            DateTime.UtcNow));
    }

    [Fact]
    public void Peca_ComDataNaoUtc_DeveLancarArgumentException()
    {
        var dataLocal = DateTime.Now;

        Assert.Throws<ArgumentException>(() => new Peca(
            "Saia Midi Plissada",
            "Saia xadrez tartan tradicional.",
            "38",
            "Saias",
            "Excelente",
            89.90m,
            dataLocal));
    }

    [Fact]
    public void Peca_JaVendida_MarcarComoVendidaNovamente_DeveLancarInvalidOperationException()
    {
        var peca = new Peca(
            "Vestido Floral Vintage",
            "Vestido midi fluido com estampa botânica.",
            "M",
            "Vestidos",
            "Seminovo",
            110.00m,
            DateTime.UtcNow);

        peca.MarcarComoVendida();
        Assert.True(peca.Vendida);

        Assert.Throws<InvalidOperationException>(() => peca.MarcarComoVendida());
    }

    [Fact]
    public void Usuario_ComPapelInvalido_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Usuario(
            "Maria Silva",
            "maria@exemplo.com",
            "hash-qualquer",
            "gerente",
            DateTime.UtcNow));
    }

    [Fact]
    public void Venda_ComDataNaoUtc_DeveLancarArgumentException()
    {
        var dataUnspecified = new DateTime(2026, 10, 30, 14, 0, 0, DateTimeKind.Unspecified);

        Assert.Throws<ArgumentException>(() => new Venda(
            1,
            1,
            dataUnspecified,
            "PIX"));
    }
}
