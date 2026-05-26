# Happy Home — Projektminne för Claude Code

## Vad är Happy Home?
En psykiatri- och terapiklinik. Systemet hanterar klienter, terapeuter, bokningar, konsultationer, journaler och AI-diagnosstöd. Det är ett fiktivt demosystem byggt för undervisning, men modellerat som ett verkligt klinisystem.

---

## Stack
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core
- SQL Server (LocalDB för dev)
- React 19 + Vite (frontend, separat process — `HappyHome.Web/`)
- JWT-baserad auth (egen `Användare`-tabell, BCrypt-hash, `Microsoft.AspNetCore.Authentication.JwtBearer`)
- Tailwind CSS för styling
- React Router + @tanstack/react-query + axios
- C# på backend, JavaScript (JSX) på frontend

---

## Mappstruktur
```
HappyHome/
  HappyHome.sln
  HappyHome.Api/             → Web API, controllers, Program.cs, AuthController, JwtTokenService
  HappyHome.Core/            → Domänmodeller, enums, interfaces (inkl. Användare + AnvändarRoll)
  HappyHome.Infrastructure/  → DbContext, migrationer, seed (inkl. testanvändare)
  HappyHome.Web/             → React-frontend (Vite)
    src/
      api/         → axios-klient + en modul per resurs (auth, klient, terapeut, bokning, tidslucka)
      components/  → Layout, ProtectedRoute
      context/     → AuthContext (login, logout, token, user)
      hooks/       → custom hooks (tomt så länge)
      pages/
        auth/      → Login
        klient/    → MinaBokningar, Boka, MinJournal
        terapeut/  → MinaPatienter, Konsultation, AiDiagnos
        admin/     → Dashboard, Terapeuter, Tidsluckor
      App.jsx      → Router + Protected/Klient/Terapeut/AdminRoute
      main.jsx     → QueryClientProvider, root render
  tests/
    HappyHome.Api.Tests/     → xUnit-integrationstester
```

---

## Domänmodeller

### Klient
Representerar en patient/klient på kliniken.
- Id, Förnamn, Efternamn, Personnummer, Skapad

### Terapeut
Anställd på kliniken som utför konsultationer.
- Id, Förnamn, Efternamn, Epost
- Roll (TerapeutRoll enum)
- Specialiseringar (List<TerapiTyp> — vilka terapityper de får ta)
- Aktiv (bool), AktivFromDatum, Skapad
- Inaktivering = Aktiv sätts till false, posten raderas ALDRIG (historik måste bevaras)

### Tidslucka
En tidsperiod då en terapeut är tillgänglig för bokning.
- Id, TerapeutId, Start, Slut
- Status: Ledig | Bokad | Avbokad
- Skapad

### Bokning
En klient bokar en tidslucka hos en terapeut.
- Id, KlientId, TidsluckaId, TerapiTyp
- AnledningTillBesok (klientens egna ord — terapeuten ser detta som förberedelse)
- Status: Bokad | Genomförd | Avbokad | Uteblev
- Skapad

### Konsultation
Det faktiska mötet mellan klient och terapeut.
- Id, KlientId, TerapeutId (obligatorisk — alltid kopplad till en terapeut)
- BokningId (nullable — drop-in/akut saknar bokning)
- TerapiTyp, Symptom, Bakgrund
- Skapad

### Journal
En per klient. Skapas automatiskt vid första konsultation.
- Id, KlientId, Skapad

### JournalAnteckning
Anteckningar kopplade till en konsultation.
- Id, JournalId, KonsultationId, TerapeutId
- AnteckningsTyp: Bedömning | Behandlingsplan | Förlopp | Sammanfattning | Övrigt
- Innehåll (text)
- Signerad (bool), SigneradDatum (nullable)
- Skapad
- Signerade anteckningar är låsta — de redigeras ALDRIG, bara kompletteras med ny anteckning
- Osignerade är utkast — bara författaren ser dem

### JournalLogg
Logg över varje åtkomst till en journal (Patientdatalagen-inspirerad).
- Id, JournalId, TerapeutId
- Åtgärd: Läste | Skrev | Signerade
- Tidpunkt

### DiagnosForslag
AI-genererat beslutsstöd kopplat till en konsultation.
- Id, KonsultationId
- Diagnos, Sannolikhet (0-100), Motivering, Rekommendation
- Skapad
- OBS: AI föreslår ALDRIG — terapeuten beslutar alltid. DiagnosForslag är stöd, inte svar.

---

## Enums

```csharp
TerapiTyp: Familjeterapi, Beroendetrapi, Individualtrapi, Krissamtal

TerapeutRoll: Psykolog, Psykiatriker, Familjeterapeut, Beroendeterapeut, Krisspecialist

TidsluckaStatus: Ledig, Bokad, Avbokad

BokningStatus: Bokad, Genomförd, Avbokad, Uteblev

AnteckningsTyp: Bedömning, Behandlingsplan, Förlopp, Sammanfattning, Övrigt

JournalÅtgärd: Läste, Skrev, Signerade
```

---

## Relationer

```
Klient          → många Bokningar
Klient          → en Journal
Terapeut        → många Tidsluckor
Terapeut        → många Konsultationer
Tidslucka       → en Bokning (nullable, 1:1)
Bokning         → en Konsultation (nullable, 1:1)
Konsultation    → många DiagnosForslag
Konsultation    → många JournalAnteckningar
Journal         → många JournalAnteckningar
Journal         → många JournalLogg
```

---

## Affärsregler (måste alltid hålla)

1. En tidslucka kan bara bokas en gång — inga dubbelbokningar
2. En klient får max en aktiv bokning per dag
3. Bokning kräver att terapeuten har rätt TerapiTyp i sina Specialiseringar
4. När en bokning avbokas → tidsluckans Status sätts automatiskt till Ledig
5. En konsultation kräver alltid en TerapeutId
6. JournalAnteckningar raderas aldrig — de kompletteras
7. Signerade anteckningar är låsta för redigering
8. Terapeuter raderas aldrig — Aktiv sätts till false
9. DiagnosForslag är beslutsstöd — inte diagnos. Terapeuten beslutar alltid.

---

## Byggplan (steg)

| Steg | Innehåll | Status |
|------|----------|--------|
| 1 — Initial | Klient, Konsultation, DiagnosForslag, grundläggande API | Klar |
| 2A — Utbyggnad | Terapeut, Tidslucka, Bokning, koppling Konsultation→Terapeut | Pågår |
| 2A.1 — Affärsregeltester | xUnit-tester som dokumenterar reglerna + HTML-täckningsrapport via `run-tests.ps1` | Klar |
| 2B — React-frontend + JWT-auth | Vite/React-app, Användare-tabell, AuthController, ProtectedRoute, sidor för klient/terapeut/admin | Klar |
| 2C — AI Journal | AI-utkast till journalanteckningar, terapeutens godkännandeflöde, journalmodul | Kommande |

---

## Namnkonventioner

- Svenska namn på modeller och properties (Klient, inte Patient eller Client)
- Engelska på tekniska saker (DbContext, Repository, Controller)
- Controllers: `[Modell]Controller`
- DbSets: plural svenska (`Klienter`, `Terapeuter`, `Bokningar`)
- Migrationer: beskrivande CamelCase (`AddTerapeutBokningKonsultation`)

---

## Designbeslut

- **Ingen hård radering** — allt sätts inaktivt eller avbokat, aldrig DELETE i databasen (undantag: tidsluckor utan bokning)
- **Drop-in stöds** — Konsultation.BokningId är nullable
- **Loggning** — JournalLogg skrivs vid varje journalåtkomst
- **Transparens** — klienter kan se sina egna signerade journalanteckningar
- **AI som stöd** — DiagnosForslag och JournalAnteckning-utkast är förslag, terapeuten godkänner alltid

---

## Seed-data (testklienter och terapeuter)

Tre terapeuter:
- Anna Lindgren, Psykolog, specialisering: Individualterapi + Krissamtal
- Marcus Ek, Familjeterapeut, specialisering: Familjeterapi + Individualterapi
- Sara Björk, Beroendeterapeut, specialisering: Beroendetrapi

Två klienter:
- Erik Svensson
- Maria Karlsson

---

## Tester (steg 2A.1)

### Projekt
- `tests/HappyHome.Api.Tests` — xUnit-integrationstester som kör mot en in-memory-databas via `HappyHomeApiFactory` (`WebApplicationFactory<Program>`).
- Varje muterande test börjar med `/api/dev/reset` så att seeden är deterministisk och tester inte påverkar varandra.

### Skrivstil
- Testnamn är svenska meningar på formen `Område_RegelSomGäller` (t.ex. `Bokning_MisslyckasOm_TidsluckaRedanBokad`).
- Varje test inleds med en kommentar som förklarar VARFÖR regeln finns (patientsekretess, patientsäkerhet, intäktsbortfall, spårbarhet), inte hur koden tekniskt gör.

### Affärsregler som täcks av tester
1. En tidslucka kan inte bokas två gånger — `Bokning_MisslyckasOm_TidsluckaRedanBokad`
2. En klient kan inte ha två bokningar samma dag — `Bokning_MisslyckasOm_KlientHarAnnanBokningSammaDag`
3. Bokning kräver att terapeuten har rätt specialisering — `Bokning_MisslyckasOm_TerapeutenSaknarRattSpecialisering`
4. När en bokning avbokas frigörs tidsluckan automatiskt — `Bokning_NarAvbokas_FrigorTidsluckanAutomatiskt`
5. En konsultation måste alltid ha en terapeut — `Konsultation_HarAlltid_KopplingTillTerapeut`

### Kör testerna
```powershell
# Bara testerna
dotnet test

# Tester + HTML-täckningsrapport som öppnas i webbläsaren
./run-tests.ps1
```

`run-tests.ps1` (i projektets rot):
1. Städar `TestResults/` och `TestReport/`.
2. Säkerställer att ReportGenerator finns som lokalt dotnet-verktyg (skapar `.config/dotnet-tools.json` vid behov).
3. Kör `dotnet test --collect:"XPlat Code Coverage"` (Cobertura).
4. Slår ihop täckningsfilerna till HTML i `TestReport/`.
5. Öppnar `TestReport/index.html` i webbläsaren (kan stängas av med `-NoBrowser`).

---

## Steg 2B — frontend, auth och säkerhet

### Identitet
- **Användare** (separat tabell från Klient/Terapeut) har Id, Epost (unik), LösenordHash, Roll, KlientId (nullable), TerapeutId (nullable), Skapad.
- **AnvändarRoll**: Klient, Terapeut, Admin.
- En klient-användare har KlientId satt, terapeut-användare har TerapeutId satt, admin har båda null.
- Lösenord hashas med BCrypt (`BCrypt.Net-Next`) i seederingen och vid framtida registrering.

### Seedade testkonton (samma lösen `Demo123!`)
| Epost                    | Roll      | Kopplad till   |
|--------------------------|-----------|----------------|
| klient@happyhome.se      | Klient    | klienter[0]    |
| terapeut@happyhome.se    | Terapeut  | terapeuter[0]  |
| admin@happyhome.se       | Admin     | —              |

### Autentisering
- `POST /api/Auth/login` (anonym) — verifierar BCrypt-hash, utfärdar JWT.
- `GET /api/Auth/me` (authorize) — returnerar inloggad användares info från claims.
- JWT signeras med HS256, settings i `appsettings.Jwt:{Issuer,Audience,Key,ExpireMinutes}` (8 timmar default).
- Claims: `sub`, `email`, `role`, `klientId` (om Klient), `terapeutId` (om Terapeut).
- `JwtTokenService` (singleton, `HappyHome.Api/Auth/`) bär all signering — controllers vet inget om JWT-internals.

### CORS
- Tillåtna origins läses från `appsettings.Cors:AllowedOrigins`. Default i dev: `http://localhost:5173`.
- Pipeline-ordning i `Program.cs`: `UseCors` → `UseAuthentication` → `UseAuthorization`. Bryts denna ordning får preflight-anrop 401.

### Auktorisering
- Alla skarpa controllers är `[Authorize]` på klassnivå.
- `[Authorize(Roles = "Admin")]` på terapeut-CRUD.
- `[Authorize(Roles = "Admin,Terapeut")]` på skapa/ta bort tidsluckor.
- `DevController` är `[AllowAnonymous]` men returnerar `404` utanför `Development`/`Testing`.
- Tester använder `HappyHomeApiFactory.CreateSeededClient()` som loggar in som admin via `/api/Auth/login` och pre-injicerar Bearer-token i `HttpClient`.

### React-frontend
- Skapad med `npm create vite@latest -- --template react`. Tailwind 3, react-router-dom, @tanstack/react-query, axios.
- **AuthContext** lagrar token i `localStorage`, validerar mot `/me` vid mount, exponerar `login`/`logout`/`user`/`token`.
- **Axios-interceptor** auto-injicerar `Authorization: Bearer <token>`. 401-svar rensar inloggningen och redirectar till `/login`.
- **ProtectedRoute** redirectar oinloggade till `/login`, fel roll till `/no-access`.
- **AI-anrop** i `Konsultation.jsx` och `AiDiagnos.jsx` är stub:ade — ingen patientdata lämnar maskinen i nuvarande steg. Verklig AI-integration ska ske server-side, aldrig från React direkt.

### Köra utvecklingsmiljön
```powershell
# Terminal 1 — API på http://localhost:5189
dotnet run --project HappyHome.Api --launch-profile http

# Terminal 2 — React på http://localhost:5173
cd HappyHome.Web
npm install     # första gången
npm run dev
```

Återställ databasen (rensar och seedar inkl. användare):
```powershell
Invoke-RestMethod -Uri 'http://localhost:5189/api/dev/reset' -Method Delete
```
