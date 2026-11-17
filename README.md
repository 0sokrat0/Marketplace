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

## Документация

- [База данных и схема](docs/database.md)
- [REST API](docs/api.md)

## Тесты

```bash
dotnet test Marketplace.Api.Tests
```

## Работа в команде

Каждая часть делается в своей ветке `feature/*` и вливается в `main` через merge.

## Запуск API

```bash
dotnet run --project Marketplace.Api
```

API слушает адрес из `launchSettings.json`. База `marketplace.db` создается и заполняется при первом запуске.
Swagger: `/swagger`.

Тестовые учетные записи:

| Логин | Пароль | Роль |
|---|---|---|
| admin | admin123 | Администратор |
| user | user123 | Пользователь |

## Быстрый запуск через Docker

```bash
make up      # собрать образ и запустить API на http://localhost:5081
make logs    # журнал API
make down    # остановить
make clean   # остановить и стереть базу
```

Swagger: `http://localhost:5081/swagger`. База лежит в томе `marketplace-data`. Без Docker: `make run` и `make test`.

## Запуск клиента

WPF-приложение запускается только на Windows. Сначала запустите API, затем:

```bash
dotnet run --project Marketplace.Client
```

Адрес API по умолчанию `http://localhost:5081/`, его можно задать переменной окружения `MARKETPLACE_API`.

Устройство клиента:

- паттерн MVVM на `CommunityToolkit.Mvvm`, зависимости собирает `Microsoft.Extensions.DependencyInjection`;
- `Services/ApiClient.cs` единственное место, где клиент обращается к API, ошибки сервера приходят как `ApiException`;
- левое меню зависит от роли: гость видит каталог, пользователь корзину, избранное и заказы, администратор разделы
  управления товарами, категориями, пунктами выдачи, заказами и отзывами;
- изображения товаров лежат в `Marketplace.Client/Assets/Images`, в базе хранится только имя файла, для неизвестного
  имени показывается `placeholder.png`.
