using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Brecho.Api.Comum;
using Brecho.Api.Dados;
using Brecho.Api.Dtos;
using Brecho.Api.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Brecho.Api.Servicos;

public class AuthServico
{
    private readonly BrechoDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthServico> _logger;

    public AuthServico(BrechoDbContext db, IConfiguration config, ILogger<AuthServico> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task<Resultado<AuthRespostaDto>> RegistrarAsync(RegistroDto dto)
    {
        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Usuarios.AnyAsync(u => u.Email == emailNormalizado))
        {
            _logger.LogWarning("Tentativa de cadastro com e-mail já utilizado: {Email}", emailNormalizado);
            return Resultado<AuthRespostaDto>.Falha(
                TipoErro.Conflito,
                "Este endereço de e-mail já está cadastrado no sistema.");
        }

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
        var usuario = new Usuario(dto.Nome, emailNormalizado, senhaHash, "cliente", DateTime.UtcNow);

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Novo cliente registrado com sucesso: {UsuarioId} ({Email})", usuario.Id, usuario.Email);

        return Resultado<AuthRespostaDto>.Ok(GerarRespostaToken(usuario));
    }

    public async Task<Resultado<AuthRespostaDto>> LoginAsync(LoginDto dto)
    {
        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == emailNormalizado);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("Login recusado para o e-mail {Email}", emailNormalizado);
            return Resultado<AuthRespostaDto>.Falha(
                TipoErro.NaoAutorizado,
                "E-mail ou senha inválidos.");
        }

        _logger.LogInformation("Login realizado com sucesso pelo usuário {UsuarioId} com papel {Papel}", usuario.Id, usuario.Papel);

        return Resultado<AuthRespostaDto>.Ok(GerarRespostaToken(usuario));
    }

    private AuthRespostaDto GerarRespostaToken(Usuario usuario)
    {
        var segredo = _config["Jwt:Segredo"];
        if (string.IsNullOrWhiteSpace(segredo) || segredo.Length < 32)
            throw new InvalidOperationException("Configuração Jwt:Segredo inválida ou ausente.");

        var expiraEm = DateTime.UtcNow.AddHours(8);
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Papel)
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: "Brecho.Api",
            audience: "Brecho.Cliente",
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new AuthRespostaDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Papel = usuario.Papel,
            Token = tokenString,
            ExpiraEm = expiraEm
        };
    }
}

