# Yuviron Backend — Полный разбор работы проекта

Документ описывает архитектуру, потоки запросов, доменные события, фоновые джобы, модель данных и деплой. Цель — чтобы новый разработчик мог детально понять, как все устроено и как проходит полный процесс выполнения операций.

## 1. Архитектура и структура решения

Проект разбит на 4 слоя:

- `Yuviron.Api` — HTTP API, контроллеры, middleware, Swagger, CORS, health checks, статика.
- `Yuviron.Application` — бизнес‑логика, CQRS (команды/запросы), MediatR, валидации, обработчики доменных событий.
- `Yuviron.Domain` — сущности, доменные события, исключения, перечисления.
- `Yuviron.Infrastructure` — EF Core, MySQL, Redis, аутентификация, кэш, email, файловое хранилище, фоновые джобы.

Зависимости:

- `Api` зависит от `Application` и `Infrastructure`.
- `Application` зависит от `Domain`.
- `Infrastructure` зависит от `Application` и `Domain`.
- `Domain` независим.

## 2. Запуск приложения: пошаговый сценарий

Последовательность выполнения в `Program.cs`:

1. Создается `WebApplicationBuilder`.
2. Регистрируется слой Application (`AddApplication`).
3. Регистрируется слой Infrastructure (`AddInfrastructure`).
4. Добавляются контроллеры и Swagger.
5. Настраивается CORS.
6. Регистрируется обработчик глобальных ошибок и ProblemDetails.
7. Собирается `app`.
8. Выполняется `AppDbContextInitializer`:
   - `InitialiseAsync()` применяет миграции.
   - `SeedAsync()` заполняет базовые данные.
9. Настраивается pipeline:
   - Exception handler.
   - Swagger (только dev или `Swagger:Enabled`).
   - HTTPS редирект (не dev).
   - CORS.
   - Static files из `FILE_STORAGE_ROOT` по пути `/storage`.
   - Authentication + Authorization.
   - Контроллеры.
10. Регистрируются health checks:
    - `/health/live` — self.
    - `/health/ready` — MySQL + Redis с JSON‑ответом.

## 3. Конфигурация и окружение

Основные файлы:

- `appsettings.json` — общие настройки + `JwtSettings` + `SeedUsers`.
- `appsettings.Development.json` — строки подключения, Redis, email, `FILE_STORAGE_ROOT`.

Ключевые параметры:

- `ConnectionStrings:Default` — MySQL.
- `ConnectionStrings:Redis` — Redis.
- `JwtSettings` — секрет, issuer, audience, срок жизни.
- `Email` — SMTP настройки.
- `FILE_STORAGE_ROOT` — корень файлового хранилища.
- `Swagger:Enabled` — включение Swagger.

В dev‑деплое GitHub Actions генерирует `appsettings.Development.json` из секретов.

## 4. Dependency Injection: что и где регистрируется

### Application (`AddApplication`)

- MediatR.
- Pipeline behaviors:
  - `LoggingBehavior` — логирует запросы, скрывает `ISensitiveRequest`.
  - `ValidationBehavior` — FluentValidation.
  - `AuthorizationBehavior` — проверка `ISecuredRequest`.
- Автоподключение валидаторов.

### Infrastructure (`AddInfrastructure`)

- EF Core (MySQL, Pomelo):
  - Retry on failure.
  - Query splitting.
- JWT аутентификация.
- Redis:
  - `IDistributedCache`.
  - `IConnectionMultiplexer`.
- Health checks.
- Фоновые сервисы:
  - `ProcessOutboxMessagesJob`.
  - `TempFilesCleanupJob`.
- Основные сервисы:
  - `IJwtTokenGenerator`.
  - `IPasswordHasher`.
  - `IPermissionService`.
  - `ICacheService`.
  - `IEmailService`.
  - `IOtpService`.
  - `IFileStorageService`.
  - `IAudioMetadataService`.
  - `IUserContext`, `ICurrentUserService`.
  - `TimeProvider.System`.

## 5. Полный жизненный цикл HTTP‑запроса

Ниже детальный пошаговый поток любого запроса:

1. HTTP запрос приходит в контроллер.
2. Контроллер вызывает MediatR `Send()`.
3. Pipeline behaviors:
   - `LoggingBehavior` записывает старт запроса.
   - `ValidationBehavior` вызывает FluentValidation.
   - `AuthorizationBehavior` проверяет `ISecuredRequest`.
4. Handler выполняет бизнес‑логику и изменения через `IApplicationDbContext`.
5. `SaveChangesAsync()`:
   - Собирает доменные события из измененных сущностей.
   - Создает записи `OutboxMessage`.
   - Сохраняет изменения в БД.
6. HTTP ответ отправляется клиенту.
7. Outbox‑воркер асинхронно обрабатывает доменные события и выполняет побочные эффекты.

Ключевой момент: многие побочные эффекты выполняются не внутри запроса, а позже через outbox.

## 6. Обработка ошибок

`GlobalExceptionHandler` преобразует исключения в ProblemDetails:

- `ValidationException` -> 400 с массивом ошибок.
- `ArgumentException` / `InvalidOperationException` -> 400.
- `NotFoundException` -> 404.
- `UserAlreadyExistsException` -> 409.
- `UnauthorizedAccessException` -> 401 или 403.
- `DomainException` -> 400.
- `DbUpdateException` (дубликат) -> 409.
- Остальное -> 500.

## 7. Аутентификация и авторизация

### JWT

- Access‑token содержит:
  - `sub`, `NameIdentifier`, email.
  - роли (`ClaimTypes.Role`).
  - `is_premium` при активной подписке.
- Срок жизни берется из `JwtSettings:ExpiryMinutes`.

### Refresh Tokens

- В БД хранится только хеш.
- Логика refresh:
  1. Находит токен по хешу.
  2. Если токен был уже использован:
     - в пределах 1 минуты можно повторить.
     - после 1 минуты — ревок всех токенов.
  3. Создает новый refresh + access.

### Права доступа

- `ISecuredRequest.RequiredPermission`.
- Проверка выполняется в `AuthorizationBehavior`.
- `PermissionService`:
  - рассчитывает права по ролям и подписке.
  - кэширует в Redis на 1 час.
  - инвалидирует кэш через `UserPermissionsChangedEvent`.

### Auth endpoints (кратко)

- `POST /api/auth/check-email`
- `POST /api/auth/send-code`
- `POST /api/auth/login-with-code`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me/permissions`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `POST /api/auth/change-password`

Refresh‑token хранится в `HttpOnly` cookie.

## 8. Доменные события и Outbox

### Как события попадают в outbox

1. Сущность вызывает `AddDomainEvent`.
2. `AppDbContext.SaveChangesAsync`:
   - забирает события из `Entity.DomainEvents`.
   - сериализует событие.
   - создает `OutboxMessage`.

### Как события обрабатываются

`ProcessOutboxMessagesJob`:

1. Каждые 5 секунд проверяет outbox.
2. Берет пачку 20 записей с `FOR UPDATE SKIP LOCKED`.
3. Публикует доменные события через MediatR.
4. При ошибке делает ретрай:
   - максимум 5 попыток.
   - задержка 5s, 10s, 20s, 40s, 80s.
5. Помечает как `Processed` или `Failed`.

### Главные события и обработчики

- `TempFileNeedsMovingEvent` — перенос файла из temp в целевую папку.
- `FileNeedsDeletionEvent` — удаление файла.
- `UserPermissionsChangedEvent` — инвалидация кэша прав.
- `UserPasswordChangedEvent` — ревок refresh‑токенов.
- `UserDeletedEvent` — отмена подписок + очистка профиля.
- `ArtistDeletedEvent` — скрытие или переразметка альбомов.
- `AlbumDeletedEvent` — удаление треков альбома.
- `TrackDeletedEvent` — удаление треков из плейлистов.

## 9. Работа с файлами

### Общий процесс

1. `POST /api/files/upload` принимает файл.
2. Файл сохраняется в `FILE_STORAGE_ROOT/temp`.
3. Ответ содержит относительный путь и URL `/storage/{path}`.
4. При создании/обновлении сущностей:
   - путь в БД сохраняется уже как конечный.
   - событие `TempFileNeedsMovingEvent` переносит файл из `temp` в нужную папку.

### Важные детали

- Параметр `folder` в `FilesController` сейчас не используется.
- `LocalFileStorageService` защищает от `..` и path traversal.
- Файл сохраняется под GUID‑именем.
- `MoveAsync` идемпотентен: если файл уже перенесен, операция считается успешной.

### Очистка временных файлов

`TempFilesCleanupJob`:

- запускается каждые 12 часов.
- удаляет файлы в `temp`, которым больше 24 часов.

## 10. Проверка аудиофайлов

`AudioMetadataService`:

- использует TagLibSharp.
- проверяет, что файл аудио.
- проверяет длительность (1 сек – 3 часа).
- возвращает метаданные для трека.

Используется при создании/обновлении треков.

## 11. Модель данных (крупные области)

Основные доменные области:

- Identity: User, Role, Permission, UserRole, RefreshToken, UserBlock.
- Profile: UserProfile, UserSettings, Theme, CustomTheme.
- Catalog: Artist, Album, Track, Genre, Mood, TeamMember.
- Library: Playlist, PlaylistTrack, SavedTrack/Album.
- Monetization: Plan, Subscription, Payouts, Ads.
- Player: PlaybackSession, PlaybackQueueItem.
- Social: SharedRoom, SmartLink.
- Content/Moderation: Lyrics, Complaint, VerificationRequest.
- Analytics: ListeningEvent, TrackListenHeatmap.
- Notifications: Notification, ReleaseNotificationTemplate.
- Outbox: OutboxMessage.

### Soft‑delete

Фильтры `HasQueryFilter` используются для:

- User
- Artist
- Album
- Track
- Genre
- Mood
- Playlist

## 12. EF Core и миграции

Маппинги в `Infrastructure/Persistence/Configurations`:

- Все таблицы в snake_case.
- Индексы на важные поля (email, name, status).
- Ссылки с `Cascade` или `Restrict` в зависимости от сущности.
- Отдельные конфиги для outbox.

SQL‑снапшоты:

- `latest_migration.sql`
- `outbox_migration.sql`

## 13. Админ API: структура и права

Все admin‑эндпоинты требуют `[Authorize]`, а разрешения проверяются через `ISecuredRequest`.

Права:

- `AccessAdminPanel` — чтение.
- `ManageCatalog` — создание/обновление/удаление контента.
- `ManageUsers` — управление пользователями.

Контроллеры и пути:

- `/api/admin/dashboard`
  - `GET /stats`
- `/api/admin/users`
  - `GET /`
  - `GET /{id}`
  - `POST /`
  - `PUT /{id}`
  - `DELETE /{id}`
  - `POST /{id}/block`
  - `POST /{id}/unblock`
- `/api/admin/roles`
  - `GET /`
- `/api/admin/artists`
  - `GET /`
  - `GET /{id}`
  - `POST /`
  - `PUT /{id}`
  - `DELETE /{id}`
  - `GET /{id}/team`
  - `POST /{id}/team`
  - `PUT /{id}/team/{userId}`
  - `DELETE /{id}/team/{userId}`
- `/api/admin/albums`
  - `GET /`
  - `GET /{id}`
  - `POST /`
  - `PUT /{id}`
  - `DELETE /{id}`
- `/api/admin/tracks`
  - `GET /`
  - `GET /{id}`
  - `POST /`
  - `PUT /{id}`
  - `DELETE /{id}`
- `/api/admin/genres`
  - `GET /`
  - `GET /{id}`
  - `POST /`
  - `PUT /{id}`
  - `DELETE /{id}`
- `/api/admin/moods`
  - `GET /`
  - `GET /{id}`
  - `POST /`
  - `PUT /{id}`
  - `DELETE /{id}`
- `/api/admin/playlists`
  - `GET /`
  - `GET /{id}`
  - `POST /`
  - `PUT /{id}`
  - `DELETE /{id}`
  - `GET /{id}/tracks`
  - `POST /{id}/tracks`
  - `DELETE /{id}/tracks/{trackId}`
  - `PUT /{id}/tracks/{trackId}/position`

## 14. Фоновые джобы

### Outbox обработчик

- Читает outbox.
- Публикует события через MediatR.
- Делает ретраи и backoff.

### Cleanup temp файлов

- Удаляет временные файлы старше 24 часов.
- Запускается раз в 12 часов.

## 15. Health checks

- `/health/live` — simple self.
- `/health/ready` — MySQL + Redis с JSON‑ответом.

## 16. Деплой и окружения

### GitHub Actions (deploy-dev.yml)

При пуше в `dev`:

1. Self‑hosted runner делает `git reset --hard origin/dev`.
2. Генерирует `appsettings.Development.json`.
3. Пересобирает migrator.
4. Выполняет миграции.
5. Пересобирает и перезапускает backend + frontend.

### docker-compose.yml

Поднимает:

- MySQL 8.0.
- Redis.

### Elastic Beanstalk

`.ebextensions/01_redis_docker.config`:

- Устанавливает Docker.
- Запускает Redis контейнер.

## 17. Ключевые бизнес‑процессы: пошагово

### Регистрация

1. `POST /api/auth/register`.
2. Проверяется уникальность email.
3. Хэшируется пароль.
4. Создается User + UserProfile.
5. Привязываются роли.
6. Создается Artist при `IsArtist = true`.
7. Сохраняется в БД.
8. Отправляется email (ошибка не блокирует процесс).

### Логин (пароль)

1. `POST /api/auth/login`.
2. Проверяется пользователь и пароль.
3. Проверяется состояние аккаунта.
4. Вычисляются права.
5. Генерируется access + refresh.
6. Refresh‑token сохраняется в БД (в хеше).
7. Refresh‑token кладется в cookie.

### Логин по коду

1. `POST /api/auth/send-code`:
   - Генерируется 6‑значный код.
   - Хэш кода сохраняется в Redis на 10 минут.
   - Код отправляется email.
2. `POST /api/auth/login-with-code`:
   - Проверяется код в Redis.
   - Код удаляется из Redis.
   - Выдаются access + refresh.

### Refresh access token

1. `POST /api/auth/refresh` читает refresh cookie.
2. Проверяется токен в БД.
3. При reuse вне grace‑периода ревок всех токенов.
4. Старый refresh ревокается.
5. Создается новый refresh + access.

### Создание трека (с файлом)

1. `POST /api/files/upload` -> файл попадает в `temp`.
2. `POST /api/admin/tracks`:
   - Проверяются Album/Artist/Genre/Mood.
   - Читается метадата аудио.
   - В БД сохраняется путь уже с `tracks/`.
   - Создается доменное событие `TempFileNeedsMovingEvent`.
3. Outbox‑воркер переносит файл из `temp/` в `tracks/`.

### Удаление трека

1. `DELETE /api/admin/tracks/{id}`.
2. Трек помечается удаленным.
3. Доменные события:
   - Удаление обложки.
   - Удаление аудио.
4. Outbox‑воркер удаляет файлы.
5. Отдельный handler очищает треки из плейлистов.

### Удаление пользователя

1. `DELETE /api/admin/users/{id}`.
2. User помечается удаленным, email анонимизируется.
3. Доменные события:
   - `UserDeletedEvent`.
   - `UserPermissionsChangedEvent`.
   - `FileNeedsDeletionEvent` (avatar).
4. Outbox‑воркер:
   - отменяет подписки.
   - чистит профиль и удаляет аватар.
   - инвалидирует кэш прав.

## 18. Что важно помнить разработчику

- Побочные эффекты не гарантированы синхронно: outbox делает их позже.
- Redis не критичен, но влияет на производительность.
- Все admin‑операции завязаны на `AppPermission`.
- `FILE_STORAGE_ROOT` должен быть доступен и writable.
- Temp‑файлы чистятся автоматически, не храните их долго.
