# Libify — API REST

Todos os recursos seguem o mesmo contrato CRUD (via `BaseController<TEntity, TDto>`),
na rota `api/[controller]`.

## Operações padrão (por recurso)

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/{recurso}` | Lista todos (exclui soft-deleted) |
| `GET` | `/api/{recurso}/{id}` | Busca por Id |
| `POST` | `/api/{recurso}` | Cria |
| `PUT` | `/api/{recurso}/{id}` | Atualiza |
| `DELETE` | `/api/{recurso}/{id}` | Exclusão lógica (soft delete) |

## Recursos disponíveis

| Recurso | Rota base | DTO |
|---------|-----------|-----|
| Usuário | `/api/usuario` | `UsuarioDto` |
| Cliente | `/api/cliente` | `ClienteDto` |
| Serviço | `/api/servico` | `ServicoDto` |
| Proposta | `/api/proposta` | `PropostaDto` |
| Contrato | `/api/contrato` | `ContratoDto` |
| Cobrança | `/api/cobranca` | `CobrancaDto` |
| Lançamento financeiro | `/api/lancamentofinanceiro` | `LancamentoFinanceiroDto` |
| Nota fiscal | `/api/notafiscal` | `NotaFiscalDto` |
| Evento | `/api/evento` | `EventoDto` |
| Tarefa | `/api/tarefa` | `TarefaDto` |
| Post | `/api/post` | `PostDto` |
| Plano | `/api/plano` | `PlanoDto` |

## Autenticação

JWT Bearer. Configure `Jwt:Key`, `Jwt:Issuer` e `Jwt:Audience` em `appsettings`.

## Documentação interativa

Em ambiente de desenvolvimento, o **Swagger** fica disponível em `/swagger`.

## Como executar

```bash
dotnet restore
dotnet build
dotnet run --project Libify.API
```
