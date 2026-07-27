# DevPulse Web

Vue 3 (Vite) single-page dashboard for the DevPulse analytics platform.

## Design direction

Developer-tool aesthetic (Grafana / GitHub / Linear), not a generic AI-product
look: dark-first, flat surfaces, sharp corners, monospace for identifiers and
metrics, no gradients/glow/glassmorphism. Tokens live in
`src/assets/styles/tokens.css`.

## Structure

Mirrors the backend's domain split rather than a flat `components/` bucket —
each bounded context (users, repositories, search, dashboard) owns its pages,
and cross-cutting concerns are separated into their own layers:

```
src/
├── api/            - one file per backend domain (users, repositories,
│                     commits, pull-requests, search), thin wrappers over
│                     the 24 REST endpoints; http.js also camelizes the
│                     backend's snake_case JSON so components never see it
├── stores/         - Pinia stores, one per domain
├── layouts/         - app shell: sidebar (nav + live service status),
│                      topbar (global search)
├── modules/         - feature pages, grouped by domain
│   ├── dashboard/
│   ├── repositories/
│   ├── users/
│   └── search/
├── components/common/ - shared presentational components (PanelCard,
│                        MetricStat, StatusPill, ProviderBadge)
└── router/          - route table
```

## Running

```bash
npm install
npm run dev      # proxies /api -> http://localhost:5000 (see vite.config.js)
```

The dev server never talks to the backend directly from the browser — all
`/api/*` requests go through Vite's dev-server proxy (`server.proxy` in
`vite.config.js`), which forwards to `http://localhost:5000`. Point that at
the real API with `kubectl -n devpulse port-forward svc/devpulse-api 5000:80`
(EKS) or a local `dotnet run`. Set `VITE_API_BASE_URL` (see `.env.example`)
only if you need to bypass the proxy entirely (e.g. building for a specific
deployed API host).

## Status

Every page is wired to the live API via Pinia stores — no mock data. List
endpoints return `{ totalCount, items }`; stores unwrap `items`. Repository
health score / PR count / contributor count come from the separate
`/repositories/{id}/metrics` endpoint, not the list endpoint, to avoid N+1
calls on the dashboard.
