# ASMX-Reversi

A Reversi (Othello) game built with an Angular frontend and a .NET backend — originally powered by ASMX web services, now modernized to a .NET 10 minimal API with an AI opponent, Docker containers, and Azure deployment.

---

## Project Overview

ASMX-Reversi is a fully playable Reversi/Othello board game. You can play against another human (pass-and-play) or challenge an AI opponent at three difficulty levels. The frontend is built in Angular; the backend is a .NET 10 REST API that manages board state, move validation, and the AI engine.

## The Rabbit Hole — How This Started

This project began as a delightfully impractical experiment, documented in Jason Young's blog post *[Reversi with ASP.NET ASMX: A Rabbit Hole Story](https://medium.com/@justjason24/reversi-with-asp-net-asmx-a-rabbit-hole-story-cbea5b9a148e)*. Inspired by early-2000s movies about disconnecting from technology, Jason removed most apps from his phone and rediscovered boredom. While waiting for coffee one day, he picked up an old CS textbook — Drake's *Data Structures and Algorithms in Java* — which had readers implement Reversi in Chapter 1. Around the same time, he was browsing StackOverflow from question #1 and stumbled onto question #16 about ASMX, an antiquated .NET web service technology built on XML and SOAP. Naturally, he combined the two ideas: build Reversi with an ASMX backend.

The technical journey involved setting up an ASMX web service in Visual Studio with .NET Framework and XML SOAP envelopes, then building an Angular frontend to consume it — fighting CORS errors and parsing XML responses along the way. The Reversi game logic lived in a `ReversiBoard` class passed back and forth as XML, with methods for filling the board, flipping pieces in all eight directions, marking eligible moves via "sandwiching" logic, and detecting game-over conditions.

Containerizing the app with Docker proved especially painful since .NET Framework required Windows containers. Jason's conclusion: ASMX isn't terrible, but XML is a clear downgrade from JSON, and the client had to conform rigidly to server expectations. The whole exercise was a fun, educational rabbit hole.

## Squad AI Modernization

This project was modernized using **GitHub Copilot's Squad framework** — an AI team orchestration system that assigns specialist agents to parallelize development work. The team was cast from the *Alien* universe:

| Agent | Role | Scope |
| ------ | ------ | ------ |
| **Dallas** | Lead | Architecture, planning, code review |
| **Lambert** | Frontend Dev | Angular, UI, components |
| **Kane** | Backend Dev | .NET API, game engine, AI |
| **Ash** | Cloud/DevOps | Docker, Azure, CI/CD |
| **Parker** | Tester | xUnit tests, quality |

### Modernization Phases

1. **Backend Modernization** — Kane ported the legacy .NET Framework ASMX backend to a .NET 10 minimal API. Collapsed 8 redundant directional scanning methods into a single parameterized method and fixed a bug in the eligible-moves algorithm.

2. **API Contract** — Kane designed 4 clean JSON REST endpoints (`/api/game/new`, `/api/game/move`, `/api/game/ai-move`, `/api/health`) replacing the XML SOAP interface.

3. **AI Opponent** — Kane implemented a minimax algorithm with alpha-beta pruning for single-player mode. Three difficulty levels: Easy (depth 1, random), Medium (depth 3, positional weights), Hard (depth 5–6, positional + mobility + stability + parity).

4. **Containerization** — Ash replaced Windows containers with Linux multi-stage Docker builds. Frontend uses `node:18-alpine` → `nginx:alpine`. Backend uses .NET 10 SDK → runtime. `docker-compose` orchestrates both with healthchecks.

5. **Azure Infrastructure** — Ash created Bicep modules for Azure Container Apps (ACR, Container Apps Environment, frontend + backend apps) and a GitHub Actions CI/CD pipeline with OIDC auth.

6. **Frontend Rewiring** — Lambert gutted the SOAP/XML service layer and replaced it with typed `HttpClient` JSON calls. Added a game setup screen (PvP vs AI mode selector with difficulty picker) and an AI thinking spinner.

7. **Testing** — Parker wrote 59 xUnit tests covering board logic, AI behavior, and API integration. Found a validation bug (invalid moves returned 200) which Kane fixed.

Agents ran concurrently where possible — Kane handled the backend first, then Ash and Lambert ran in parallel since their work was independent. Parker ran last once the implementation was stable.

## Getting Started

### Prerequisites

- **Docker & Docker Compose** (for containerized run)
- **.NET 10 SDK** (for local backend development)
- **Node.js 18+** (for local frontend development)

### Quick Start (Docker)

```bash
docker-compose up --build
```

- **Frontend:** http://localhost:80
- **API:** http://localhost:8080

### Local Development

```bash
# Backend
cd ReversiApi
dotnet run

# Frontend
cd angular-reversi
npm install
ng serve
```

- **Frontend:** http://localhost:4200 (redirects to `/reversi`)
- **API:** http://localhost:5249

### Routes

| Path | Description |
|------|-------------|
| `/` | Redirects to `/reversi` |
| `/reversi` | Main game board |
| `**` (wildcard) | Redirects to `/reversi` |

### Azure Deployment

Infrastructure is defined in `infra/` using Bicep. CI/CD is handled via `.github/workflows/deploy.yml` and deploys automatically on push to `main`.

---

*Originally built as a rabbit-hole experiment with ASMX. Modernized by a squad of AI agents who, unlike the Nostromo crew, actually survived the mission.*
