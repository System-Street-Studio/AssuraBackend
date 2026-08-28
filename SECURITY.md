# Security remediation record

## Leaked credentials (found 2026-08-23, during AWS deployment planning)

The following were committed to this repository's git history with real, working-looking values:

| Item | Where | Status |
|---|---|---|
| MySQL host/user/password for a free hosted DB (`db45494.public.databaseasp.net`) | `src/Assura.API/appsettings.json`, `.env.example`, and a duplicate committed build-output copy at `tmp/build-check/appsettings.json` | Values scrubbed from the working tree in this remediation pass. **The password itself must still be rotated at the hosting provider — that cannot be done from this repo.** |
| ~30 plaintext seed-user passwords (employee/division-head/core test accounts) | `credentials.md` | Untracked from git (`git rm --cached`); local file kept on disk for reference, `.gitignore`d going forward. |
| A 26,899-line raw EF Core debug log (SQL queries incl. password-hash lookups, auth flow traces) | `src/Assura.API/dotnet_log.txt` | Untracked from git; `.gitignore`d. |
| A test data-transfer JSON dump | `transfer_data.json` | Untracked from git; `.gitignore`d. |
| A full committed build-output directory (compiled DLLs + another `appsettings.json` copy) | `tmp/build-check/` | Untracked from git; `.gitignore`d. |
| Several ad-hoc one-off console projects/scripts not part of `AssuraBackend.sln` | `dbfix/`, `tmp_seed_core/`, `src/tmp/`, `tmp/`, `check_db.cs`, `DbCheck.csx`, `db_script.sql`, `backend_log.txt`, `restore_error.txt` | Untracked from git; `.gitignore`d (also excluded from the Docker build context via `.dockerignore`). |

## Required follow-up (cannot be done from inside this repo)

1. **Rotate the MySQL password** on the `db45494.public.databaseasp.net` host (or retire that free-tier DB entirely in favor of the AWS RDS instance from the deployment plan). Treat the old password as burned — assume it has been seen even though the repo was never intentionally made public.
2. **Rotate the JWT signing key.** `Jwt:Key` in `appsettings.json` is now empty; a real value must be supplied via the `JWT_SECRET_KEY` environment variable (or AWS Secrets Manager once deployed) before the API will start. Generate a new one with `openssl rand -base64 32` — do not reuse the old placeholder value that was committed.
3. Until both of the above are done, **the app will not start with a real DB connection or valid JWT signing** unless `DB_*`/`JWT_SECRET_KEY` environment variables are set — this is intentional (fail closed, not fail open on a known-weak default).

## Git history

The commands above only stop these values from being committed *again* — they are still present in this repo's git history. Two real options, not silently picked:
- **Rewrite history** (`git filter-repo` targeting the old blobs of `appsettings.json`, `.env.example`, `credentials.md`, `tmp/build-check/`, `dotnet_log.txt`) to remove them retroactively. Real cost: rewrites every commit SHA after the earliest affected commit, requires a force-push, and breaks any existing clones/forks/CI caches that reference the old history.
- **Rotate and move on.** Since the credentials are being rotated anyway (see above), the historical blobs stop being a live risk — they're just old, dead values. For a small/solo-maintained repo, this is a defensible, lower-disruption choice.

No history rewrite has been performed as part of this pass. Decide and record the choice here before making the repo public or handing it to additional collaborators.

## Gate going forward

- `.gitleaks.toml` + `.pre-commit-config.yaml` added — run `pre-commit install` locally so new secrets are caught before they're committed.
- The Jenkins pipeline (see deployment plan) runs `gitleaks detect` as its first stage, so a secret that bypasses the local hook still fails CI before any build compute is spent.

## Other findings noted, not fixed here (out of scope for this pass)

- `SeedController` (`src/Assura.API/Controllers/SeedController.cs`) exposes destructive data-seeding/reset endpoints, including raw `ExecuteSqlRawAsync` calls. It is already gated to `Admin`/`SystemAdmin` via `[Authorize]`, so it is not an open vulnerability, but it's still reachable in any environment where those roles exist — worth considering whether it should be compiled out of production builds entirely (e.g. `#if DEBUG`) rather than relying solely on role-based access control.
