# Libify — Arquitetura

Backend em **.NET 10** do aplicativo **Libify**, voltado a prestadores de serviço,
profissionais liberais e MEIs (propostas, contratos, financeiro/Asaas, notas fiscais,
agenda, tarefas e portfólio).

A solution segue **arquitetura em camadas** (mesmo padrão da solution IEBT/Hatlas).

## Camadas

```
Libify.sln
├── Libify.Domain          # Entidades, Enums, Helpers (somente DataAnnotations, sem dependências externas)
├── Libify.Infraestructure # AppDbContext (PostgreSQL/Npgsql), Factory de design-time, integração Asaas, Migrations
├── Libify.Repository      # BaseRepository<T> + IBaseRepository<T> (acesso a dados genérico)
├── Libify.Services        # BaseServices<T> + IBaseServices<T> (regras de negócio)
├── Libify.API             # Controllers, DTOs, AutoMapper, Middleware, Program.cs
└── Libify.Test            # xUnit + EF Core InMemory
```

### Dependências entre projetos

```
API ──> Services ──> Repository ──> Infraestructure ──> Domain
 └────> Infraestructure ─────────────────────────────────┘
```

- `Domain` não referencia nenhum pacote externo (apenas `System.ComponentModel.DataAnnotations`),
  ficando livre de acoplamento com EF Core.
- O **EF Core Design** está restrito ao `Infraestructure` (com `PrivateAssets`), não vazando
  para as demais camadas nem para o pacote publicado.

## Padrões aplicados

| Padrão | Onde | Motivo |
|--------|------|--------|
| Repository genérico | `BaseRepository<T>` | CRUD único para todas as entidades |
| Service genérico | `BaseServices<T>` | Regra de criação/atualização (datas) centralizada |
| Controller genérico | `BaseController<TEntity, TDto>` | Evita duplicação do CRUD em cada recurso |
| DTO + AutoMapper | `Libify.API/Dto` + `MappingProfiles` | Separa contrato da API do modelo de domínio |
| Soft delete | `ModelBase.DeletedAt` + `SoftDeleteAsync` | Exclusão lógica padrão |
| Middleware global | `GlobalExceptionHandlerMiddleware` | Tratamento único de exceções |

## Modelo de domínio

Todas as entidades herdam de `ModelBase` (`Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`).

| Entidade | Descrição |
|----------|-----------|
| `Usuario` | Prestador / profissional liberal / MEI. Raiz do isolamento de dados (tenant). |
| `Cliente` | Pagador atendido pelo prestador. |
| `Servico` | Item do catálogo de serviços (preço padrão). |
| `Proposta` / `PropostaItem` | Proposta comercial e seus itens (rascunho/enviada/aceita/recusada). |
| `Contrato` | Gerado de uma proposta aceita, com aceite/assinatura digital. |
| `Cobranca` | Cobrança no Asaas (PIX/Boleto/Cartão), com parcelas. |
| `LancamentoFinanceiro` | Contas a pagar/receber (despesa pode vir de OCR de comprovante). |
| `NotaFiscal` | NFS-e (NFS-e Nacional / fallback eNotas). |
| `Evento` | Compromisso de agenda (Google Calendar / Meet). |
| `Tarefa` | Tarefa / follow-up (Google Tasks / Keep). |
| `Post` | Portfólio antes/depois com legenda (Gemini) e publicação no Instagram. |
| `Plano` | Assinatura Premium (mensal/semestral/anual) recorrente via Asaas. |

### Multi-tenancy

As entidades de dados possuem `UsuarioId`, isolando os dados por prestador. O `Usuario`
também guarda `Locale` e `Pais`, base para a expansão internacional (i18n / países hispano-falantes).

## Integrações previstas

| Integração | Camada / Componente | Status |
|------------|--------------------|--------|
| **Asaas** (white-label) | `Infraestructure/Services/AsaasClient` (`IAsaasClient`) | Esqueleto (subconta por prestador, `access_token` isolado) |
| **Google** (Drive, Calendar, Tasks) | Campos de Id nas entidades | Planejado |
| **Gemini** (OCR / geração de conteúdo) | Config `Gemini` | Planejado |
| **WhatsApp Cloud API** | Config `WhatsApp` | Planejado |
| **Instagram Graph API** | Entidade `Post` | Planejado |
| **NFS-e** | Entidade `NotaFiscal` | Planejado |

## Segurança

- Autenticação **JWT** (`Jwt` em `appsettings`).
- Headers de segurança (`X-Content-Type-Options`, `X-Frame-Options`, `Strict-Transport-Security`).
- Queries via **EF Core** (parametrizadas — sem concatenação de SQL).
- Isolamento de dados por `UsuarioId` (tenant).
- Credenciais **fora do repositório** (ver `BancoDeDados.md`).
- Dependências sem vulnerabilidades conhecidas (`dotnet list package --vulnerable`).
