# Garimpo Vintage — API de Brechó
**Nicho:** Brechó ou loja de roupas · **Aluno:** Pedro Falleiro Mendes (@pedrinfal)
**API no ar:** https://brecho-api.onrender.com/swagger
**Saúde e versão:** https://brecho-api.onrender.com/health · https://brecho-api.onrender.com/api/versao
**Front-end:** https://brecho-front.onrender.com

## O que a API faz
API REST para gestão de acervo e vendas de um brechó de moda circular e garimpo vintage (**Garimpo Vintage**).
- **Quem usa:** visitantes e clientes consultam o catálogo público de peças únicas com busca e paginação; clientes autenticados registram suas compras e consultam/cancelam seus próprios pedidos; administradores (`admin`) gerenciam o catálogo completo de peças (criação, edição, ativação/inativação e exclusão) e acompanham todas as vendas.
- **Regra de negócio que devolve `409 Conflict`:** como em um brechó cada peça garimpada possui estoque exclusivo de **uma única unidade**, tentar registrar uma venda (`POST /api/vendas`) para uma peça que já foi vendida (`Vendida == true` ou com venda concluída ativa) ou inativa é bloqueado dentro de uma transação (`BeginTransactionAsync`) e retorna `409 Conflict` em formato `ProblemDetails` com mensagem clara. Também retorna `409 Conflict` ao tentar excluir uma peça que já possui histórico de vendas ou ao tentar cadastrar um e-mail já utilizado.

## Recursos e endpoints
| Verbo | Rota | Quem pode | Respostas |
|---|---|---|---|
| GET | `/health` | todos | 200 |
| GET | `/api/versao` | todos | 200 |
| POST | `/api/auth/registro` | todos | 201, 400, 409 |
| POST | `/api/auth/login` | todos | 200, 400, 401 |
| GET | `/api/pecas?busca=&pagina=&tamanho=` | todos | 200 |
| GET | `/api/pecas/{id}` | todos | 200, 404 |
| POST | `/api/pecas` | admin | 201, 400, 401, 403 |
| PUT | `/api/pecas/{id}` | admin | 200, 400, 401, 403, 404 |
| DELETE | `/api/pecas/{id}` | admin | 204, 401, 403, 404, 409 |
| GET | `/api/vendas` | cliente (suas) / admin (todas) | 200, 401 |
| GET | `/api/vendas/{id}` | dono ou admin | 200, 401, 403, 404 |
| POST | `/api/vendas` | cliente / admin | 201, 400, 401, 404, 409 |
| PUT | `/api/vendas/{id}` | dono ou admin | 200, 400, 401, 403, 404, 409 |
| DELETE | `/api/vendas/{id}` | dono ou admin | 204, 401, 403, 404 |

## Rotas para o corretor testar (sem token)
- Listagem paginada: `/api/pecas?pagina=1&tamanho=5`
- Busca (exemplo que encontra algo): `/api/pecas?busca=jaqueta`
- Detalhe: `/api/pecas/1` · inexistente (404): `/api/pecas/9999`
- Protegida (401 sem token): `/api/vendas`

## Contas de teste (fictícias)
- **admin:** `admin@exemplo.com` / `Admin@2026` (ou o valor definido em `Seed__AdminSenha`)
- **cliente A:** `ana@exemplo.com` / `Cliente@2026` · **cliente B:** `bruno@exemplo.com` / `Cliente@2026`

## Banco de dados
- **`Usuario`:** `Id`, `Nome`, `Email` (único), `SenhaHash` (BCrypt), `Papel` (`admin` ou `cliente`), `CriadoEm` (UTC).
- **`Peca` (Entidade Principal):** `Id`, `Nome`, `Descricao`, `Tamanho`, `Categoria`, `Condicao`, `Preco`, `Vendida`, `Ativa`, `CriadaEm` (UTC). O construtor valida campos obrigatórios, preço positivo (`> 0`) e `DateTimeKind.Utc`.
- **`Venda` (Segunda Entidade — 1 para N com `Peca` e com `Usuario`):** `Id`, `PecaId`, `UsuarioId`, `DataVenda` (UTC), `FormaPagamento`, `Observacoes`, `Situacao` (`Concluida` / `Cancelada`).
- **Onde está a regra do 409:** em `Brecho.Api/Servicos/VendaServico.cs` (método `CriarAsync`, dentro de `await _db.Database.BeginTransactionAsync()`) e no domínio `Peca.MarcarComoVendida()`.

## Como rodar no computador
Defina as variáveis de ambiente no terminal (sem gravar segredos em arquivos):
```bash
export ConnectionStrings__Padrao="Host=...;Database=...;Username=...;Password=...;SSL Mode=VerifyFull;Channel Binding=Require"
export Jwt__Segredo="uma-chave-longa-e-aleatoria-com-32-caracteres-ou-mais"
export Seed__AdminSenha="Admin@2026"

# Aplicar migrations / rodar a API:
dotnet ef database update --project Brecho.Api
dotnet run --project Brecho.Api

# Compilar sem avisos e rodar os 15 testes automatizados xUnit:
dotnet build -warnaserror
dotnet test
```

## Front-end
Localizado na pasta `front/` (React 19 + TypeScript + Vite + React Router 8.4.0, CSS próprio com variáveis e suporte a modo claro/escuro):
- **`/` (`Vitrine.tsx`):** vitrine pública paginada sincronizada com `?busca=` e `?pagina=` na URL, esqueleto animado de carregamento e estado vazio com botão de limpar busca.
- **`/pecas/:id` (`DetalhePeca.tsx`):** detalhes da peça ou tela amigável de 404.
- **`/login` e `/cadastro`:** autenticação JWT e registro de novos clientes com medidor de força de senha e exibição de erros 400/401/409 vindos do `ProblemDetails`.
- **`/vendas` (`Vendas.tsx`):** área logada que lista as compras do cliente e permite registrar uma nova compra (`POST /api/vendas` com `Authorization: Bearer <token>`), exibindo na tela o erro `409 Conflict` caso a peça já tenha sido vendida, além de cancelamento com confirmação em `<dialog>` nativo.
- **`/admin/pecas` (`AdminPecas.tsx`):** painel exclusivo para o papel `admin` para criar, editar (`?editar=ID`) e excluir peças.
- **CORS na API:** configurado via variável `Cors__Origens` (padrão `http://localhost:5173` no desenvolvimento e a URL do Static Site `https://brecho-front.onrender.com` na Render).

## Como usei a IA
- **IA usada:** Google Gemini (Antigravity).
- **Os 3 pedidos (prompts) que mais ajudaram:**
  1. Prompt inicial do Passo 6 especificando o nicho *Brechó ou loja de roupas*, a relação 1:N entre `Peca` e `Venda`, a regra do `409 Conflict` para peça única já vendida e os 15 fatos técnicos da stack (.NET 10, EF Core 10, Npgsql, Swashbuckle 10.2.3).
  2. Pedido para construção da camada de serviços (`VendaServico` e `PecaServico`) com `Resultado<T>`, transação explícita no registro da venda, checagem de dono ou admin contra IDOR (`403 Forbidden`) e suíte de 15 testes xUnit com SQLite em memória.
  3. Prompt do front-end em React + Vite consumindo o contrato da API, respeitando `erasableSyntaxOnly` no TypeScript e tratando respostas `ProblemDetails`.
- **Um erro da IA que percebi e como corrigi (arquivo, trecho e o que mudou):**
  - **Arquivo:** `Brecho.Api/Program.cs` (e configuração do Swagger/OpenAPI).
  - **Trecho e problema:** Ao criar o projeto com `dotnet new webapi`, o `Program.cs` vinha com `builder.Services.AddOpenApi();` e `app.MapOpenApi();` do pacote `Microsoft.AspNetCore.OpenApi`, que havia sido removido para usar o `Swashbuckle.AspNetCore` 10.2.3, gerando o erro `CS1061: 'IServiceCollection' não contém uma definição para 'AddOpenApi'`. Além disso, na versão 10.2.3 do Swashbuckle (que usa `Microsoft.OpenApi` 2.x), usar o namespace antigo `Microsoft.OpenApi.Models` ou `new OpenApiReference` não compila.
  - **Como corrigi:** Substituí `AddOpenApi()`/`MapOpenApi()` por `AddEndpointsApiExplorer()`, `AddSwaggerGen()` com `using Microsoft.OpenApi;` e `new OpenApiSecuritySchemeReference("Bearer", doc)` dentro de `AddSecurityRequirement`, deixando `UseSwagger()` e `UseSwaggerUI()` habilitados também em produção.
- **O prompt usado para gerar o front:**
  - Foi utilizado o prompt do Passo 6 para o front-end passando o contrato das rotas `/api/pecas`, `/api/vendas` e `/api/auth`, solicitando `src/api.ts`, `src/sessao.tsx`, vitrine com busca e paginação na URL, login, cadastro, detalhe, página logada de vendas com `<dialog>` nativo e painel admin com tema claro/escuro.
- **O que aprendi:**
  - Aprendi a diferença prática entre erros de validação (`400 Bad Request`), falta de autenticação (`401`), falta de permissão/IDOR (`403 Forbidden` conferindo o `ClaimTypes.NameIdentifier` do token antes de alterar ou cancelar uma venda), recurso inexistente (`404`) e conflito de estado do domínio (`409 Conflict` garantindo em transação que uma peça única de brechó não seja vendida duas vezes). Também aprendi a importância de padronizar datas em UTC para o PostgreSQL (`timestamp with time zone`) e manter `ToLower().Contains()` para compatibilidade entre PostgreSQL em produção e SQLite em memória nos testes.
