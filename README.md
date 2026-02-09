# Frånvarohantering – Brandbergsskolan

Ett komplett system för att hantera frånvaroanmälningar för personal på Brandbergsskolan.

## 🚀 Funktioner

### Personal
- Registrera frånvaro (Sjuk, VAB, Semester, Ledig, Annat)
- Välja omfattning (Heldag, Förmiddag, Eftermiddag)
- Ladda upp bilagor (PDF, JPG, PNG - max 5 MB)
- Se och hantera sina egna ärenden
- Redigera/ta bort ärenden som ej behandlats

### Administratör
- Se alla ärenden med avancerad filtrering
- Godkänna eller avslå ärenden
- Exportera till CSV
- Statistiköversikt på dashboard
- Full hantering av alla ärenden oavsett status

## 📋 Förutsättningar

Innan du börjar, se till att du har följande installerat:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (för PostgreSQL och pgAdmin)
- Git (valfritt, för kloning)

### Verifiera installation

```bash
dotnet --version  # Bör visa 8.0.x
docker --version  # Bör visa Docker version
docker compose version  # Bör visa Docker Compose version
```

## 🏁 Kom igång

### 1. Klona eller packa upp projektet

```bash
# Om du klonar
git clone <repo-url>
cd BrandbergFranvaro

# ELLER packa upp zip-filen
unzip BrandbergFranvaro.zip
cd BrandbergFranvaro
```

### 2. Starta databasen med Docker

```bash
docker compose up -d
```

Detta startar:
- **PostgreSQL** på port `5432`
- **pgAdmin** på port `5050`

Vänta cirka 10 sekunder för att databaserna ska initialiseras.

### 3. Installera beroenden

```bash
cd BrandbergFranvaro
dotnet restore
```

### 4. Kör databas-migrationen

```bash
dotnet ef database update
```

> **Tips:** Om du inte har `dotnet ef` installerat, kör först:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

### 5. Starta applikationen

```bash
dotnet run
```

Applikationen startar på: **http://localhost:5000**

## 🔐 Testkonton

| Roll | E-post | Lösenord |
|------|--------|----------|
| Admin | admin@brandberg.se | Password123! |
| Personal | personal@brandberg.se | Password123! |
| Personal | erik@brandberg.se | Password123! |

## 📊 pgAdmin (Databashantering)

Öppna **http://localhost:5050** i webbläsaren.

**Inloggningsuppgifter:**
- E-post: `admin@brandberg.se`
- Lösenord: `admin123`

**Anslut till databasen:**
1. Högerklicka på "Servers" → "Register" → "Server..."
2. Fyll i:
   - **Name:** Brandberg
   - **Host:** `postgres` (eller `host.docker.internal` på Mac/Windows)
   - **Port:** `5432`
   - **Database:** `brandberg_franvaro`
   - **Username:** `postgres`
   - **Password:** `postgres`

## 🗂 Projektstruktur

```
BrandbergFranvaro/
├── docker-compose.yml       # Docker-konfiguration
├── README.md                # Denna fil
└── BrandbergFranvaro/       # .NET-projektet
    ├── Data/                # DbContext och Migrations
    ├── Models/              # Datamodeller (User, AbsenceRequest)
    ├── Pages/               # Razor Pages
    │   ├── Account/         # Inloggning/utloggning
    │   ├── Admin/           # Admin-sidor
    │   ├── Franvaro/        # Personal-sidor
    │   └── Shared/          # Layout och delad kod
    ├── Services/            # Tjänster (filuppladdning)
    ├── wwwroot/             # Statiska filer (CSS, JS)
    └── App_Data/Uploads/    # Uppladdade bilagor
```

## ⚙️ Konfiguration

Alla inställningar finns i `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=brandberg_franvaro;Username=postgres;Password=postgres"
  },
  "FileUpload": {
    "MaxFileSizeBytes": 5242880,
    "AllowedExtensions": [".pdf", ".jpg", ".jpeg", ".png"],
    "UploadPath": "App_Data/Uploads"
  }
}
```

## ❓ Vanliga problem och lösningar

### Problem: "Connection refused" vid start

**Orsak:** PostgreSQL har inte hunnit starta.

**Lösning:**
```bash
# Kontrollera att containrarna kör
docker compose ps

# Vänta och försök igen, eller starta om
docker compose restart
```

### Problem: Port 5432 är redan upptagen

**Orsak:** En annan PostgreSQL-instans kör redan.

**Lösning:**
```bash
# Stoppa befintlig PostgreSQL eller ändra port i docker-compose.yml
docker compose down
# Ändra "5432:5432" till "5433:5432" i docker-compose.yml
docker compose up -d
# Uppdatera också ConnectionString i appsettings.json till Port=5433
```

### Problem: Port 5050 är redan upptagen (pgAdmin)

**Lösning:** Ändra `5050:80` till exempelvis `5051:80` i docker-compose.yml.

### Problem: "dotnet ef" kommandot finns inte

**Lösning:**
```bash
dotnet tool install --global dotnet-ef
# Starta om terminalen
```

### Problem: Migrationen misslyckas

**Orsak:** Databasen är inte tillgänglig.

**Lösning:**
```bash
# Kontrollera att PostgreSQL kör
docker compose logs postgres

# Försök ansluta manuellt
docker exec -it brandberg_postgres psql -U postgres -d brandberg_franvaro
```

### Problem: Filer laddas inte upp

**Orsak:** Mappen App_Data/Uploads finns inte.

**Lösning:**
```bash
mkdir -p BrandbergFranvaro/App_Data/Uploads
```

## 🔄 Starta om allt från scratch

```bash
# Stoppa och ta bort alla containrar och volymer
docker compose down -v

# Starta om
docker compose up -d

# Kör migrationen igen
cd BrandbergFranvaro
dotnet ef database update
dotnet run
```

## 🛑 Stoppa applikationen

```bash
# Stoppa .NET-appen: Ctrl+C i terminalen

# Stoppa Docker-containrarna
docker compose down

# Stoppa OCH ta bort all data (volymer)
docker compose down -v
```

## 📝 Teknikstack

- **Backend:** .NET 8, ASP.NET Core Razor Pages
- **Frontend:** Bootstrap 5, Bootstrap Icons
- **Databas:** PostgreSQL 16 via Docker
- **ORM:** Entity Framework Core 8 (Npgsql)
- **Autentisering:** ASP.NET Core Identity

## 📄 Licens

Detta projekt är skapat för Brandbergsskolan.

---

**Skapad med ❤️ för Brandbergsskolan**

