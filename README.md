# FinData

MVP web de controle financeiro pessoal para TCC, com ASP.NET Core .NET 10, React + TypeScript + Vite e PostgreSQL 17 em Docker.

## Escopo implementado

- Cadastro e login com ASP.NET Core Identity e tokens Bearer.
- Cadastro e listagem de categorias por usuário.
- Criação, consulta mensal, edição e exclusão de receitas/despesas.
- Dashboard com receitas, despesas, saldo e despesas por categoria.
- Validações no domínio e isolamento dos dados pela conta autenticada.
- EF Core, migrations, Swagger/OpenAPI e script de testes HTTP.
- Frontend com login, cadastro, filtro de período e formulários.

## Arquitetura

Uma única API com DDD e CQRS simplificados: comandos e consultas separados no código, compartilhando o mesmo PostgreSQL. Não há mensageria ou event sourcing.

```text
Controllers/                     Rotas HTTP, autorização e respostas
Application/Commands/            Operações que alteram dados e autenticação
Application/Queries/             Listagens e agregações para o dashboard
Application/Interfaces/          Contrato do usuário autenticado
Domain/Entities/                 Category, Transaction e regras de domínio
Domain/Interfaces/               Contratos dos repositórios
Infrastructure/Identity/         Identity e leitura das claims
Infrastructure/OpenApi/          Documentação da autenticação
Infrastructure/Persistence/      DbContext, mapeamentos e migrations
Infrastructure/Repositories/     Persistência de comandos
frontend/                        Aplicação React, compilada separadamente
```

Escrita: controller → handler → entidade/repositório → EF Core → PostgreSQL. Consultas usam handlers com projeções/agregações via EF Core e retornam DTOs. Regras de domínio ficam nas entidades; handlers são injetados diretamente. Consultas e autenticação dependem da infraestrutura: a organização é simplificada em um projeto, sem isolamento rígido entre assemblies.

Referência: [CQRS e DDD simplificados — Microsoft Learn](https://learn.microsoft.com/pt-br/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/apply-simplified-microservice-cqrs-ddd-patterns). O FinData adapta a separação de comandos e consultas ao escopo acadêmico.

## Pré-requisitos

SDK .NET 10; Node.js 24 LTS com npm; Docker Desktop em execução com contêineres Linux e Compose; PowerShell 7 para os testes. Os comandos abaixo partem da pasta de `FinData.csproj`.

## Configuração inicial

1. Se ainda não existir `.env`, copie `.env.example` e substitua a senha de exemplo por uma senha local. Preserve qualquer configuração existente; não versione `.env`.

```powershell
Copy-Item .env.example .env
```

2. Inicie o banco e confira se o serviço `findata-db` está saudável:

```powershell
docker compose up -d
docker compose ps
```

Banco `findata`, usuário `findata_user`, porta local `127.0.0.1:5433`. Os dados ficam no volume `findata_postgres_data`. Alterar a senha em `.env` não muda a senha de um banco já inicializado. Preserve o volume; não use `docker compose down -v` para solucionar conexão.

3. Configure `ConnectionStrings:FinData` no Secret Manager local do .NET. O Compose lê `.env`, mas a API não carrega esse arquivo automaticamente. Use a mesma senha do banco neste modelo:

```text
Host=localhost;Port=5433;Database=findata;Username=findata_user;Password=SUA_SENHA_LOCAL
```

Para informar a conexão sem escrevê-la no histórico do terminal:

```powershell
$finDataConnection = Read-Host 'Cole a conexão completa do PostgreSQL' -MaskInput
dotnet user-secrets set 'ConnectionStrings:FinData' $finDataConnection
Remove-Variable finDataConnection
```

Secret Manager é para desenvolvimento, sem criptografia de cofre. Não publique credenciais. Em outros ambientes, configure `ConnectionStrings__FinData` pelo mecanismo de segredos da hospedagem.

4. Restaure ferramentas e aplique as migrations existentes, sem criar outra migration inicial:

```powershell
dotnet restore
dotnet tool restore
dotnet ef database update -- --environment Development
```

O manifesto `dotnet-tools.json` fixa a ferramenta local. Um aviso de patch anterior ao runtime não equivale a falha; confira se o comando termina com `Done.`.

## Executar

Terminal 1, na raiz:

```powershell
dotnet run --launch-profile http
```

API: `http://localhost:5068`; Swagger: `http://localhost:5068/swagger`; OpenAPI: `http://localhost:5068/openapi/v1.json`. A documentação é habilitada apenas em Development.

Terminal 2:

```powershell
cd frontend
npm ci
npm run dev
```

Abra `http://localhost:5173`. O Vite encaminha `/api` para a API na porta 5068. Mantenha os dois terminais e o banco ativos. Abrir uma rota protegida da API diretamente no navegador pode retornar 401.

## Usar a aplicação

1. Clique em **Criar conta**, informe e-mail e senha com pelo menos oito caracteres, maiúscula, minúscula e número.
2. Entre e cadastre uma categoria.
3. Em **Novo lançamento**, registre receita ou despesa. Valores são positivos e têm até duas casas decimais; o tipo define entrada ou saída.
4. Escolha o mês e clique em **Atualizar**. Saldo = receitas − despesas.
5. Use **Editar** ou **Excluir** na tabela. A exclusão pede confirmação e é permanente.

Salvar em outro mês muda a tela para esse período. A API identifica o usuário pelo token; os formulários não enviam `UserId`. Categorias de outra conta não podem ser usadas.

## Rotas principais

| Método | Rota | Finalidade |
| --- | --- | --- |
| POST | `/api/auth/register` | Cadastro |
| POST | `/api/auth/login` | Login e tokens |
| GET | `/api/auth/me` | Identificar usuário |
| POST / GET | `/api/categories` | Criar / listar categorias |
| POST | `/api/transactions` | Criar lançamento |
| GET | `/api/transactions?year=2026&month=11` | Listar mês |
| PUT / DELETE | `/api/transactions/{id}` | Editar / excluir |
| GET | `/api/dashboard/summary?year=2026&month=11` | Resumo mensal |
| GET | `/api/dashboard/expenses-by-category?year=2026&month=11` | Despesas por categoria |

Somente cadastro e login são públicos. Nas demais rotas envie `Authorization: Bearer <accessToken>`. O token do Identity é opaco, não JWT. No Swagger, faça login e use `accessToken`, sem aspas, em **Authorize**. Se o campo solicitar o cabeçalho completo, informe `Bearer <accessToken>`; confira no cURL que existe apenas um prefixo `Bearer`.

## Compilação e testes

Na raiz:

```powershell
dotnet build
```

Dentro de `frontend`:

```powershell
npm run lint
npm run build
```

Para testar a API, cadastre duas contas distintas. Mantenha API e banco ativos e execute na raiz, substituindo os e-mails:

```powershell
pwsh -File .\Test-FinData.ps1 -EmailA 'conta-a@exemplo.com' -EmailB 'conta-b@exemplo.com'
```

O script pede senhas com entrada oculta e verifica autenticação, isolamento, categorias, CRUD de transações, valores inválidos, resumo e acesso anônimo. Cria lançamentos temporários e tenta removê-los no final; mantém categorias para reutilização. Execute somente no banco local de desenvolvimento. A quantidade de verificações varia se precisar criar categorias. Não há suíte xUnit configurada; `dotnet test` não substitui o script.

Roteiro manual complementar: cadastrar conta nova; entrar; criar categoria; registrar receita e despesa; conferir totais; editar valor e data; cancelar exclusão; excluir lançamento descartável; sair; conferir com outra conta que os dados anteriores não aparecem.

## Problemas frequentes

- **Conexão recusada:** confira os terminais da API/Vite e `docker compose ps`. Subir só o banco não inicia a API.
- **401:** entre novamente e substitua a autorização no Swagger. No frontend, recarregar a página exige novo login.
- **400 em transações:** leia a mensagem e use uma categoria da conta atual. IDs de exemplo do Swagger não são registros existentes.
- **404 ao alterar/excluir:** registro ausente ou de outra conta. Excluir duas vezes pode retornar 204 e depois 404.
- **npm sem package.json:** execute dentro de `frontend`.
- **Porta 5173 ocupada:** encerre a instância anterior do Vite ou libere a porta; `strictPort` evita troca silenciosa.
- **Avisos npm:** analise `npm audit` antes de atualizar; evite `npm audit fix --force` sem avaliar mudanças de versão.

## Limites atuais

- Web primeiro; mobile fica para evolução futura.
- Categorias têm criação e listagem, sem edição/exclusão.
- Sessão do frontend apenas em memória, sem renovação automática. Sair remove a sessão local, sem revogar tokens emitidos.
- Sem recuperação de senha ou confirmação de e-mail.
- Registros antigos com `UserId` nulo permanecem no banco e ficam invisíveis nas consultas autenticadas. Atribuí-los exige decisão e migração de dados específica.
- O frontend rejeita valores acima da precisão segura de centavos do JavaScript; o backend usa `decimal` e PostgreSQL `numeric(18,2)`.
- O build gera `frontend/dist`; a API não serve esses arquivos. O proxy Vite é só de desenvolvimento. Publicação requer HTTPS e encaminhamento de `/api` na hospedagem.
