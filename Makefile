.PHONY: up down logs build run test clean

# API в контейнере: http://localhost:5081/swagger
up:
	docker compose up --build -d

down:
	docker compose down

logs:
	docker compose logs -f api

build:
	dotnet build Marketplace.sln

# API без Docker
run:
	dotnet run --project Marketplace.Api

test:
	dotnet test Marketplace.Api.Tests

# остановить контейнер и стереть базу
clean:
	docker compose down -v
