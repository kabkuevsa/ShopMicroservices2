# 🛒 ShopMicroservices

Микросервисное приложение интернет-магазина на **C# / ASP.NET Core Web API (.NET 9)**.

Состоит из трёх микросервисов и API Gateway (YARP). Микросервисы взаимодействуют через REST/HTTP. Покрыто интеграционными тестами (**24 шт.**).

![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet)
![EF Core](https://img.shields.io/badge/EF%20Core-9-512BD4)
![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoftsqlserver)
![Tests](https://img.shields.io/badge/tests-24%20passed-brightgreen)

## 📑 Содержание

- [Архитектура](#-архитектура)
- [Сервисы и порты](#-сервисы-и-порты)
- [Стек](#-стек)
- [Запуск](#-запуск)
- [API](#-api-через-gateway-7060)
- [Взаимодействие сервисов](#-взаимодействие-сервисов)
- [Интеграционные тесты](#-интеграционные-тесты)
- [Обработка ошибок](#-обработка-ошибок)
- [Структура проекта](#-структура-проекта)
- [Лицензия](#-лицензия)

## 🏗 Архитектура

```text
        Клиент (Postman)
              │
              ▼
    ┌─────────────────────┐
    │    API Gateway      │  https://localhost:7060
    │    (YARP)           │
    └──────────┬──────────┘
               │
    ┌──────────┼──────────┐
    ▼          ▼          ▼
┌────────┐ ┌────────┐ ┌────────┐
│  User  │ │Product │ │ Order  │
│ :7036  │ │ :7015  │ │ :7248  │
└───┬────┘ └───┬────┘ └───┬────┘
    │          │          │
 users_db  products_db  orders_db
                         │
                         └─► вызывает User и Product по HTTP
```

## 📦 Сервисы и порты

| Сервис           | Назначение                | Порт   | БД            |
|------------------|---------------------------|--------|---------------|
| `ApiGateway`     | Единая точка входа (YARP) | `7060` | —             |
| `UserService`    | Пользователи              | `7036` | `users_db`    |
| `ProductService` | Товары                    | `7015` | `products_db` |
| `OrderService`   | Заказы                    | `7248` | `orders_db`   |

Все сервисы работают по HTTPS: `https://localhost:<порт>`.

## 🛠 Стек

**Приложение:** .NET 9 · ASP.NET Core · EF Core 9 · SQL Server LocalDB · BCrypt · YARP · Scalar

**Тесты:** xUnit · WebApplicationFactory · Moq · FluentAssertions

## 🚀 Запуск

### Требования

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server LocalDB
- Visual Studio 2022
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Шаги

**1. Примените миграции для каждого сервиса:**

```powershell
dotnet ef database update --project src/UserService --startup-project src/UserService
dotnet ef database update --project src/ProductService --startup-project src/ProductService
dotnet ef database update --project src/OrderService --startup-project src/OrderService
```

**2. Запустите все сервисы одновременно:**

*Solution → Configure Startup Projects → Multiple startup projects* → **Start** у всех 4 проектов → **F5**.

**3.** Отправляйте запросы на API Gateway: `https://localhost:7060`.

## 📡 API (через Gateway :7060)

### Users

| Метод | Путь                  | Описание                 |
|-------|-----------------------|--------------------------|
| POST  | `/api/users/register` | Регистрация пользователя |
| POST  | `/api/users/login`    | Вход                     |
| GET   | `/api/users/{id}`     | Получить пользователя    |

### Products

| Метод  | Путь                                    | Описание                 |
|--------|-----------------------------------------|--------------------------|
| GET    | `/api/products`                         | Список товаров           |
| POST   | `/api/products`                         | Создать товар            |
| GET    | `/api/products/{id}`                    | Получить товар           |
| PUT    | `/api/products/{id}`                    | Обновить товар           |
| DELETE | `/api/products/{id}`                    | Удалить товар            |
| POST   | `/api/products/{id}/reserve?quantity=N` | Зарезервировать N единиц |

### Orders

| Метод | Путь                      | Описание       |
|-------|---------------------------|----------------|
| POST  | `/api/orders`             | Создать заказ  |
| GET   | `/api/orders`             | Список заказов |
| GET   | `/api/orders/{id}`        | Получить заказ |
| POST  | `/api/orders/{id}/cancel` | Отменить заказ |

## 🔗 Взаимодействие сервисов

При создании заказа `OrderService`:

1. Проверяет пользователя в `UserService`
2. Проверяет товар и резервирует его в `ProductService`
3. Сохраняет заказ в `orders_db`
4. Возвращает заказ с вложенными `user` и `product`

## 🧪 Интеграционные тесты

### Запуск

```powershell
dotnet test
```

### Что покрыто (24 теста)

| Проект                 | Тестов | Что проверяют                                                                                              |
|------------------------|:------:|------------------------------------------------------------------------------------------------------------|
| `UserService.Tests`    | 6      | Регистрация, дубликат email, логин, неверный пароль, получение, 404                                        |
| `ProductService.Tests` | 7      | Создание, список, получение, 404, резерв Stock, недостаток Stock, фильтр                                   |
| `OrderService.Tests`   | 7      | Создание заказа с вложенными данными, юзер/товар не найден, мало Stock, недоступность сервиса, список, 404 |
| `ApiGateway.Tests`     | 4      | Запуск, регистрация маршрутов                                                                              |

### Подход

- **`WebApplicationFactory<Program>`** — поднимает реальный сервис в памяти.
- **`Environment=Testing`** — в `Program.cs` подменяет БД на in-memory.
- **Moq** — мокает HTTP-клиенты в `OrderService`.
- **FluentAssertions** — читаемые проверки.

### Результат

```text
Passed!  - Failed: 0, Passed: 24, Skipped: 0, Total: 24
```

### Скриншоты

![Результаты тестов — 1](https://github.com/user-attachments/assets/5ee97804-b6d8-4609-8067-c1573c4c01b5)

![Результаты тестов — 2](https://github.com/user-attachments/assets/8deed5a8-eb8d-4c41-9d29-8cb070df4c10)

![Результаты тестов — 3](https://github.com/user-attachments/assets/a352d7d0-cd85-4208-9032-ace39de7a308)

## 🧯 Обработка ошибок

| Ситуация               | Ответ                |
|------------------------|----------------------|
| Юзер / товар не найден | `400 { "error": … }` |
| Мало товара            | `400 { "error": … }` |
| Сервис недоступен      | `400 { "error": … }` |

## 📁 Структура проекта

```text
ShopMicroservices/
├── src/
│   ├── ApiGateway/
│   ├── OrderService/
│   ├── ProductService/
│   └── UserService/
├── tests/
│   ├── ApiGateway.Tests/
│   ├── OrderService.Tests/
│   ├── ProductService.Tests/
│   └── UserService.Tests/
├── ShopMicroservices.sln
├── ShopMicroservices.postman_collection.json
└── README.md
```

## 📄 Лицензия

Учебный проект.
