# Task Manager System

## О проекте

**Task Manager System** - это веб-приложение для управления задачами с ролевой моделью доступа. Разработано на ASP.NET Core 8 с использованием Entity Framework Core и JWT-аутентификации.

### Основные возможности

-  **Аутентификация и авторизация** - JWT токены, ролевая модель
-  **Управление задачами** - создание, редактирование, удаление
-  **Ролевая модель** - Начальник, Сотрудник, Наблюдатель
-  **Назначение исполнителей** - гибкое распределение задач
-  **Фильтрация и сортировка** - по статусу, приоритету, отделу
-  **Swagger документация** - интерактивное тестирование API
-  **Минимальный Frontend** - демонстрация работы системы

### Роли и права

| Роль | Права |
|------|-------|
| **Начальник** | Полный контроль над задачами отдела: создание, назначение, изменение статуса и приоритета, удаление |
| **Сотрудник** | Работа только со своими задачами: создание (только для себя), изменение статуса |
| **Наблюдатель** | Только просмотр задач своего отдела |

## Технологии

- **Backend:** ASP.NET Core 8 Web API
- **ORM:** Entity Framework Core 8
- **БД:** SQLite (легко заменяется на PostgreSQL, MSSQL)
- **Аутентификация:** JWT Bearer
- **Хэширование:** BCrypt.Net-Next
- **Документация:** Swagger/OpenAPI
- **Frontend:** HTML/CSS/JavaScript (vanilla)

## Структура проекта


TaskManager.Api/
+-- Controllers/ # API контроллеры
¦ +-- AuthController.cs
¦ +-- TasksController.cs
¦ +-- UsersController.cs
¦ L-- DepartmentsController.cs
+-- Domain/ # Доменные модели (Entity)
¦ +-- User.cs
¦ +-- TaskItem.cs
¦ +-- Department.cs
¦ +-- Role.cs
¦ L-- Permission.cs
+-- DTOs/ # Объекты передачи данных
+-- Services/ # Бизнес-логика
¦ +-- Auth/
¦ L-- Task/
+-- Infrastructure/ # Инфраструктурный слой
¦ +-- Data/
¦ L-- Repositories/
+-- wwwroot/ # Статические файлы (Frontend)
¦ L-- index.html
+-- Program.cs # Точка входа
L-- appsettings.json # Конфигурация



## Установка и запуск

### Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/)
- Любая IDE (Visual Studio 2022, VS Code, Rider)

### Клонирование репозитория

```bash
git clone https://github.com/JohnGold1979/Tasks.git
cd Tasks/TaskManager.Api```


Приложение будет доступно по адресам:

    HTTP: http://localhost:5000

    HTTPS: https://localhost:5001

Проверка работы

Откройте в браузере:

    Frontend: https://localhost:5001/index.html

    Swagger UI: https://localhost:5001/swagger

Тестовые аккаунты
Логин	Пароль	Роль	Отдел
chief@it	123	Начальник	IT
employee@it	123	Сотрудник	IT
observer@it	123	Наблюдатель	IT
chief@hr	123	Начальник	HR

API Endpoints
Аутентификация
Метод	Endpoint	Описание
POST	/api/Auth/login	Вход в систему

Задачи
Метод	Endpoint	Описание	Доступ
GET	/api/Tasks	Получить список задач	Все роли
GET	/api/Tasks/{id}	Получить задачу по ID	Все роли
POST	/api/Tasks	Создать задачу	Начальник, Сотрудник
PUT	/api/Tasks/{id}/status	Изменить статус	Начальник, Сотрудник (свои)
PUT	/api/Tasks/{id}/priority	Изменить приоритет	Только Начальник
PUT	/api/Tasks/{id}/assign	Назначить исполнителя	Только Начальник
DELETE	/api/Tasks/{id}	Удалить задачу	Только Начальник

Пользователи и отделы
Метод	Endpoint	Описание
GET	/api/Users	Список пользователей
GET	/api/Users/by-department/{id}	Пользователи отдела
GET	/api/Departments	Список отделов