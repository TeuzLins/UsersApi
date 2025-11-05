# UsersApi (ASP.NET Core Web API com JWT e Roles)

Projeto de portfólio profissional: autenticação JWT, autorização por roles, ASP.NET Core Identity e persistência em SQLite.

## Requisitos
- .NET SDK 9+
- SQLite (embutido via `Data Source=users.db`)

## Configuração
1. Ajuste a chave JWT em `appsettings.json` (`Jwt:Key`). Use uma chave longa e aleatória.
2. (Opcional) Defina variáveis de ambiente para seed de admin:
   - `ADMIN_USER` (ex.: `admin`)
   - `ADMIN_PASS` (ex.: `Adm1n!234`)

## Executar
```powershell
cd UsersApi
dotnet run
```

Durante o desenvolvimento, o OpenAPI estará disponível (`/openapi/v1.json`).

## Endpoints principais
- `POST /api/auth/register` — cadastra usuário. Body:
```json
{ "username": "alice", "password": "Pass!234", "email": "alice@exemplo.com", "role": "User" }
```
- `POST /api/auth/login` — retorna `{ token }`.
- `POST /api/auth/assign-role?username={u}&role={r}` — requer role `Admin`.
- `GET /api/users/me` — requer autenticação.
- `GET /api/users/admin-only` — requer role `Admin`.

## Autenticação
Inclua o token JWT no header:
```
Authorization: Bearer {token}
```

## Migrações EF Core
Já geradas e aplicadas. Para atualizar manualmente:
```powershell
dotnet ef migrations add <Nome> -p UsersApi -s UsersApi
dotnet ef database update -p UsersApi -s UsersApi
```

## Logs
Serilog configurado para Console e arquivo em `logs/log-<data>.txt`.

## Próximos passos
- Adicionar testes (`xUnit`) para Auth e Users.
- Documentar com Swagger UI completo.
- Rate limit, refresh tokens e 2FA.