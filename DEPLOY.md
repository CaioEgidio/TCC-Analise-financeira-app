# Publicar FinData — Render Free + Neon Free

O Dockerfile compila o React e o backend e serve ambos no mesmo endereço. O PostgreSQL fica no Neon. Não é necessário executar Vite, Docker Desktop ou .NET no seu computador após a publicação.

## 1. Preparar o GitHub

Envie o código atual, incluindo Dockerfile, .dockerignore, render.yaml e Program.cs. Preserve package-lock.json e migrations. Não envie .env, senhas, tokens, node_modules, bin ou obj.

Se FinData.csproj estiver numa subpasta do repositório, ela deve ser a Root Directory do serviço no Render. O Dockerfile espera FinData.csproj e frontend/ no mesmo diretório de contexto.

## 2. Criar o banco Neon

1. Crie sua conta em https://neon.com e escolha o plano Free.
2. Crie um projeto e, quando disponível, escolha PostgreSQL 17. Prefira uma região próxima à do serviço Render.
3. Na opção Connect, obtenha os dados de conexão para .NET/Npgsql. Use o hostname direto (sem `-pooler`) para esta primeira publicação, pois a aplicação também aplicará migrations.
4. Monte a conexão no formato abaixo com os dados reais. Não use literalmente uma URL `postgresql://...` no lugar da conexão Npgsql:

```text
Host=HOST_DO_NEON;Port=5432;Database=BANCO_DO_NEON;Username=USUARIO_DO_NEON;Password=SENHA_DO_NEON;SSL Mode=VerifyFull
```

Não remova a verificação de certificado. Se a senha contiver caracteres especiais de connection string, utilize o formato de conexão Npgsql fornecido pela plataforma ou escape corretamente o valor.

Guarde essa conexão somente no campo secreto do Render, não em arquivos versionados nem no frontend. Não é necessário compartilhar a senha no chat.

O banco online começa separado do banco Docker local. Contas e categorias locais não serão copiadas automaticamente. Para a apresentação, cadastre uma nova conta online e insira dados fictícios pela interface.

## 3. Criar o serviço Render

Crie sua conta em https://render.com e conecte o repositório do GitHub. A autorização de acesso ao GitHub deve ser feita pelo dono da conta.

Opção Blueprint: New → Blueprint → selecione o repositório e confirme o serviço Free definido em render.yaml. Informe ConnectionStrings__FinData quando solicitado.

Opção manual: New → Web Service, selecione o repositório, linguagem/runtime Docker e instância Free. Configure:

| Configuração | Valor |
| --- | --- |
| Root Directory | Pasta que contém FinData.csproj; vazia se estiver na raiz |
| Dockerfile Path | ./Dockerfile, relativo à Root Directory |
| Docker Build Context | . |
| Health Check Path | /healthz |
| ASPNETCORE_ENVIRONMENT | Production |
| Hosting__HttpsTerminatedAtProxy | true |
| Database__ApplyMigrations | true |
| ConnectionStrings__FinData | Conexão secreta do Neon |

Não configure uma porta fixa no código: a variável PORT, quando fornecida, é respeitada. O processo escuta em 0.0.0.0. O HTTPS público é tratado pelo Render. Não habilite Hosting__HttpsTerminatedAtProxy fora de uma hospedagem que realmente forneça HTTPS e redirecionamento na borda.

O primeiro deploy executa as migrations existentes quando Database__ApplyMigrations=true. Este atalho é destinado à demonstração com uma instância e requer permissão de alteração de schema no banco. Não configura dados de exemplo nem usa EnsureCreated. Uma falha de conexão/migration interrompe a inicialização e precisa ser resolvida pelos logs. Em uma operação maior, separe migrations do processo web.

## 4. Conferir o endereço público

Após o serviço ficar Live, abra o link HTTPS gerado pelo Render. Verifique:

1. /healthz retorna status ok. Essa rota comprova que o processo está ativo; não testa o banco.
2. / abre a tela de login e os arquivos CSS/JS carregam.
3. Cadastre uma conta nova e faça login.
4. Crie uma categoria, uma receita e uma despesa. Confira o resumo.
5. Edite um lançamento e confirme os valores.
6. Teste a exclusão somente com um lançamento descartável.
7. Use outra conta para conferir isolamento.

O Swagger permanece desabilitado em Production. Uma rota inexistente em /api deve retornar 404; não deve retornar o HTML da aplicação.

## 5. Apresentação e limites gratuitos

- O Render Free pausa após inatividade e pode demorar a responder ao primeiro acesso. Abra o link com antecedência e teste login e dashboard antes de apresentar.
- Confira as cotas atuais do Neon Free e mantenha somente dados de demonstração.
- O filesystem da aplicação é efêmero. O banco fica no Neon. Chaves locais de proteção de tokens podem ser perdidas em reinícios/redeploys, invalidando tokens anteriores: faça login novamente. O frontend já guarda a sessão somente em memória.
- Não há cópia automática do banco local, backup configurado pelo projeto ou envio de senhas de teste à nuvem.
- Não escolha upgrades pagos ou forneça cartão sem revisar a oferta. Caso a plataforma exija pagamento, pare e revise a opção.

## Verificação local opcional da imagem

Com Docker Desktop disponível, na pasta de FinData.csproj:

```powershell
docker build -t findata-demo .
```

O teste completo do container exige fornecer a conexão do banco por variável de ambiente em execução. A conexão e a senha nunca devem ser passadas como argumento de build ou copiadas para a imagem.

## Referências

- https://render.com/docs/free
- https://render.com/docs/tls
- https://render.com/docs/docker
- https://neon.com/pricing
- https://neon.com/docs/guides/dotnet-npgsql
