# පද්ධතියේ තාක්ෂණික ගැටලු සඳහා විසඳුම් (Technical Debt Resolution)

මෙම ගොනුව මඟින් හඳුනාගත් තාක්ෂණික ගැටලු 5 සඳහා පද්ධතිය තුළ සිදුකළ ප්‍රධාන සංශෝධන විස්තර කෙරේ.
This document outlines the resolutions for the 5 identified technical debt items in the system.

---

### 1. SQL Server vs MySQL Migration Mismatch
**Sinhala:** මීට පෙර තිබූ migrations, SQL Server සඳහා පමණක් ගැලපෙන ලෙස සකස් වී තිබුණි (උදාහරණ: `nvarchar`, `datetime2`). මම එම පැරණි migrations ඉවත් කර, MySQL සඳහා ගැලපෙන පරිදි අලුත්ම `InitialCreate` migration එකක් සකස් කළා.
**English:** Previous migrations were configured specifically for SQL Server (e.g., using `nvarchar`, `datetime2`). These have been removed and replaced with a fresh `InitialCreate` migration compatible with MySQL.

### 2. FluentValidation Pipeline Behavior
**Sinhala:** පද්ධතිය තුළ validators තිබුණද, ඒවා MediatR හරහා ස්වයංක්‍රීයව ක්‍රියාත්මක වීමට අවශ්‍ය pipeline behavior එක සකසා නොතිබුණි. `ValidationBehavior` නමින් නව class එකක් සකසා එය MediatR pipeline එකට ඇතුළත් කළා.
**English:** While validators existed, they weren't executing automatically. A new `ValidationBehavior` has been implemented and registered in the MediatR pipeline to ensure all requests are validated before reaching their handlers.

### 3. JWT Based User Identification
**Sinhala:** කලින් audit logs වල "CreatedBy" සහ "UpdatedBy" සඳහා ස්ථාවරව "System" ලෙස සටහන් විය. `CurrentUserService` එකක් මඟින් JWT token එකේ ඇති පරිශීලක ID එක ලබාගෙන එය database එකට ඇතුළත් කිරීමට පියවර ගත්තා.
**English:** Replaced the hardcoded "System" string in audit fields with the actual user ID extracted from JWT claims via a new `CurrentUserService`. This ensures proper tracking of data changes.

### 4. Test Project Setup & Updates
**Sinhala:** පද්ධතියේ ආරක්ෂාව සහ නිවැරදි බව පරීක්ෂා කිරීමට `ValidationBehavior` සඳහා unit tests කිහිපයක් `Assura.Application.Tests` ව්‍යාපෘතියට අලුතින් එක් කළා.
**English:** Added unit tests for the `ValidationBehavior` within the `Assura.Application.Tests` project to verify the validation logic and ensure the test suite is functional.

### 5. CORS and Exception Handling (Production Readiness)
**Sinhala:** CORS සඳහා ඕනෑම වෙබ් අඩවියකට (AllowAnyOrigin) අවසර දීම වෙනුවට නිශ්චිත අඩවි වලට පමණක් අවසර දෙන ලෙස සීමා කළා. එමෙන්ම සිදුවන දෝෂ (Exceptions) පිළිබඳ සියලු විස්තර Production වලදී පිටතට නොපෙන්වන ලෙස Exception Middleware එක සංශෝධනය කළා.
**English:** Hardened the system for production by restricting CORS to specific `ALLOWED_ORIGINS` and updating the `ExceptionMiddleware` to hide sensitive internal error details in non-Development environments.

---

### Repository Working Instructions (පද්ධතිය සමඟ වැඩ කිරීමේ උපදෙස්)

#### 1. Database Migrations (දත්ත සමුදාය යාවත්කාලීන කිරීම)
**English:** Since old migrations were deleted, you need to update your local database to match the new schema.
**Sinhala:** පැරණි migrations ඉවත් කර ඇති බැවින්, නව සැලැස්මට අනුව ඔබේ පරිගණකයේ දත්ත සමුදාය යාවත්කාලීන කළ යුතුය.

**Command:**
```powershell
dotnet ef database update --project src/Assura.Infrastructure --startup-project src/Assura.API
```

#### 2. Environment Configuration (පරිසර සැකසුම්)
**English:** A new key `ALLOWED_ORIGINS` is required in your `.env` or `appsettings.json` for CORS.
**Sinhala:** CORS ආරක්ෂාව සඳහා ඔබගේ `.env` හෝ `appsettings.json` ගොනුවට `ALLOWED_ORIGINS` යන අගය ඇතුළත් කළ යුතුය.

**Example (.env):**
```env
ALLOWED_ORIGINS=http://localhost:4200,https://yourdomain.com
```

#### 3. Running Tests (පරීක්ෂණ ක්‍රියාත්මක කිරීම)
**English:** Use the following command to run the newly added unit tests.
**Sinhala:** අලුතින් එක් කරන ලද unit tests ක්‍රියාත්මක කිරීමට පහත විධානය පාවිච්චි කරන්න.

**Command:**
```powershell
dotnet test
```

#### 4. Automatic Validation (ස්වයංක්‍රීය වලංගුකරණය)
**English:** No manual validation code is needed in handlers. Just create a `Validator` class inheriting from `AbstractValidator<T>`, and it will run automatically.
**Sinhala:** Handler එක තුළ අතින් validation කේත ලිවීමට අවශ්‍ය නැත. `AbstractValidator<T>` හරහා `Validator` පන්තියක් සකස් කළ පමණින් එය ස්වයංක්‍රීයව ක්‍රියාත්මක වේ.

#### 5. Syncing with Main Branch (Main ශාඛාව සමඟ යාවත්කාලීන වීම)
**English:** To get the latest changes from `main` into your feature branch, follow these steps:
**Sinhala:** `main` ශාඛාවේ ඇති නවතම වෙනස්කම් ඔබේ ශාඛාවට (feature branch) ලබා ගැනීමට පහත පියවර අනුගමනය කරන්න:

**Commands:**
```powershell
# 1. Commit or stash your current changes
# 1. ඔබේ current branch එකේ ඇති දත්ත commit හෝ stash කරන්න

# 2. Fetch the latest changes from the main branch
# 2. Main ශාඛාවේ අලුත්ම දත්ත ලබාගන්න
git fetch origin main

# 3. Merge the main branch into your current branch
# 3. Main ශාඛාව ඔබේ ශාඛාව සමඟ ඒකාබද්ධ (merge) කරන්න
git merge origin/main
```
