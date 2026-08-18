# SAFE_FIX_BASELINE_AssetQrCodeMapping

Baseline captured before fixing the Medium-severity bug found by `/test-mobile-app`
(2026-08-17): `GetAssetsQuery`/`GetAssetByIdQuery` never returned the QR image
`CreateAssetCommand` generates and persists.

## Blast radius identified

`CreateAssetCommand.cs` correctly maps `QrCode = a.QrCode` on creation, but three
other hand-rolled `AssetDto` projections in the same feature omitted it — the same
copy-pasted projection duplicated with the field missing each time:

- `GetAssetsQuery.cs` (`GET /api/Assets`) — the bug as originally reported.
- `GetAssetByIdQuery.cs` (`GET /api/Assets/{id}`) — the bug as originally reported.
- `CheckinAssetCommand.cs` (`POST /api/Assets/{id}/checkin`) — same gap, found while
  mapping the blast radius; in scope since it's the identical root cause.
- `UpdateAssetCommand.cs` (`PUT /api/Assets/{id}`) — same gap, same reasoning.

Consumers confirmed via grep, unaffected by code changes (additive field only):
- Mobile: `AssetService.fetchAssets()`/`updateAssetStatus()` — doesn't read `qrCode`
  today, unaffected either way.
- Web frontend: `asset-details.ts` (`if (a.qrCode?.trim())`, else client-generates a
  QR encoding the asset code) and `assets.ts`'s bulk QR print modal (same fallback
  pattern) — both already handle a real `qrCode` correctly; they'll now use the real
  backend-generated image instead of silently falling back, which is the intended
  fix, not a behavior regression (both encode the same asset code either way).

## Before (baseline)

- `dotnet build AssuraBackend.sln`: clean, 0 errors.
- `dotnet test AssuraBackend.sln`: 274/274 passing (Domain 1, Infrastructure 4, API 70,
  Application 199).
- Live (real backend, seeded `admin`/`Password@123`, a freshly created asset with a
  real generated QR image via `POST /api/Assets`):
  - `GET /api/Assets/{id}` → `"qrCode":null`
  - `GET /api/Assets` (same asset in the list) → `"qrCode":null`
  - `PUT /api/Assets/{id}` response → `"qrCode":null`
  - (all reproduced against the same live asset, id 231, created for this test)

## After (post-fix)

- `dotnet build AssuraBackend.sln`: clean, 0 errors.
- `dotnet test AssuraBackend.sln`: 276/276 passing (274 baseline + 2 new
  `AssetQrCodeMappingTests`).
- Live, same asset (id 231):
  - `GET /api/Assets/{id}` → real Base64 PNG data.
  - `GET /api/Assets` (list) → same asset entry now carries the real Base64 PNG data.
  - `PUT /api/Assets/{id}` response → real Base64 PNG data.
  - `POST /api/Assets/{id}/checkin` response → real Base64 PNG data (checked out to a
    user, then checked back in, to reach a checkin-eligible state).
  - Test asset 231 deleted after verification.

## Comparison

| Check | Before | After | Result |
|---|---|---|---|
| `dotnet build` | Clean | Clean | Unaffected |
| `dotnet test` full solution | 274/274 | 276/276 | Fixed (+2 new), no regressions |
| `GET /api/Assets/{id}` qrCode | `null` | Real PNG data | Fixed |
| `GET /api/Assets` qrCode | `null` | Real PNG data | Fixed |
| `PUT /api/Assets/{id}` qrCode | `null` | Real PNG data | Fixed |
| `POST /api/Assets/{id}/checkin` qrCode | `null` | Real PNG data | Fixed |
| `POST /api/Assets` (create) qrCode | Real PNG data | Real PNG data | Unaffected (already correct) |

No Pass → Fail regressions found.
