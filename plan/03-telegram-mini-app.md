# Pick'em — Telegram Mini App

## Что такое Telegram Mini App

Telegram Mini App (ранее Web App) — это веб-приложение, открывающееся внутри Telegram как встроенный WebView. Пользователь нажимает кнопку у бота → открывается ваше веб-приложение с полной информацией о пользователе.

**Ключевые особенности:**
- Работает в WebView (Chrome-based на Android, Safari-based на iOS)
- Telegram передаёт `initData` с подписанными данными пользователя
- Доступ к Telegram UI: кнопка BackButton, MainButton, haptic feedback, тема
- Размер окна: полноэкранный или половина экрана
- Максимальный размер JS bundle: нет жёсткого лимита, но чем меньше — тем лучше (< 200 KB)

## Почему НЕ Blazor WASM

| Критерий | Blazor WASM | React/Preact |
|----------|-------------|--------------|
| Размер бандла | ~5-10 MB (.NET runtime + DLLs) | 50-150 KB gzipped |
| Время загрузки (3G) | 15-30 сек | 1-3 сек |
| Telegram WebView совместимость | Работает, но тяжело | Нативно |
| Доступ к Telegram JS SDK | Через JS Interop | Напрямую |
| Hot reload в MiniApp | Сложно | Стандартно |

**Вердикт:** Blazor WASM слишком тяжёл для Telegram Mini App. Рекомендуется лёгкий JS-фреймворк.

## Выбранный фронтенд

- **React** (зафиксировано)
- Vite + TypeScript
- Telegram Web App SDK

## Архитектура Telegram Mini App

```
┌───────────────────────────────────────────────────┐
│                    Telegram                        │
│                                                    │
│  ┌──────────────────────────────────────────────┐ │
│  │              Mini App WebView                 │ │
│  │                                               │ │
│  │  ┌─── React App ─────────────────────────┐   │ │
│  │  │                                        │   │ │
│  │  │  [Match Card]  [Leaderboard]  [Rules]  │   │ │
│  │  │       │                │                │   │ │
│  │  │       ▼                ▼                │   │ │
│  │  │  REST API calls   Poll Blob JSON          │   │ │
│  │  │       │                │                  │   │ │
│  │  └───────┼────────────────┼──────────────────┘   │ │
│  └──────────┼────────────────┼──────────────────────┘ │
└─────────────┼────────────────┼────────────────────────┘
              │                │
              ▼                ▼
    ┌─────────────────────────────────┐
    │  Azure Functions (API + Bot)    │
    └─────────────────────────────────┘
              │
              ▼
    ┌─────────────────────────────────┐
    │ Azure Blob: match-state-<id>.json│
    └─────────────────────────────────┘
```

## Аутентификация через Telegram

### Поток аутентификации

```
1. Пользователь нажимает кнопку в Telegram боте
2. Telegram открывает Mini App URL с параметром initData
3. Mini App читает window.Telegram.WebApp.initData
4. Mini App отправляет initData в заголовке Authorization на бэкенд
5. Бэкенд валидирует HMAC-SHA256 подпись через Bot Token
6. Бэкенд возвращает JWT (или session token) для дальнейших запросов
```

### Валидация initData на бэкенде (C#)

```csharp
public static bool ValidateTelegramInitData(string initData, string botToken)
{
    var parsed = HttpUtility.ParseQueryString(initData);
    var hash = parsed["hash"];
    parsed.Remove("hash");

    var dataCheckString = string.Join("\n",
        parsed.AllKeys
              .OrderBy(k => k)
              .Select(k => $"{k}={parsed[k]}"));

    using var hmacSha256 = new HMACSHA256(
        HMACSHA256.HashData(Encoding.UTF8.GetBytes("WebAppData"), Encoding.UTF8.GetBytes(botToken)));
    
    var computedHash = Convert.ToHexString(
        hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString))).ToLower();

    return computedHash == hash;
}
```

### Данные пользователя из initData

```json
{
  "user": {
    "id": 123456789,
    "first_name": "Иван",
    "last_name": "Петров",
    "username": "ivanpetrov",
    "language_code": "ru",
    "photo_url": "https://t.me/i/userpic/..."
  },
  "auth_date": 1735000000,
  "hash": "a1b2c3d4..."
}
```

## Telegram Bot Setup

Для запуска Mini App нужен Telegram бот:

1. Создать бота через @BotFather
2. Настроить Menu Button → Web App URL
3. Или создать inline-кнопку с Web App URL

### Команды BotFather

```
/newbot → создать бота
/setmenubutton → установить кнопку Mini App
/setdescription → описание бота
```

### Inline-кнопка для открытия Mini App из сообщения бота

```python
# Бот (Python или C#) отправляет сообщение с кнопкой:
InlineKeyboardButton(
    text="🎯 Открыть Pick'em",
    web_app=WebAppInfo(url="https://pickem.yoursite.com")
)
```

## Использование Telegram UI в Mini App

```javascript
const tg = window.Telegram.WebApp;

// Тема (светлая/тёмная — синхронизируется с Telegram)
tg.themeParams.bg_color       // "#ffffff"
tg.themeParams.text_color     // "#000000"
tg.themeParams.button_color   // "#3390ec"

// Haptic feedback (вибрация при нажатии кнопок)
tg.HapticFeedback.impactOccurred("medium");

// MainButton (большая кнопка внизу экрана)
tg.MainButton.setText("Сохранить прогноз");
tg.MainButton.show();
tg.MainButton.onClick(() => submitPrediction());

// BackButton
tg.BackButton.show();
tg.BackButton.onClick(() => navigateBack());

// Закрыть Mini App
tg.close();

// Expand to full screen
tg.expand();
```

## Хостинг Mini App

Mini App — это обычный статический сайт. Варианты хостинга:

| Хостинг | Стоимость | HTTPS | CDN | Рекомендация |
|---------|-----------|-------|-----|--------------|
| Azure Static Web Apps | Free tier | ✅ | ✅ | ✅ Уже используется |
| Vercel | Free tier | ✅ | ✅ | Быстрый деплой |
| Cloudflare Pages | Free tier | ✅ | ✅ Global | Минимальный latency |
| GitHub Pages | Free | ✅ | ✅ | Для MVP |

**Требование:** HTTPS обязателен для Telegram Mini App.

## Экраны Mini App

### 1. Главный экран (Tournament)
- Название турнира + логотип
- Текущий матч (карточка с кнопками прогноза)
- Кнопка "Leaderboard"
- Статус регистрации (игровой ник)

### 2. Match Card (состояния)
- **Upcoming**: "Ожидается игра #N" — кнопки неактивны
- **Open**: Кнопки "Мирные"/"Мафия" + сетка игроков 1-10 + "Никто" + crowd-статистика (агрегированные проценты) → MainButton "Сохранить"
- **Locked**: Кнопки заблокированы + crowd-статистика (финальные проценты перед `Resolved`)
- **Resolved**: Подсвечены правильные ответы + начисленные очки + crowd-статистика (итоговые агрегированные проценты)
- **Canceled**: Серая карточка "Игра отменена"

### 3. Leaderboard
- Ранг | Аватар + Никнейм | Очки
- Выделение Топ-3 (золото/серебро/бронза)
- Своя позиция подсвечена

### 4. Admin Panel (отдельный URL / доступ по Telegram ID)
- Список матчей с кнопками смены статуса
- Dropdown для выбора правильных ответов
- Кнопки "Завершить и рассчитать" / "Аннулировать"

## Регистрация пользователя

**Требование:** доступ к прогнозам открыт зарегистрированным пользователям, обязательна только регистрация по игровому нику.

### Поток регистрации

1. Пользователь открывает Mini App через Telegram бота.
2. Mini App проходит Telegram auth и вызывает `/api/auth/telegram`.
3. Если у пользователя нет сохраненного ника, показывается экран регистрации.
4. Пользователь вводит игровой ник и подтверждает сохранение.
5. Mini App вызывает `/api/me/nickname` для сохранения ника.
6. После успешного сохранения ника доступ к прогнозам и лидерборду активируется.
