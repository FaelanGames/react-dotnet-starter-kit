# React + .NET Starter Kit
[![CI](https://github.com/FaelanGames/react-dotnet-starter-kit/actions/workflows/ci.yml/badge.svg)](https://github.com/FaelanGames/react-dotnet-starter-kit/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/FaelanGames/react-dotnet-starter-kit/branch/main/graph/badge.svg)](https://codecov.io/gh/FaelanGames/react-dotnet-starter-kit)
[![Support on Ko-fi](https://img.shields.io/badge/Support-Ko--fi-orange?logo=ko-fi)](https://ko-fi.com/faelangames)


A production‑ready **full‑stack starter kit** using **React + TypeScript (Vite)** and **.NET 10 Web API**, with **JWT authentication**, **SQLite**, **tests**, and **CI** already set up.

> The goal: clone the repo and have a real, authenticated full‑stack app running locally in **under 30 minutes** — without yak‑shaving.

---

## What this is

This repository is a **clean, opinionated baseline** for building full‑stack applications with React and .NET.

It gives you:

* a working authentication flow (register / login)
* a protected API endpoint
* a protected frontend route
* tests on both sides
* GitHub Actions CI

So you can focus on **building features**, not setting up infrastructure.

---

## What’s included

### Backend (.NET 10)

* .NET 10 Web API
* SQLite database (zero setup)
* Entity Framework Core
* JWT authentication
* Password hashing (PBKDF2)
* Example protected endpoint: `GET /api/users/me`
* Swagger with JWT support
* Automatic DB migrations on startup
* Rotating refresh tokens with `/api/auth/refresh` + `/api/auth/logout`

### Frontend (React)

* React + TypeScript
* Vite
* React Router
* Auth context with token + refresh token persistence
* Protected routes
* API client with automatic auth headers and refresh handling
* Example pages:

  * Register
  * Login
  * Dashboard (protected)

### Tooling

* Frontend tests: Vitest + Testing Library
* Backend tests (if added)
* GitHub Actions CI
* Clean monorepo structure

---

## Repository structure

```
starter-kit/
├─ backend/          # .NET 10 Web API
├─ frontend/         # React + Vite app
└─ .github/
   └─ workflows/     # CI configuration
```

---

## Prerequisites

Make sure you have the following installed:

* **Node.js 20+**
* **.NET SDK 10.0**
* Git

> No Docker required for local development.

---

## Quick start

### 1️⃣ Clone the repository

```bash
git clone https://github.com/FaelanGames/react-dotnet-starter-kit
cd react-dotnet-starter-kit
```

---

### 2️⃣ Start the backend

```bash
cd backend
dotnet run --project src/StarterKit.Api
```

The API will start on:

```
https://localhost:7042
```

Swagger UI:

```
https://localhost:7042/swagger
```

> On first run, the SQLite database is created automatically and migrations are applied.

---

### 3️⃣ Configure frontend environment

Create a file:

```
frontend/.env
```

With the following content:

```bash
VITE_API_BASE_URL=https://localhost:7042
```

---

### 4️⃣ Start the frontend

```bash
cd frontend
npm install
npm run dev
```

The app will be available at:

```
http://localhost:5173
```

---

### 5️⃣ Try the full flow

1. Open the frontend in your browser
2. Register a new account
3. You’ll be redirected to the dashboard
4. The dashboard calls `GET /api/users/me`
5. Log out and confirm protected routes redirect correctly

You now have a **fully working full‑stack app with authentication**.

---

## Running tests

### Frontend tests

```bash
cd frontend
npm run test
```

### Backend tests

```bash
cd backend
dotnet test
```

---

## Coverage

You can generate coverage for both backend and frontend from the repo root:

```bash
npm run test:coverage
```

This runs:

* backend coverage with Coverlet (Cobertura output + minimum total line threshold)
* frontend coverage with Vitest v8 provider

### Backend coverage report (HTML)

```bash
npm run coverage:report:backend
```

Open:

* `backend/TestResults/CoverageReport/index.html`

### Frontend coverage report (HTML)

Open:

* `frontend/coverage/index.html`

### PowerShell helper

You can also run:

```powershell
./scripts/coverage.ps1
```

---

## Showing coverage on GitHub

Yes, coverage can be shown on the GitHub page.

Two common options:

* Upload CI artifacts only (already configured in `.github/workflows/ci.yml`) so reports are downloadable per run.
* Publish to a service like Codecov/Coveralls and add a badge to the top of this README (Codecov upload is now configured in CI).

Example Codecov badge (replace `<owner>` and `<repo>`):

```md
[![codecov](https://codecov.io/gh/<owner>/<repo>/branch/main/graph/badge.svg)](https://codecov.io/gh/<owner>/<repo>)
```

---

## Environment variables

### Frontend

| Variable            | Description                 |
| ------------------- | --------------------------- |
| `VITE_API_BASE_URL` | Base URL of the backend API |

### Backend

Configured in `appsettings.json`:

* JWT issuer / audience
* JWT signing key
* SQLite connection string

> Warning: Never commit real JWT signing keys to source control. During development you can keep secrets in [.NET user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):
>
> ```bash
> cd backend
> dotnet user-secrets init
> dotnet user-secrets set "Jwt:SigningKey" "generate-a-long-random-value-here"
> ```
>
> In production, set the same `Jwt:*` keys as environment variables (for example `Jwt__SigningKey`) so the existing `builder.Configuration` binding picks them up automatically.

---

## Authentication overview

* Authentication uses **JWT Bearer tokens** (default 60-minute expiry).
* Refresh tokens are stored (hashed) in SQLite, rotated on every `/api/auth/refresh`, and revoked via `/api/auth/logout`.
* The frontend keeps both tokens in `localStorage`, automatically calling the refresh endpoint if the API returns 401.
* API endpoints are protected using `[Authorize]`.

This setup is intentionally **simple and extensible** — suitable for:

* side projects
* internal tools
* SaaS prototypes

---

## Why these choices?

* **Monorepo** → easier onboarding
* **SQLite** → zero setup, fewer moving parts
* **JWT** → clear, explicit auth flow
* **Vite** → fast dev feedback
* **Minimal abstractions** → easy to understand and modify

This is a foundation, not a framework.

---

## Extending the project

Next steps:

* Add roles / permissions
* Swap SQLite for Postgres
* Add feature-based folders
* Add E2E tests

The codebase is structured to support all of the above without rewrites.

---

## License

This project is provided under a MIT license.
See the LICENSE file for full terms.

---

## Who this is for

* Developers who want a clean starting point
* Freelancers spinning up client projects
* Indie hackers building MVPs
* Teams tired of re‑implementing auth

If that’s you — this repo is meant to save you time.

---

## Support the project

If this starter kit saved you time, helped you learn, or gave you a solid foundation for your project, you can support future work here:

☕ https://ko-fi.com/faelangames

Support is completely optional — but always appreciated.
