## Limpar, restaurar e compilar
- cd backend/src
- dotnet clean  ./InvestTracker.Api/InvestTracker.Api.csproj
- dotnet restore ./InvestTracker.Api/InvestTracker.Api.csproj -v minimal
- dotnet build   ./InvestTracker.Api/InvestTracker.Api.csproj -v minimal


## Limpar, Bancos
Mensagens para processa no Worker
docker compose exec postgres psql -U app -d investwrite -c \
'DELETE FROM outbox_messages WHERE "ProcessedAtUtc" IS NULL;'

Confirmar
docker compose exec postgres psql -U app -d investwrite -c \
'SELECT COUNT(*) AS pending FROM outbox_messages WHERE "ProcessedAtUtc" IS NULL;'

Deletar tudo no outbox
docker compose exec postgres psql -U app -d investwrite -c \
'DELETE FROM outbox_messages;'


## build
- cd backend/src
- dotnet build ./InvestTracker.Api/InvestTracker.Api.csproj -v minimal

## Subir bancos
- cd backend/docker
- docker compose up -d

## Rodar API (terminal separado)
- cd backend/src/InvestTracker.Api
- export ASPNETCORE_ENVIRONMENT=Development

(se ainda não tiver) instalar a CLI do EF
- dotnet tool install --global dotnet-ef || true
- export PATH="$HOME/.dotnet/tools:$PATH"

aplicar migrações no Postgres
dotnet ef database update \
  --project "../InvestTracker.Infrastructure/InvestTracker.Infrastructure.csproj" \
  --startup-project "./InvestTracker.Api.csproj" \
  --context "InvestTracker.Infrastructure.Persistence.AppWriteDbContext"

liberar a porta, se ocupada
- sudo fuser -k 5187/tcp || true

# rodar API
- dotnet run --no-launch-profile --urls "http://0.0.0.0:5187"

Alternative 
- DOTNET_ENVIRONMENT=Development dotnet run --urls http://localhost:5187


## Rodar Worker de Projeções (terminal separado)

cd backend/src/InvestTracker.Projections
dotnet run

## Frontend (terminal separado)
cd frontend

apontar o front para a API
echo 'VITE_API_URL=http://localhost:5187' > .env.local

npm i
npm run dev -- --host

# Observações
Na primeira execução, o EF cria/migra o banco e **semeia** um usuário:
- Email: `demo@local`
- Senha: `demo123`

> Ajuste o segredo JWT em `appsettings.json` (chave `Jwt:Key`).

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

## Extras
# Usuário e o database do postgres do container
- POSTGRES_PASSWORD=app
- POSTGRES_USER=app
- POSTGRES_DB=investwrite
