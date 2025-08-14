## Limpar, restaurar e compilar
- ~/invest-tracker/backend/src
- dotnet clean  ./InvestTracker.Api/InvestTracker.Api.csproj
- dotnet restore ./InvestTracker.Api/InvestTracker.Api.csproj -v minimal
- dotnet build   ./InvestTracker.Api/InvestTracker.Api.csproj -v minimal

## build
- cd ~/Documentos/GitHub/invest-tracker/backend/src
- dotnet build ./InvestTracker.Api/InvestTracker.Api.csproj -v minimal

## Subir bancos
- cd backend/docker
- docker compose up -d

## Rodar API (terminal separado)
- cd invest-tracker/backend/src/InvestTracker.Api
- export ASPNETCORE_ENVIRONMENT=Development

# (se ainda não tiver) instalar a CLI do EF
- dotnet tool install --global dotnet-ef || true
- export PATH="$HOME/.dotnet/tools:$PATH"

# aplicar migrações no Postgres
dotnet ef database update \
  --project "../InvestTracker.Infrastructure/InvestTracker.Infrastructure.csproj" \
  --startup-project "./InvestTracker.Api.csproj" \
  --context "InvestTracker.Infrastructure.Persistence.AppWriteDbContext"

# liberar a porta, se ocupada
- sudo fuser -k 5187/tcp || true

# rodar API
- dotnet run --no-launch-profile --urls "http://0.0.0.0:5187"


Na primeira execução, o EF cria/migra o banco e **semeia** um usuário:
- Email: `demo@local`
- Senha: `demo123`

> Ajuste o segredo JWT em `appsettings.json` (chave `Jwt:Key`).

## Rodar Worker de Projeções

cd backend/src/InvestTracker.Projections
dotnet run

## Frontend
cd invest-tracker/frontend

# apontar o front para a API
echo 'VITE_API_URL=http://localhost:5187' > .env.local

npm i
npm run dev -- --host


## Fluxo de teste
1. Acesse `http://localhost:5173/login` e entre com `demo@local` / `demo123`.
2. Vá para **Investments**, crie alguns lançamentos.
3. O Worker irá projetar os eventos para o **MongoDB** (soma mensal e por tipo).
4. Abra **Dashboard** para ver os gráficos.

## Endpoints principais
- `POST /api/auth/login` → { email, password } ⇒ { token }
- `POST /api/investments` (autenticado) → cria investimento
- `GET /api/investments?page=1&size=50` (autenticado) → lista (Dapper/Postgres)
- `GET /api/dashboard?from=2025-01-01&to=2025-12-31` (autenticado) → série temporal (Mongo)

## Observações
- O exemplo cobre **Create** e **List**. Para **Update/Delete**, você pode criar novos comandos/handlers e publicar eventos de correção na Outbox (ex.: `InvestmentUpdatedV1`, `InvestmentDeletedV1`) e, no Worker, ajustar o agregado mensal (revertendo valor antigo e aplicando novo).
- Em produção, considere:
  - Migrations dedicadas e CI/CD.
  - Outbox com bloqueio/concorrência robusta e política de reprocessamento.
  - Observabilidade (OpenTelemetry).
  - Rate limiting, CORS, versionamento de API, validação (FluentValidation).

