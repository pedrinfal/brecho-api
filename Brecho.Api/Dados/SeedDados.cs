using Brecho.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Brecho.Api.Dados;

public static class SeedDados
{
    public static async Task InicializarAsync(BrechoDbContext db, string adminSenha)
    {
        if (string.IsNullOrWhiteSpace(adminSenha))
            throw new InvalidOperationException("A variável Seed__AdminSenha não foi configurada.");

        var agora = DateTime.UtcNow;

        if (!await db.Usuarios.AnyAsync(u => u.Email == "admin@exemplo.com"))
        {
            db.Usuarios.Add(new Usuario(
                "Curadoria Garimpo Vintage (Admin)",
                "admin@exemplo.com",
                BCrypt.Net.BCrypt.HashPassword(adminSenha),
                "admin",
                agora));
        }

        if (!await db.Usuarios.AnyAsync(u => u.Email == "ana@exemplo.com"))
        {
            db.Usuarios.Add(new Usuario(
                "Ana Clara Ribeiro",
                "ana@exemplo.com",
                BCrypt.Net.BCrypt.HashPassword("Cliente@2026"),
                "cliente",
                agora));
        }

        if (!await db.Usuarios.AnyAsync(u => u.Email == "bruno@exemplo.com"))
        {
            db.Usuarios.Add(new Usuario(
                "Bruno Martins Souza",
                "bruno@exemplo.com",
                BCrypt.Net.BCrypt.HashPassword("Cliente@2026"),
                "cliente",
                agora));
        }

        await db.SaveChangesAsync();

        if (!await db.Pecas.AnyAsync())
        {
            var pecasIniciais = new List<Peca>
            {
                new(
                    "Jaqueta Jeans Oversized Anos 90",
                    "Jaqueta jeans lavado claro original dos anos 90, botões metálicos de latão e bolsos frontais amplos.",
                    "G",
                    "Casacos e Jaquetas",
                    "Excelente (seminova)",
                    149.90m,
                    agora),
                new(
                    "Vestido Midi Floral Retrô em Viscose",
                    "Vestido midi fluido com estampa botânica vintage, botões forrados e cintura marcada.",
                    "M",
                    "Vestidos",
                    "Seminovo sem avarias",
                    118.00m,
                    agora),
                new(
                    "Camisa de Seda Pura Cacharel Vintage",
                    "Camisa clássica de garimpo em seda pura tom terracota com abotoamento em madrepérola.",
                    "P",
                    "Camisas e Blusas",
                    "Perfeito estado",
                    135.50m,
                    agora),
                new(
                    "Calça Alfaiataria Lã Fria Cintura Alta",
                    "Calça de alfaiataria corte reto com pregas frontais em tecido de lã fria xadrez príncipe de gales.",
                    "40",
                    "Calças",
                    "Excelente (higienizada)",
                    129.00m,
                    agora),
                new(
                    "Cardigã Tricô Argyle Escocês",
                    "Suéter aberto em tricô encorpado padrão losangos nos tons verde-musgo, creme e vinho.",
                    "M",
                    "Tricôs e Malhas",
                    "Muito bom",
                    95.00m,
                    agora),
                new(
                    "Saia Midi Plissada Tartan Vintage",
                    "Saia xadrez tartan tradicional com pregas permanentes e fechamento lateral por fivela de couro.",
                    "38",
                    "Saias",
                    "Excelente estado",
                    89.90m,
                    agora),
                new(
                    "Blazer Estruturado Tweed com Botões Dourados",
                    "Blazer de corte clássico em tweed texturizado off-white com forro acetinado impecável.",
                    "G",
                    "Casacos e Jaquetas",
                    "Como novo",
                    185.00m,
                    agora),
                new(
                    "Bolsa Tiracolo Couro Legítimo Caramelo Anos 80",
                    "Bolsa transversal de couro legítimo envelhecido naturalmente, ferragens em ouro velho e três divisórias.",
                    "UN",
                    "Acessórios",
                    "Vintage com pátina natural",
                    160.00m,
                    agora),
                new(
                    "Macacão Utilitário Sarja Verde Militar",
                    "Macacão estilo worker em sarja 100% algodão pesado, múltiplos bolsos e ajuste na cintura.",
                    "M",
                    "Macacões",
                    "Seminovo",
                    155.00m,
                    agora),
                new(
                    "Coturno Couro Café Solado Tratorado",
                    "Bota estilo coturno vintage em couro macio cor café, palmilha anatômica nova e cadarço encerado.",
                    "37",
                    "Calçados",
                    "Ótimo estado",
                    172.00m,
                    agora)
            };

            db.Pecas.AddRange(pecasIniciais);
            await db.SaveChangesAsync();
        }
    }
}

