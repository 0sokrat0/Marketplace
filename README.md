# Marketplace

Учебный маркетплейс: база данных в третьей нормальной форме, REST API на ASP.NET Core
и десктопное приложение на WPF.

## Состав

| Проект | Назначение |
|---|---|
| `Marketplace.Contracts` | общие DTO и правила заказа |
| `Marketplace.Api` | база данных, авторизация, бизнес-логика, REST API |
| `Marketplace.Client` | WPF-приложение |
| `Marketplace.Api.Tests` | интеграционные тесты API |

## Работа в команде

Каждая часть делается в своей ветке `feature/*` и вливается в `main` через merge.

## Запуск API

```bash
dotnet run --project Marketplace.Api
```

API слушает адрес из `launchSettings.json`. База `marketplace.db` создается и заполняется при первом запуске.

Тестовые учетные записи:

| Логин | Пароль | Роль |
|---|---|---|
| admin | admin123 | Admin |
| user | user123 | User |
