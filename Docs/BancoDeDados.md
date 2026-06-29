# Libify — Banco de Dados e Migrations

## Provedor

**PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL`.

- `AppDbContext` (em `Libify.Infraestructure/DataBase`) define os `DbSet` de todas as entidades.
- `AppDbContextFactory` fornece o contexto em **design-time** (usado pelo `dotnet ef`),
  lendo a connection string da variável de ambiente `PROFLIB_CONNECTION`
  (com fallback para `localhost`).

## Connection string

Ambiente do banco (RedeHost):

```
Host=pgsql01.redehost.com.br;Port=5432;Database=<nome_do_banco>;Username=libify-admin;Password=<senha>;
```

> **A senha NUNCA deve ser versionada.** O `appsettings.json` é commitado com `Password=` vazio.

### Onde definir a senha (sem commitar)

**Em desenvolvimento — User Secrets (recomendado):**

```bash
cd Libify.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=pgsql01.redehost.com.br;Port=5432;Database=<db>;Username=libify-admin;Password=<senha>;"
```

**Em homologação / produção — variável de ambiente (no servidor):**

```
ConnectionStrings__DefaultConnection = Host=...;Database=...;Username=libify-admin;Password=<senha>
Jwt__Key = <chave-forte>
Asaas__PlatformApiKey = <api-key>
```

**Para rodar migrations (design-time):**

```powershell
$env:PROFLIB_CONNECTION = "Host=pgsql01.redehost.com.br;Port=5432;Database=<db>;Username=libify-admin;Password=<senha>;"
```

## Ambientes (Development / Homologacao / Production)

A API carrega `appsettings.json` (base) + `appsettings.{ASPNETCORE_ENVIRONMENT}.json`:

| Ambiente     | Arquivo                          | Banco           | Asaas    | Swagger |
|--------------|----------------------------------|-----------------|----------|---------|
| Development  | `appsettings.Development.json`   | `libify_dev`    | sandbox  | sim     |
| Homologacao  | `appsettings.Homologacao.json`   | `libify_homolog`| sandbox  | sim     |
| Production   | `appsettings.Production.json`    | `libify_prod`   | produção | não     |

- Todos os arquivos são commitados **com `Password=` vazio** — a senha entra por User Secrets (dev) ou variável de ambiente (homolog/prod).
- Selecionar o ambiente: variável `ASPNETCORE_ENVIRONMENT` ou os perfis em `Properties/launchSettings.json` (`Development`, `Homologacao`, `Production`).
- Ajuste os nomes dos bancos (`libify_dev`/`libify_homolog`/`libify_prod`) conforme os que você criar na RedeHost.

## Acesso remoto (importante)

O PostgreSQL da RedeHost restringe conexões por IP. Se aparecer o erro:

```
28000: no pg_hba.conf entry for host "<seu_ip>", user "libify-admin", database "..."
```

é necessário **liberar o IP de origem** no painel da RedeHost (acesso remoto PostgreSQL).
Painel web (consulta): http://phppgadmin01.redehost.com.br

## Comandos de Migration

```powershell
# Criar uma nova migration
dotnet ef migrations add <Nome> --project Libify.Infraestructure --startup-project Libify.Infraestructure

# Aplicar as migrations no banco
$env:PROFLIB_CONNECTION = "Host=pgsql01.redehost.com.br;Port=5432;Database=<db>;Username=libify-admin;Password=<senha>;"
dotnet ef database update --project Libify.Infraestructure --startup-project Libify.Infraestructure

# Reverter a última migration (antes de aplicar)
dotnet ef migrations remove --project Libify.Infraestructure --startup-project Libify.Infraestructure
```

> O `Infraestructure` é usado como *startup project* porque contém a `AppDbContextFactory`
> (design-time), mantendo a `Libify.API` sem dependência de ferramentas de design.

### Migration existente

- `InitialCreate` — cria todas as tabelas do domínio.
