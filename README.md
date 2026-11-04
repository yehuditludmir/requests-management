# מערכת לניהול פניות

ניהול פניות מארגונים: רשימה עם דפדוף, סינון, חיפוש ומיון בצד השרת, סיכומים, עדכון סטטוס עם זיהוי התנגשויות, היסטוריית שינויים ועדכון מרובה (Bulk). 100,000 פניות במסד.

**מסמכים נוספים:** [ביצועים ואינדקסים](docs/performance.md) · [תוכנית עבודה](docs/work-plan.md)

## טכנולוגיות

| | |
|---|---|
| **שרת** | .NET 8 (ASP.NET Core Web API), EF Core 8 |
| **מסד נתונים** | SQL Server (LocalDB 2019) |
| **לקוח** | Angular 21, RxJS, TypeScript |
| **בדיקות** | xUnit, WebApplicationFactory |

## הרצה

**דרישות:** .NET 8 SDK, Node.js 20.19+, SQL Server LocalDB.

```
cd server
dotnet run --project RequestsManagement.API -- --seed --reset   # יצירת המסד + 100,000 פניות
dotnet run --project RequestsManagement.API --launch-profile http     # השרת: http://localhost:5275/swagger
dotnet test                                                           # בדיקות (מסד נפרד: RequestsDb_Tests)

cd client
npm install
npm start                                                             # הלקוח: http://localhost:4200
```

## מבנה הפתרון

```
server/
├── RequestsManagement.API    ← Controllers, טיפול שגיאות מרכזי, הגדרות DI
├── RequestsManagement.BL     ← לוגיקה עסקית ו-Cache. בלי EF
├── RequestsManagement.DAL    ← Entities, DbContext, Migrations, Repository, Seed
├── RequestsManagement.DTO    ← מה שעובר בין השכבות ואל הלקוח
└── RequestsManagement.Tests  ← Unit + Integration
client/src/app/
├── components/   ← requests-page (מצב + שרת), requests-filters, requests-table, requests-stats, request-history
├── services/     ← קריאות ל-API, המשתמש הנוכחי
├── interceptors/ ← מוסיף X-User לכל בקשה
└── models/, enums/
```

- **שרת:** התלויות חד‑כיווניות (API ← BL ← DAL ← DTO), וכל שכבה היא פרויקט נפרד, כך שהקומפיילר אוכף את ההפרדה.
- **לקוח:** רק `requests-page` מדברת עם השרת. שאר הקומפוננטות מציגות ומדווחות על פעולות.

## ה‑API

| בקשה | מה עושה |
|---|---|
| `GET /api/requests` | עמוד של פניות: סינון, חיפוש (`search`), מיון, דפדוף (`pageSize` עד 100) |
| `GET /api/requests/stats` | ספירה לפי סטטוס ולפי עדיפות, עם אותם פילטרים |
| `PATCH /api/requests/{id}/status` | עדכון סטטוס. 400 מעבר אסור, 404, 409 התנגשות |
| `PATCH /api/requests/status` | עדכון עד 100 פניות (Partial Success) |
| `GET /api/requests/{id}/history` | היסטוריית שינויי הסטטוס |

שגיאות מוחזרות כ‑ProblemDetails. שגיאה לא צפויה מחזירה 500 עם `traceId`, בלי פרטים פנימיים.

## בסיס הנתונים והאינדקסים

**SQL Server עם EF Core (Code‑First).** הנתונים טבלאיים, עם סינון משולב, `GROUP BY` וטרנזקציה לעדכון + היסטוריה. `rowversion` נותן זיהוי התנגשויות מובנה. **חלופה:** MongoDB, שגמישות המבנה שלו לא נדרשת כאן.

| אינדקס | התרחיש |
|---|---|
| `(CreatedAt)` | מסך הפתיחה, טווח תאריכים |
| `(Status, CreatedAt)` | סינון וסיכומים לפי סטטוס |
| `(OrganizationName, CreatedAt)` INCLUDE `(Status, Priority)` | פניות של ארגון |
| `(AssignedTo, CreatedAt)` INCLUDE `(Status, Priority)` | פניות של מטפל |
| `(RequestId, ChangedAt)` בהיסטוריה | ההיסטוריה של פנייה |

- כל אינדקס מתחיל בעמודת הסינון ומסתיים ב‑`CreatedAt` (המיון), כך שהמסד קופץ לשורות ומקבל אותן ממוינות.
- **INCLUDE** מאפשר לבדוק סטטוס ועדיפות בתוך האינדקס: סינון משולב ירד מ‑5,029 ל‑176 דפים.
- **אין טעינה מלאה לזיכרון:** הסינון, המיון והדפדוף נשלחים כ‑SQL אחד, ולשרת מגיעות רק שורות העמוד.

## Concurrency ו‑Bulk Update

- **Concurrency:** לכל פנייה יש `RowVersion`, שהלקוח שולח חזרה בעדכון. אם הגרסה השתנתה בינתיים, חוזר **409**. השינוי ושורת ההיסטוריה נשמרים באותה טרנזקציה.
- **Bulk:** עד 100 פניות. כל פנייה נבדקת ונשמרת בנפרד, עם אותם כללים כמו בעדכון בודד. התשובה: `{ succeeded, failed }`, עם סיבה לכל כישלון.

## Cache ו‑Invalidation

- **מה נשמר:** הסיכומים בלבד. הם יקרים יחסית ונקראים בכל טעינה. הרשימה לא נשמרת, כי יש לה אינסוף שילובים והיא מהירה בזכות האינדקסים.
- **איך:** Decorator (`CachedRequestService`) מעל ה‑Service. המפתח: הפילטרים + **מספר גרסה**. כל עדכון מוצלח מעלה את הגרסה, וכך כל הסיכומים הקודמים מתבטלים בפעולה אחת. בנוסף, Expiration של 60 שניות.
- **בכמה מופעי שרת:** `IMemoryCache` נפרד לכל שרת, ועדכון בשרת אחד לא מבטל את השאר. **הפתרון:** Redis, עם מספר הגרסה ב‑Redis (`INCR`).

## Connection String

ב‑`appsettings.json`, מול LocalDB עם משתמש Windows, בלי סיסמה. להרצה מול שרת אחר: משתנה הסביבה `ConnectionStrings__DefaultConnection`. בייצור: משתני סביבה או Key Vault.

## בדיקות

- **Unit:** כללי המעברים בין סטטוסים, Validation של Bulk.
- **Integration** (שרת אמיתי + מסד נפרד): סינון ומיון, עדכון + היסטוריה, 409, 400, Cache אחרי עדכון, Bulk מעורב.

## החלטות טכנולוגיות

**1. Seed עם `SqlBulkCopy`.** טעינה של 100,000 פניות לוקחת **כ‑2-6 שניות** (תלוי במחשב ובבניית המסד), לעומת **14 שניות** עם EF. **חלופה:** `AddRange` של EF. פשוט יותר, אבל איטי בכמה מונים. שתי השיטות מומשו ונמדדו, וגרסת EF הוסרה אחרי המדידה.

**2. Bulk עם Partial Success.** הפניות עצמאיות, ולכן כישלון של אחת (התנגשות, מעבר אסור) לא צריך לבטל את השאר. **חלופה:** "הכול או כלום" בטרנזקציה אחת. מתאים כשהפריטים תלויים זה בזה, אבל כאן היה מבטל 99 עדכונים תקינים בגלל אחד.

## מגבלה ושיפור

**מגבלה:** החיפוש (`LIKE '%term%'`) לא יכול להשתמש באינדקס, ולכן סורק את כל הטבלה (~0.5 שנייה). פירוט ב‑[docs/performance.md](docs/performance.md).

**שיפור:** Full‑Text Search של SQL Server, שהופך את החיפוש ל‑Seek. לא מומש כי LocalDB לא תומך בו.

## שימוש ב‑AI

עוזר AI (Claude) היה מעורב בכתיבת כל שלבי הקוד והתיעוד. כל כתיבה נעשתה רק אחרי שאישרתי אותה.

בסוף כל שלב פיתוח בוצעו:
- בנייה בלי אזהרות.
- הרצת הבדיקות האוטומטיות, ובדיקה שהן נכשלות כשהקוד שבור.
- בדיקה מול ה‑API של התרחישים (200, 400, 404, 409, Bulk מעורב).
- בדיקה ידנית בדפדפן.
