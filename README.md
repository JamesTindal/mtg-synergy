# 🔮 MTG Synergy Finder

A web application for discovering synergistic Magic: The Gathering cards based on underlying **game mechanics** (tribal alignment, card types, colors, enablers, and payoffs) rather than crowdsourced decklists.

The MVP focuses on the **Innistrad: Crimson Vow (`VOW`)** set with an extensible, rule-based architecture.

---

## 🛠 Tech Stack

* **Backend:** .NET 10 Web API (`net10.0`, C#)
* **Database:** PostgreSQL 17 via Docker Compose
* **Frontend:** React 19 + TypeScript + Vite + Plain CSS
* **Card Ingestion:** Scryfall API (`https://api.scryfall.com/`)
* **Testing:** xUnit test suite for synergy rules

---

## 🚀 Quick Start

### 1. Start PostgreSQL (Docker)
```bash
docker compose up -d
```
Runs PostgreSQL 17 on `localhost:5432` with database `mtg_synergy`.

### 2. Run the .NET 10 Backend API
```bash
cd backend/src/MtgSynergy.Api
dotnet run
```
The API starts at `http://localhost:5000`. On first run, it automatically ensures the database tables are created.

### 3. Sync Cards from Scryfall
To populate the database with Crimson Vow (`vow`):
```bash
curl -X POST http://localhost:5000/api/admin/sync/vow
```

### 4. Run the React Frontend
```bash
cd frontend
npm install
npm run dev
```
Open [http://localhost:5173](http://localhost:5173) in your browser.

---

## 🏛 Clean Architecture Overview

The solution adheres strictly to Clean Architecture principles with clear separation of concerns and dependency inversion:

```
backend/
├── src/
│   ├── MtgSynergy.Domain/         # Enterprise Core: Entities (Card), Domain Synergy Rules, Exceptions
│   ├── MtgSynergy.Application/    # Use Cases: ICardRepository, ISynergyEngine, CardSynergyService, DTOs
│   ├── MtgSynergy.Infrastructure/ # External Concerns: EF Core AppDbContext, CardRepository, ScryfallClient
│   └── MtgSynergy.Api/            # Presentation: Modular Minimal Endpoints, Composition Root
└── tests/
    └── MtgSynergy.Tests/          # Unit tests for Domain Rules and Application Services

frontend/
├── src/
│   ├── services/api.ts            # Isolated API client layer
│   ├── hooks/                     # Custom React hooks (useCardSearch, useCardSynergies)
│   ├── components/                # Decoupled UI components (SearchBar, Spotlight, SynergyCard)
│   ├── types.ts                   # Domain & API TypeScript interfaces
│   └── styles.css                 # Plain CSS theme
```

---

## 🧠 Extensible Synergy Engine Architecture

The synergy engine is built around a pluggable rule pipeline implementing `ISynergyRule`:

```csharp
public interface ISynergyRule
{
    string RuleName { get; }
    int Priority { get; }
    SynergyContribution? Evaluate(Card sourceCard, Card candidateCard);
}
```

### Active MVP Rules:
1. **`ColorSynergyRule`:** Identifies exact color identity matches, partial overlaps, and universal colorless cards.
2. **`TribalSynergyRule`:** Detects shared creature tribes (Vampire, Zombie, Werewolf, etc.) and awards significant bonuses when card oracle text contains tribal payoffs (e.g. *"Target Vampire you control deals damage equal to its power"*).
3. **`CardTypeSynergyRule`:** Rewards natural cross-type interactions (Creatures + Auras/Equipment, Instants/Sorceries + spell-matters payoffs, Artifacts + artifact-matters).

### Adding Future Set Rules:
To add set mechanics (e.g., Crimson Vow's *Blood*, *Exploit*, *Disturb*, *Training*), create a new rule class implementing `ISynergyRule` and register it in `Program.cs`:
```csharp
builder.Services.AddScoped<ISynergyRule, BloodSynergyRule>();
```

---

## 🧪 Running Tests
```bash
cd backend
dotnet test
```
Runs the xUnit test suite validating rule calculations and engine ranking.
