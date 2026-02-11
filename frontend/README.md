# Frontend (React + Vite)

This directory contains the React single-page app for the React + .NET Starter Kit. It implements registration, login, and a protected dashboard backed by the .NET API.

## Features

- React 19 + TypeScript, powered by Vite for fast reloads.
- Centralized auth context that stores tokens in `localStorage` and injects them into every API request.
- Protected routes (React Router) and example pages for Register, Login, and Dashboard.
- Vitest + Testing Library coverage for pages, auth context, and error paths.

## Local development

```bash
cd frontend
npm install
npm run dev
```

The dev server runs on `http://localhost:5173` by default. Make sure the backend API is running (see the repo root README) and the `.env` file contains `VITE_API_BASE_URL=https://localhost:7042`.

## Scripts

| Command            | Description                                |
| ------------------ | ------------------------------------------ |
| `npm run dev`      | Start the Vite dev server                  |
| `npm run build`    | Type-check and build for production        |
| `npm run test`     | Run Vitest in watch/run mode               |
| `npm run lint`     | Run ESLint with the configured flat config |

## Project structure

```
src/
  api/          // API client + typed helpers
  auth/         // AuthProvider, hooks, and guards
  pages/        // Register, Login, Dashboard
  __tests__/    // Vitest suites
```

Feel free to swap the styling system, add new routes, or plug in your preferred component library - the starter keeps the structure minimal on purpose.



