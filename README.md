# Frånvarohantering – Brandbergsskolan

Webbapp för att registrera och hantera frånvaroanmälningar för personal på Brandbergsskolan. Byggd med ASP.NET Core Razor Pages och ASP.NET Core Identity.

## Funktioner

### Personal

- Registrera frånvaro (sjuk, VAB, semester, ledig, annat)
- Välja omfattning (heldag, förmiddag, eftermiddag)
- Ladda upp bilagor (PDF, JPG, PNG, max 5 MB enligt konfiguration)
- Se och hantera egna ärenden
- Redigera eller ta bort ärenden som inte behandlats

### Administratör

- Se alla ärenden med filtrering
- Godkänna eller avslå ärenden
- Exportera till CSV
- Översikt på dashboard
- Hantera användare (skapa m.m.)

## Teknikstack

- **Runtime:** .NET 10
- **Webb:** ASP.NET Core Razor Pages
- **Autentisering:** ASP.NET Core Identity (roller: Admin, Personal)
- **ORM:** Entity Framework Core 10
- **Databas (standard):** SQLite (filen `brandberg.db` i projektmappen)
- **Databas (valfritt):** PostgreSQL via Npgsql
- **UI:** Bootstrap 5, Bootstrap Icons, typsnitt Source Sans 3 (CDN)

## Förutsättningar

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

Kontrollera:

```bash
dotnet --version
```

## Kom igång (SQLite, rekommenderat för lokal utveckling)

1. Klona repot och gå till webbprojektet:

   ```bash
   git clone https://github.com/Alinekzaad/Brandbergsskolan.git
   cd Brandbergsskolan/BrandbergFranvaro
   ```

2. Återställ paket och starta appen:

   ```bash
   dotnet restore
   dotnet run
   ```

3. Öppna **http://localhost:5000** (eller den URL som visas i terminalen).

Vid första start skapas databasschemat med `EnsureCreatedAsync` och **seed-data** (roller, testanvändare och exempelärenden) körs automatiskt om de saknas.

**Startsida (utloggad):** hero-bild under `wwwroot/images/hero.jpg` (lägg dit filen om den saknas i din klon).

## PostgreSQL (valfritt)

Standard är SQLite (`UseSqlite` utelämnad eller `true`). För PostgreSQL:

1. Sätt i `appsettings.json` (eller miljövariabel / user secrets):

   ```json
   "UseSqlite": false,
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=brandberg_franvaro;Username=postgres;Password=postgres"
   }
   ```

2. Se till att PostgreSQL kör och att databasen finns.

3. Applicera migrationer:

   ```bash
   cd BrandbergFranvaro
   dotnet ef database update
   ```

   (`dotnet tool install --global dotnet-ef` om kommandot saknas.)

4. Starta appen med `dotnet run`.

## Testkonton

Lösenord för seed-användare definieras i `Data/SeedData.cs` (standard: **`Password123!`**).

| Roll     | E-post                 |
|----------|------------------------|
| Admin    | admin@brandberg.se     |
| Personal | personal@brandberg.se  |
| Personal | erik@brandberg.se      |

## Konfiguration

| Inställning | Beskrivning |
|-------------|-------------|
| `UseSqlite` | `true` (standard): SQLite. `false`: PostgreSQL + migrationer. |
| `ConnectionStrings:DefaultConnection` | Används när `UseSqlite` är `false`. |
| `FileUpload` | Max storlek, tillåtna filändelser, uppladdningsmapp (`App_Data/Uploads`). |

## Projektstruktur

```
Brandbergsskolan/
└── BrandbergFranvaro/          # Webbapplikationen
    ├── Data/                   # DbContext, migrationer, seed
    ├── Models/
    ├── Pages/
    │   ├── Account/            # Inloggning, utloggning
    │   ├── Admin/
    │   ├── Franvaro/
    │   └── Shared/             # Layout
    ├── Services/               # T.ex. filuppladdning
    ├── wwwroot/                # css, js, images (hero.jpg)
    ├── App_Data/Uploads/       # Uppladdade bilagor (skapas vid behov)
    ├── Program.cs
    └── appsettings.json
```

## Felsökning

- **Webbplatsen kan inte nås / anslutning nekad:** Kör `dotnet run` från mappen `BrandbergFranvaro` och låt terminalen vara öppen. Annan process på port 5000 kan störa; använd t.ex. `set ASPNETCORE_URLS=http://127.0.0.1:5055` (Windows) före `dotnet run` för annan port.
- **Bygget misslyckas (fil låst):** Stoppa en redan körande instans av appen (Ctrl+C) eller avsluta processen `BrandbergFranvaro`.
- **SQLite:** Radera `brandberg.db` för att börja om med tom databas (seed körs igen vid nästa start).
- **Uppladdningar:** Kontrollera att `App_Data/Uploads` finns och är skrivbar.

## Utveckling

```bash
cd BrandbergFranvaro
dotnet watch run
```

Bygger om vid kodändringar under körning.

## Licens

Projektet är skapat för Brandbergsskolan.
