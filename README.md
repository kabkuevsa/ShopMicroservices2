\🛒 ShopMicroservices

Микросервисный интернет-магазин на .NET 9 / ASP.NET Core. Состоит из трёх микросервисов и API Gateway (YARP). Взаимодействие между сервисами — REST/HTTP.
📑 Содержание

    Архитектура
    Сервисы
    Стек
    Требования
    Запуск
    API
    Взаимодействие сервисов
    Тестирование
    Структура проекта
    Лицензия

🏗 Архитектура

Клиент → API Gateway (:7060) → UserService / ProductService / OrderService
                                      ↓              ↓              ↓
                                  users_db      products_db     orders_db

OrderService обращается к UserService и ProductService по HTTP. У каждого сервиса своя собственная база данных.
📦 Сервисы
Сервис 	Назначение 	БД
UserService 	Пользователи 	users_db
ProductService 	Товары 	products_db
OrderService 	Заказы 	orders_db
ApiGateway 	Точка входа (YARP) 	—
🛠 Стек

.NET 9 · ASP.NET Core · EF Core 9 · SQL Server LocalDB · BCrypt · YARP · Scalar
📋 Требования

    .NET 9 SDK
    SQL Server LocalDB (входит в Visual Studio)
    Инструмент EF Core CLI: dotnet tool install --global dotnet-ef
    Visual Studio 2022 (рекомендуется) или другая IDE
    Postman — для тестирования (опционально)

🚀 Запуск

1. Примените миграции для каждого сервиса:

dotnet ef database update --project src/UserService --startup-project src/UserService
dotnet ef database update --project src/ProductService --startup-project src/ProductService
dotnet ef database update --project src/OrderService --startup-project src/OrderService

2. Запустите все сервисы одновременно:

В Visual Studio: Solution → Configure Startup Projects → Multiple startup projects → поставьте Start у всех 4 проектов (UserService, ProductService, OrderService, ApiGateway) → F5.

3. Все запросы отправляйте на API Gateway: https://localhost:7060.
📡 API (через Gateway)
Users
Метод 	Путь 	Описание
POST 	/api/users/register 	Регистрация пользователя
POST 	/api/users/login 	Вход
GET 	/api/users/{id} 	Получить пользователя
Products
Метод 	Путь 	Описание
GET 	/api/products 	Список товаров
POST 	/api/products 	Создать товар
GET 	/api/products/{id} 	Получить товар
PUT 	/api/products/{id} 	Обновить товар
DELETE 	/api/products/{id} 	Удалить товар
POST 	/api/products/{id}/reserve?quantity=N 	Зарезервировать N единиц
Orders
Метод 	Путь 	Описание
POST 	/api/orders 	Создать заказ
GET 	/api/orders 	Список заказов
GET 	/api/orders/{id} 	Получить заказ
POST 	/api/orders/{id}/cancel 	Отменить заказ
🔗 Взаимодействие сервисов

При создании заказа OrderService:

    Проверяет пользователя в UserService
    Проверяет товар и резервирует его в ProductService
    Сохраняет заказ
    Возвращает заказ с вложенными user и product

🧪 Тестирование

Готовая Postman-коллекция: ShopMicroservices.postman_collection.json

Основной сценарий: создать пользователя → создать товар → создать заказ → проверить остаток (stock).
Обработка ошибок
Ситуация 	Ответ
Пользователь не найден 	400 { "error": … }
Товар не найден 	400 { "error": … }
Недостаточно товара 	400 { "error": … }
Сервис недоступен 	400 { "error": … }
📁 Структура проекта

ShopMicroservices/
├── src/
│   ├── UserService/
│   ├── ProductService/
│   ├── OrderService/
│   └── ApiGateway/
├── ShopMicroservices.sln
└── ShopMicroservices.postman_collection.json

📄 Лицензия

Учебный проект.
