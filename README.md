# ButterShop - E-Commerce Web API

Proste Web API do zarządzania zamówieniami i produktami, zbudowane w .NET 9 z użyciem Minimal API, Entity Framework Core i PostgreSQL.

## Technologie

- .NET 9 Minimal API
- Entity Framework Core 9 + PostgreSQL
- .NET Aspire (orkiestracja i dev environment)
- Scalar (dokumentacja API / interaktywny klient HTTP)
- GitHub Actions (CI/CD)
- Azure App Service + Azure Database for PostgreSQL (wdrożenie)
- Bicep (Infrastructure as Code)

## Uruchomienie lokalne

### Wymagania
- .NET 9 SDK
- Docker Desktop (wymagany przez Aspire do uruchomienia PostgreSQL)

### Start
```bash
dotnet run --project ButterShop.AppHost
```

Aspire automatycznie uruchomi kontener PostgreSQL i API. Dashboard Aspire będzie dostępny pod adresem wyświetlonym w konsoli.

### Dokumentacja API (Scalar)

Interaktywna dokumentacja API jest dostępna pod adresem:
- Lokalnie: `https://localhost:{port}/scalar`
- Produkcja: `https://buttershop-api.azurewebsites.net/scalar`

Link "Scalar (HTTPS)" jest również widoczny w zakładce Resources w dashboardzie Aspire.

## Endpointy API

### Produkty (`/api/products`)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/api/products` | Lista wszystkich produktów |
| GET | `/api/products/{id}` | Szczegóły produktu |
| POST | `/api/products` | Dodaj nowy produkt |
| PUT | `/api/products/{id}` | Aktualizuj produkt |
| DELETE | `/api/products/{id}` | Usuń produkt |

**Przykład - dodanie produktu:**
```json
POST /api/products
{
  "name": "Laptop",
  "price": 3999.99,
  "description": "Laptop do pracy"
}
```

### Zamówienia (`/api/orders`)

| Metoda | Endpoint | Opis |
|--------|----------|------|
| GET | `/api/orders` | Lista wszystkich zamówień |
| GET | `/api/orders/{id}` | Szczegóły zamówienia |
| POST | `/api/orders` | Utwórz zamówienie |
| PUT | `/api/orders/{id}` | Aktualizuj dane zamówienia |
| POST | `/api/orders/{id}/products` | Dodaj produkt do zamówienia |
| DELETE | `/api/orders/{orderId}/products/{productId}` | Usuń produkt z zamówienia |
| DELETE | `/api/orders/{id}` | Usuń zamówienie |

**Przykład - utworzenie zamówienia:**
```json
POST /api/orders
{
  "customerName": "Jan Kowalski",
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 3, "quantity": 1 }
  ]
}
```

## CI/CD

Pipeline GitHub Actions (`.github/workflows/deploy.yml`) uruchamia się automatycznie po każdym pushu na branch `main`.

### Etapy:
1. **Build** - restore, build i publish aplikacji
2. **Deploy Infra** - wdrożenie infrastruktury Azure za pomocą Bicep
3. **Deploy** - ustawienie connection stringa i wdrożenie aplikacji na Azure App Service

### Wymagane sekrety w GitHub:
- `SHARPSHOPAPISERVICEAPI_SPN` - JSON z danymi Service Principal
- `DB_PASSWORD` - hasło do bazy danych PostgreSQL

## Wdrożenie w Azure

### Wykorzystane usługi:
- **Azure App Service** (plan B1) - hosting aplikacji Web API
- **Azure Database for PostgreSQL Flexible Server** - baza danych
- **Azure Resource Group** (`SharpShopRG`) - grupa zasobów

### Połączenie z wdrożoną aplikacją:
```
https://buttershop-api.azurewebsites.net
```

Przykładowe zapytanie:
```bash
curl https://buttershop-api.azurewebsites.net/api/products
```

Connection string do bazy danych jest ustawiany automatycznie przez pipeline CI/CD.

## Infrastruktura jako kod (Bicep)

Plik `infra/main.bicep` definiuje:
- App Service Plan (Linux, B1)
- Web App (.NET 9)
- PostgreSQL Flexible Server (Burstable B1ms)
- Bazę danych `shopdb`
- Regułę firewalla dla usług Azure

Wdrożenie Bicep odbywa się automatycznie z poziomu pipeline'a GitHub Actions.

## Struktura projektu

```
ButterShop/
├── ButterShop.AppHost/          # Aspire orchestrator
├── ButterShop.ApiService/       # Web API
│   ├── Data/                   # DbContext
│   ├── Endpoints/              # Minimal API endpoints
│   ├── Models/                 # Modele danych
│   └── Migrations/             # Migracje EF Core
├── ButterShop.ServiceDefaults/  # Shared config (telemetry, health checks)
├── infra/                      # Bicep templates
└── .github/workflows/          # CI/CD pipeline
```
