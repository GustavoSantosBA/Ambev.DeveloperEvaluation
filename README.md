# Developer Evaluation Project

---

## Descrição

API para gerenciamento de vendas, construída com .NET 8 seguindo os princípios da arquitetura DDD (Domain Driven Design) e do padrão CQRS (Command Query Responsibility Segregation). O projeto é modular, possui validação robusta e está pronto para ser executado localmente via Docker.

---

## Tecnologias Utilizadas

- .NET 8
- Docker
- PostgreSQL
- Entity Framework Core
- MediatR (CQRS & Mediator)
- AutoMapper
- FluentValidation

---

## Como Executar o Projeto

Para executar o projeto, você precisará ter o [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) e o [Docker](https://www.docker.com/products/docker-desktop) instalados.

### 1. Clone o Repositório

```bash
git clone https://github.com/seu-usuario/seu-projeto.git
cd seu-projeto
```

---

### 2. Inicie os Serviços com Docker Compose

Este comando irá construir a imagem da API, iniciar o contêiner do banco de dados PostgreSQL e os demais serviços definidos no `docker-compose.yml`.

```bash
docker-compose up -d
```

O banco de dados será iniciado e estará acessível para a API. As credenciais e a porta são gerenciadas pelo Docker Compose.

### 3. Verifique a Connection String

O projeto já está configurado para se conectar ao banco de dados Docker. A `Connection String` no arquivo `appsettings.Development.json` deve corresponder à configuração do `docker-compose.yml`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=DeveloperEvaluation;Username=devuser;Password=devpass"
  }
}
```

---

### 4. Aplique as Migrations (se necessário)

As migrations do Entity Framework devem ser aplicadas automaticamente na inicialização da aplicação. Caso precise aplicá-las manualmente, utilize o comando abaixo:

```bash
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

---

### 5. Rode a aplicação

```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

---
### 6. Acesse a API
### 5. Acesse a Documentação da API

A documentação da API (Swagger) estará disponível no seguinte endereço, conforme a porta definida no `docker-compose.yml`:

[http://localhost:8080/swagger](http://localhost:8080/swagger)

---

## Estrutura do Projeto

```
src/
  Ambev.DeveloperEvaluation.Domain         # Entidades e regras de negócio
  Ambev.DeveloperEvaluation.Application    # Commands, Handlers, Validators (CQRS)
  Ambev.DeveloperEvaluation.ORM            # EF Core: contexto, repositórios, migrations
  Ambev.DeveloperEvaluation.WebApi         # Controllers, configuração, requests/responses

tests/
  Ambev.DeveloperEvaluation.Tests          # Testes automatizados (unitários e integração)
```

---

## Convenções e melhores práticas

- Arquitetura em camadas: Domain → Application → Infrastructure → WebApi
- Commands/Queries seguindo padrão CQRS
- Validações centralizadas (Application + Domain)
- Gerenciamento de dependências com injeção + MediatR
- Projeto pronto para migração e deploy (Docker, CI/CD, etc.)

---

## Como customizar a connection string

Se precisar alterar o banco de dados, edite a propriedade `DefaultConnection` em `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=developerdb;Username=devuser;Password=devpass"
  }
}
```

---

## Arquitetura e Boas Práticas

- **Arquitetura Limpa:** Separação clara de responsabilidades entre as camadas (Domain, Application, Infrastructure, API).
- **CQRS:** Comandos (escrita) e Queries (leitura) são tratados por handlers distintos, promovendo a escalabilidade.
- **Validação:** As regras de validação são implementadas usando FluentValidation para garantir a consistência dos dados.
- **Injeção de Dependência:** O MediatR é utilizado para desacoplar os componentes e gerenciar o fluxo de controle.


