# UsersApi — ASP.NET Core Web API com JWT e Roles

API para autenticação e autorização baseada em JWT com ASP.NET Core Identity (roles), persistência em SQLite e observabilidade com Serilog. O projeto inclui testes unitários e de integração com xUnit e `WebApplicationFactory`.

## Visão Geral
- Autenticação por `JWT Bearer` com claims padrão e expiração configurável.
- Autorização baseada em roles (`User`, `Admin`) via `AuthorizeAttribute`.
- Persistência com EF Core (SQLite) e schema do Identity pré-migrado.
- Logs em console e arquivo com Serilog (rotação diária).
- Testes: unitários para geração de token e integração cobrindo fluxo de registro, login, autorização e endpoints protegidos.

## Stack Técnica
- `ASP.NET Core 9` (Minimal Hosting Model).
- `Entity Framework Core` + `ASP.NET Core Identity`.
- `JWT` (`System.IdentityModel.Tokens.Jwt`).
- `SQLite` (arquivo `users.db`).
- `Serilog` (console e arquivo).
- `xUnit` + `Microsoft.AspNetCore.Mvc.Testing` para integração.

## Requisitos
- `DOTNET SDK` 9.0+
- Windows/macOS/Linux com acesso a filesystem (SQLite embarcado).

## Estrutura do Projeto
```
Controllers/            Endpoints de Auth e Users
Data/                   DbContext (Identity)
Migrations/             Migrações EF Core (SQLite)
Models/                 DTOs de entrada
Services/               Serviços (JwtTokenService)
Properties/             launchSettings.json
Program.cs              Bootstrap da aplicação
appsettings.json        Configuração padrão
UsersApi.Tests/         Testes unitários e integração
logs/                   Arquivos de log
users.db                Banco SQLite (dev)
ProfessionalPortfolio.sln  Solution
UsersApi.csproj         Projeto da API
```

## Configuração
- Edite `appsettings.json`:
  - `ConnectionStrings:DefaultConnection`: `Data Source=users.db` (padrão).
  - `Jwt:Key`: defina uma chave longa e aleatória (mín. 32 bytes).
  - `Jwt:Issuer` e `Jwt:Audience`: identidades do emissor e público.
- Variáveis de ambiente (opcional) para seed de admin:
  - `ADMIN_USER` (ex.: `admin`)
  - `ADMIN_PASS` (ex.: `Adm1n!234`)

## Execução (Desenvolvimento)
```powershell
dotnet run --project .\UsersApi.csproj
```
- OpenAPI JSON: `GET /openapi/v1.json`.
- Logs: `logs/log-<data>.txt`.

## Endpoints
- `POST /api/auth/register` — registra usuário.
  - Exemplo:
    ```bash
    curl -X POST http://localhost:5000/api/auth/register \
      -H "Content-Type: application/json" \
      -d '{"username":"alice","password":"Pass!234","email":"alice@exemplo.com","role":"User"}'
    ```
- `POST /api/auth/login` — retorna `{ token }`.
  - Exemplo:
    ```bash
    curl -X POST http://localhost:5000/api/auth/login \
      -H "Content-Type: application/json" \
      -d '{"username":"alice","password":"Pass!234"}'
    ```
- `POST /api/auth/assign-role?username={u}&role={r}` — requer `Admin`.
- `GET /api/users/me` — requer autenticação.
- `GET /api/users/admin-only` — requer role `Admin`.

## Autenticação
- Inclua o token no header:
  ```
  Authorization: Bearer {token}
  ```

## Migrações EF Core
- Migrações já incluídas (SQLite). Para criar/atualizar:
```powershell
dotnet ef migrations add <Nome> -p .\UsersApi.csproj -s .\UsersApi.csproj
dotnet ef database update -p .\UsersApi.csproj -s .\UsersApi.csproj
```

## Testes
- Executa unitários e integração:
```powershell
dotnet test ProfessionalPortfolio.sln -c Debug
```
- Integração usa `WebApplicationFactory<Program>` com SQLite em memória.
- Se houver erros de referência do xUnit, rode `dotnet restore` e `dotnet clean` antes de testar.

## Observabilidade
- Serilog escreve em console e arquivo:
  - `logs/log-.txt` com rotação diária.
- Ajuste nível em `Serilog.MinimumLevel` no `appsettings.json`.

## Segurança
- Mantenha `Jwt:Key` fora do VCS (use secrets/variáveis de ambiente).
- Prefira HTTPS em produção.
- Considere adicionar: expiração curta, refresh tokens, rate limiting e 2FA.

## Roadmap
- Swagger UI completo.
- Refresh tokens, rate limit, 2FA.
- Políticas de autorização granulares e auditoria de ações.

## Como contribuir
- Abra issues com contexto e passos para reproduzir.
- Submeta PRs com descrição clara e testes cobrindo mudanças.