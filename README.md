# Xml parser App
Микросервисное приложение для обработки XML-файлов, трансформации данных в формат json и сохранения состояния устройств в базе данных SQLite.


## Архитектура
Приложение состоит из двух сервисов, взаимодействующих через RabbitMQ:

### FileParserService
• Мониторит директорию с XML-файлами  
• Десериализует XML → C# модели  
• Обновляет статус устройств  
• Сериализует данные в JSON  
• Публикует сообщения в очередь RabbitMQ

### DataProcessorService
• Подписывается на очередь RabbitMQ  
• Десериализует JSON  
• Сохраняет / обновляет данные в базе SQLite  
• Применяет миграции при старте 


## Технологии
• .Net 10
• RabbitMQ
• Entity Framework Core
• SQLite
• Docker / Docker Compose
• BackgroundService / IHostedService


## Запуск в Docker
В корне проекта выполните:
```bash
docker compose up --build -d
```