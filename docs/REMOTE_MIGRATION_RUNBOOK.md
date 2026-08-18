# Remote DB Migration Runbook (Team Safe)

This runbook applies to the migration:
- `20260416052050_AddRequestWorkflowStagesAndAssetReservation`

Use this process when your team shares one remote MariaDB/MySQL database.

## 1) Preconditions
- Migration is merged to the main branch and reviewed.
- Confirm no pending migration conflicts:
  - `dotnet ef migrations list`
- Confirm target DB connection belongs to the expected environment.
- Announce maintenance window in team channel.

## 2) Backup First (Required)
- Create a DB snapshot or full SQL backup before schema changes.
- Keep rollback owner identified (who can restore backup).

## 3) Generate Idempotent Script
Generate SQL from the repo (safe to run across environments):

```powershell
Set-Location "AssuraBackend\src\Assura.API"
dotnet ef migrations script --idempotent \
  --project ..\Assura.Infrastructure\Assura.Infrastructure.csproj \
  --startup-project .\Assura.API.csproj \
  --output ..\..\docs\migrations\20260416_request_workflow_idempotent.sql
```

Why idempotent: it checks migration history and only applies missing migrations.

## 4) Review Script
- Verify only intended objects are changed.
- Confirm columns expected by workflow are present:
  - Requests: `RequiresDivisionHeadApproval`, `DivisionHeadReviewerId`, `DivisionHeadReviewedAt`, `StorekeeperProcessorId`, `StorekeeperProcessedAt`, `TemporarilyAssignedAt`, `PickupConfirmedAt`
  - Assets: `ReservedForUserId`, `ReservedByRequestId`, `ReservedUntilUtc`

## 5) Apply to Remote DB
Use DB admin tooling (pipeline, DBA console, or approved SQL client) to run:
- `docs/migrations/20260416_request_workflow_idempotent.sql`

Do not apply ad-hoc from multiple developer laptops simultaneously.

## 6) Post-Apply Verification
- Confirm migration record exists in migration history table.
- Smoke test API endpoints:
  - `GET /api/requests/{id}/suggested-assets`
  - `POST /api/requests/{id}/division-head-review`
  - `POST /api/requests/{id}/process`
  - `POST /api/requests/{id}/confirm-temporary-assignment`
- Validate dashboard KPI fields:
  - `temporaryAssignedAssets`, `awaitingPickupConfirmations`, `procurementEscalations`

## 7) Rollback Plan
- If critical issue appears:
  - Stop writes to affected flows.
  - Restore DB from pre-migration backup.
  - Create fix migration and re-run process.

## 8) Team Checklist
- [ ] Backup created
- [ ] Script generated
- [ ] Script reviewed
- [ ] Migration applied once
- [ ] End-to-end smoke tests passed
- [ ] Team notified completion
