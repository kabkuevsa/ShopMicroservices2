# 🛒 ShopMicroservices

Микросервисный интернет-магазин на **.NET 9 / ASP.NET Core**. Состоит из трёх микросервисов и API Gateway (YARP). Взаимодействие между сервисами — REST/HTTP.

![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet)
![EF Core](https://img.shields.io/badge/EF%20Core-9-512BD4)
![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoftsqlserver)

## 📑 Содержание

- [Архитектура](#-архитектура)
- [Сервисы](#-сервисы)
- [Порты](#-порты)
- [Стек](#-стек)
- [Требования](#-требования)
- [Запуск](#-запуск)
- [API](#-api-через-gateway)
- [Взаимодействие сервисов](#-взаимодействие-сервисов)
- [Тестирование](#-тестирование)
- [Структура проекта](#-структура-проекта)
- [Лицензия](#-лицензия)

## 🏗 Архитектура

```text
Клиент → API Gateway (:7060) → UserService / ProductService / OrderService
                                      ↓              ↓              ↓
                                  users_db      products_db     orders_db
```

`OrderService` обращается к `UserService` и `ProductService` по HTTP. У каждого сервиса своя собственная база данных.

## 📦 Сервисы

| Сервис         | Назначение        | БД            |
|----------------|-------------------|---------------|
| `UserService`    | Пользователи      | `users_db`    |
| `ProductService` | Товары            | `products_db` |
| `OrderService`   | Заказы            | `orders_db`   |
| `ApiGateway`     | Точка входа (YARP) | —             |

## 🔌 Порты

| Сервис           | Адрес                    |
|------------------|--------------------------|
| `UserService`    | `https://localhost:7036` |
| `ProductService` | `https://localhost:7015` |
| `OrderService`   | `https://localhost:7248` |
| `ApiGateway`     | `https://localhost:7060` |

## 🛠 Стек

.NET 9 · ASP.NET Core · EF Core 9 · SQL Server LocalDB · BCrypt · YARP · Scalar

## 📋 Требования

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server LocalDB (входит в Visual Studio)
- Инструмент EF Core CLI: `dotnet tool install --global dotnet-ef`
- Visual Studio 2022 (рекомендуется) или другая IDE
- [Postman](https://www.postman.com/) — для тестирования (опционально)

## 🚀 Запуск

**1. Примените миграции для каждого сервиса:**

```powershell
dotnet ef database update --project src/UserService --startup-project src/UserService
dotnet ef database update --project src/ProductService --startup-project src/ProductService
dotnet ef database update --project src/OrderService --startup-project src/OrderService
```

**2. Запустите все сервисы одновременно:**

В Visual Studio: *Solution → Configure Startup Projects → Multiple startup projects* → поставьте **Start** у всех 4 проектов (`UserService`, `ProductService`, `OrderService`, `ApiGateway`) → **F5**.

**3.** Все запросы отправляйте на API Gateway: `https://localhost:7060`.

## 📡 API (через Gateway)

### Users

| Метод | Путь                   | Описание                |
|-------|------------------------|-------------------------|
| POST  | `/api/users/register`  | Регистрация пользователя |
| POST  | `/api/users/login`     | Вход                    |
| GET   | `/api/users/{id}`      | Получить пользователя   |

### Products

| Метод  | Путь                                   | Описание                    |
|--------|----------------------------------------|-----------------------------|
| GET    | `/api/products`                        | Список товаров              |
| POST   | `/api/products`                        | Создать товар               |
| GET    | `/api/products/{id}`                   | Получить товар              |
| PUT    | `/api/products/{id}`                   | Обновить товар              |
| DELETE | `/api/products/{id}`                   | Удалить товар               |
| POST   | `/api/products/{id}/reserve?quantity=N`| Зарезервировать N единиц    |

### Orders

| Метод | Путь                       | Описание        |
|-------|----------------------------|-----------------|
| POST  | `/api/orders`              | Создать заказ   |
| GET   | `/api/orders`              | Список заказов  |
| GET   | `/api/orders/{id}`         | Получить заказ  |
| POST  | `/api/orders/{id}/cancel`  | Отменить заказ  |

## 🔗 Взаимодействие сервисов

При создании заказа `OrderService`:

1. Проверяет пользователя в `UserService`
2. Проверяет товар и резервирует его в `ProductService`
3. Сохраняет заказ
4. Возвращает заказ с вложенными `user` и `product`

## 🧪 Тестирование

Готовая Postman-коллекция: [`ShopMicroservices.postman_collection.json`](ShopMicroservices.postman_collection.json)

**Основной сценарий:** создать пользователя → создать товар → создать заказ → проверить остаток (stock).

### Обработка ошибок

| Ситуация                | Ответ                |
|-------------------------|----------------------|
| Пользователь не найден  | `400 { "error": … }` |
| Товар не найден         | `400 { "error": … }` |
| Недостаточно товара     | `400 { "error": … }` |
| Сервис недоступен       | `400 { "error": … }` |

## 📁 Структура проекта

```text
ShopMicroservices/
├── src/
│   ├── UserService/
│   ├── ProductService/
│   ├── OrderService/
│   └── ApiGateway/
├── ShopMicroservices.sln
└── ShopMicroservices.postman_collection.json
```

## 📄 Лицензия

Учебный проект.

📄 Лицензия

Учебный проект.
