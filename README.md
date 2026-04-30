# SET09102 Peer-to-Peer Rental Marketplace

A .NET MAUI "Library of Things" rental marketplace built for the SET09102 coursework. Users can register, log in, list items, search nearby items, request rentals, manage rental workflows, and leave reviews after completed rentals.

API base URL:

```text
https://set09102-api.b-davison.workers.dev
```

## Features

- API authentication with JWT tokens
- Item listing, browsing, detail view, creation, and owner editing
- Rental requests with incoming and outgoing rental lists
- Rental workflow: requested, approved, rejected, out for rent, returned, completed
- Location-based "Find Near Me" search with latitude, longitude, radius, and category
- Reviews and ratings for completed rentals
- Average rating shown on profile
- MVVM architecture
- Repository pattern
- Service layer
- xUnit tests with coverage
- GitHub Actions CI/CD

## Architecture

The app follows this structure:

```text
View -> ViewModel -> Service -> Repository -> API
```

Views contain XAML UI. ViewModels expose bindable properties and commands. Services contain business rules. Repositories abstract API/data access so ViewModels do not call endpoints directly.

## Project Structure

```text
StarterApp/
|-- .github/workflows/build.yml
|-- StarterApp/
|-- StarterApp.Database/
|-- StarterApp.Migrations/
|-- StarterApp.Test/
|-- docker-compose.yml
`-- StarterApp.sln
```

## Setup

Requirements:

- .NET 10 SDK
- .NET MAUI workload
- Docker Desktop
- Android emulator
- Visual Studio Code with C# Dev Kit

Start Docker:

```bash
docker compose up -d
```

Build the app:

```bash
dotnet clean
dotnet build -c Debug
```

Install APK on emulator:

```bash
adb uninstall com.companyname.starterapp
adb install -r StarterApp/bin/Debug/net10.0-android/com.companyname.starterapp-Signed.apk
```

## Running Tests

Run all tests:

```bash
dotnet test StarterApp.Test/StarterApp.Test.csproj
```

Run tests with coverage:

```bash
dotnet test StarterApp.Test/StarterApp.Test.csproj --collect:"XPlat Code Coverage" --settings StarterApp.Test/coverlet.runsettings
```

Current coverage:

- Line coverage: 64.98%
- Branch coverage: 65.27%

View coverage XML:

```bash
grep -m 1 "<coverage" StarterApp.Test/TestResults/*/coverage.cobertura.xml
```

## CI/CD

GitHub Actions is configured in:

```text
.github/workflows/build.yml
```

The workflow runs on pushes and pull requests to `main`. It restores dependencies, builds the test project, runs xUnit tests, collects coverage, and uploads the coverage report as an artifact.

## Demo Test Flow

Recommended accounts:

- Account A: `Admin Company`
- Account B: `Edward`

Demo steps:

1. Log in as Account A and create an item.
2. Log in as Account B and request to rent the item.
3. Log in as Account A and approve the request.
4. Mark the rental as out for rent.
5. Log in as Account B and mark it as returned.
6. Log in as Account A and complete the rental.
7. Log in as Account B and submit a review.
8. View item reviews and Account A's average rating.
9. Use Find Near Me to search by location and radius.

## Coursework Scope

Implemented:

- Tier 1: authentication integration, item management, basic rental requests, MVVM, repository pattern
- Tier 2: location discovery, rental workflow, reviews, service layer, testing

Not implemented:

- Tier 3 bonus features such as State Pattern, MediatR, advanced map integration, and SonarCloud

## AI Tool Usage

AI assistance was used for planning, implementation guidance, debugging, testing strategy, and documentation support. Suggestions were reviewed, tested, and adjusted to match the coursework requirements and project architecture.

Detailed AI usage evidence is included in the final report.
