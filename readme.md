# Проект SalesService

## Swagger
Для доступа к документации API используйте Swagger UI по следующему адресу:
http://localhost:port/swagger/index.html
Замените `port` на порт, на котором работает ваше приложение.

## Описание
Проект предоставляет API для работы с продажами, аналитикой и предсказаниями. 
Он включает несколько эндпоинтов для добавления, обновления и получения информации о продажах, а также для анализа и предсказания данных.
## Таблица HTTP запросов

| Название запроса     | Тип запроса | Описание                                                                 |
|----------------------|-------------|--------------------------------------------------------------------------|
| **Add-sale**         | POST        | Этот метод добавляет новую продажу, если она найдена по ID.              |
| **Update-sale**      | POST        | Этот метод обновляет продажу, если она найдена по ID.                   |
| **Generate-forecast**| GET         | Этот метод анализирует последние N дней и делает предсказание.          |
| **Get-analytics**    | GET         | Этот метод выводит аналитику в указанном диапазоне времени.             |
| **Get-trends**       | GET         | Этот метод получает продажи по указанной категории.                     |
| **Get-all-sales**    | GET         | Этот метод выводит список всех существующих продаж.                     |

## Команда для запуска миграций

Чтобы применить миграции к базе данных, выполните следующую команду в терминале:

```bash
dotnet ef database update
```
Порядок
1) Запускаем docker compose
2) Накатываем миграции
