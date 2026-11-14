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
