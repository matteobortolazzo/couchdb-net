# Project: CouchDB.NET

.NET 10 backend library (`CouchDB.NET`) — a LINQ-enabled .NET SDK for CouchDB.
GitHub (`matteobortolazzo/couchdb-net`) for issue tracking, code, and PRs.

## Stack
- **Language/Runtime**: C# / .NET 10 (`net10.0`), nullable enabled, warnings-as-errors in Debug.
- **Tests**: xUnit + Moq, coverage via coverlet. Unit tests in `tests/CouchDB.Driver.UnitTests`, integration in `tests/CouchDB.Driver.E2ETests`.
- **Project layout**: library in `src/CouchDB.Driver`, tests under `tests/`.

## Build & Test
- Build: `dotnet build`
- Test: `dotnet test`
- Lint/format check: `dotnet format --verify-no-changes`

## Critical Rules
- ALWAYS read relevant `docs/` files when working in their topic area (e.g., `docs/git-workflow.md` before commits/PRs).
- Test-first: integration tests that assert behavior, not implementation details.
- No secrets, credentials, or API keys in code.
- Keep changes well-scoped. 1 issue = 1 PR.
- Use git worktrees for all feature work. Never modify code in the main worktree.
- Respect `Nullable` and `TreatWarningsAsErrors` — code must build clean in Debug.

## Reference Docs
On-demand topic docs live in `docs/` at the repo root. Read the file matching your work area:
- `docs/git-workflow.md` — branching, commits, PRs, versioning

`.claude/rules/` is reserved for files explicitly `@`-imported by this CLAUDE.md (auto-loaded at session start). Don't put reference docs there.
