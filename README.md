# Libify

Backend em **.NET 10** para o aplicativo **Libify** — gestão para prestadores de serviço, profissionais liberais e MEIs (propostas, contratos, financeiro/Asaas, notas fiscais, agenda, tarefas, portfólio).

A solution segue arquitetura em camadas (mesmo padrão da solution IEBT/Hatlas).

## Estrutura

```
Libify.sln
├── Libify.Domain          # Entidades (Model/Base), Enums, Helpers
├── Libify.Infraestructure # AppDbContext (PostgreSQL/Npgsql), Factory, integração Asaas
├── Libify.Repository      # BaseRepository<T> + IBaseRepository<T>
├── Libify.Services        # BaseServices<T> + IBaseServices<T>
├── Libify.API             # Controllers, DTOs, AutoMapper, Middleware, Program.cs
└── Libify.Test            # xUnit + EF Core InMemory
```

## Domínio

`Usuario` (prestador / raiz do tenant), `Cliente`, `Servico`, `Proposta` + `PropostaItem`,
`Contrato`, `Cobranca`, `LancamentoFinanceiro`, `NotaFiscal`, `Evento`, `Tarefa`, `Post`, `Plano`.

## Integrações previstas

- **Asaas** (white-label): subconta por prestador, PIX / Boleto / Cartão, link de pagamento.
- **Google** (Drive, Calendar, Tasks), **Gemini** (OCR / geração de conteúdo), **WhatsApp Cloud API**, **Instagram Graph API**, **NFS-e**.
- **i18n / multi-país** via campos `Locale` e `Pais` no `Usuario`.

## Como rodar

```bash
dotnet restore
dotnet build
dotnet run --project Libify.API
```

Configure a connection string PostgreSQL e as chaves de integração em `Libify.API/appsettings.json`.

Swagger disponível em `/swagger` no ambiente de desenvolvimento.
