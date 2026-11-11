# REST API

Ошибки возвращаются в виде `{"error": "текст"}` с кодами 400, 401, 403, 404, 409. Авторизация: заголовок
`Authorization: Bearer <token>`, токен выдают `register` и `login`.

Доступ: **гость** без токена, **пользователь** с ролью User, **админ** с ролью Admin.

## Авторизация

| Метод | Адрес | Доступ | Описание |
|---|---|---|---|
| POST | `/api/auth/register` | гость | регистрация, повторная регистрация с тем же паролем выполняет вход |
| POST | `/api/auth/login` | гость | вход |

## Каталог

| Метод | Адрес | Доступ | Описание |
|---|---|---|---|
| GET | `/api/categories` | гость | категории |
| GET | `/api/products?search&categoryId&minPrice&maxPrice` | гость | поиск и фильтры |
| GET | `/api/products/{id}` | гость | товар |
| GET | `/api/products/{id}/reviews` | гость | отзывы |
| POST | `/api/products/{id}/reviews` | пользователь | оценка 1-5 и текст, один отзыв на пользователя |
| GET | `/api/pickup-points` | гость | пункты выдачи |

## Корзина и избранное

| Метод | Адрес | Описание |
|---|---|---|
| GET | `/api/cart` | корзина с итогом |
| POST | `/api/cart/items` | добавить товар |
| PUT | `/api/cart/items/{productId}` | изменить количество |
| DELETE | `/api/cart/items/{productId}` | удалить товар |
| GET | `/api/favorites` | избранное |
| POST | `/api/favorites/{productId}` | добавить |
| DELETE | `/api/favorites/{productId}` | убрать |
| POST | `/api/favorites/{productId}/to-cart` | перенести в корзину |

## Заказы

| Метод | Адрес | Описание |
|---|---|---|
| POST | `/api/orders` | оформить заказ |
| GET | `/api/orders?sort=new\|old` | история заказов |
| GET | `/api/orders/{id}` | состав заказа |

Тело `POST /api/orders`:

```json
{ "deliveryMethod": "Самовывоз", "pickupPointId": 1, "address": null,
  "buyNow": { "productId": 10, "quantity": 1 } }
```

Без `buyNow` заказ собирается из всей корзины. Правила:

- товар должен быть доступен и в наличии, количество не больше остатка;
- сумма заказа меньше 600 рублей: только самовывоз, для доставки адрес не нужен;
- сумма от 600 рублей: доставка по адресу или самовывоз;
- в позиции заказа сохраняется цена на момент покупки;
- остаток уменьшается при оформлении и возвращается при отмене заказа.

## Администратор

| Метод | Адрес | Описание |
|---|---|---|
| POST, PUT | `/api/admin/products`, `/api/admin/products/{id}` | добавить и изменить товар: цена, описание, категория, изображение, остаток, доступность |
| POST, PUT | `/api/admin/categories`, `/api/admin/categories/{id}` | категории |
| POST, PUT, DELETE | `/api/admin/pickup-points[/{id}]` | пункты выдачи, используемый в заказах не удаляется |
| GET | `/api/admin/orders`, `/api/admin/orders/{id}` | все заказы |
| PUT | `/api/admin/orders/{id}/status` | сменить статус |
| DELETE | `/api/admin/reviews/{id}` | удалить отзыв |

Покупки удалять нельзя, поэтому у товара нет удаления: его делают недоступным.
