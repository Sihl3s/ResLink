# ResLink

**Connecting Student Living Through Technology**

A student-residence engagement prototype by **Dynamic Developers** (INSY72315).

ResLink is a digital platform for student accommodation. Students, residence administrators, security personnel, and maintenance teams share one application backed by a C# web API. Each role has its own sign-up path, home screen, and permissions.

The prototype can be opened in a web browser or in the Android app. Both use the same API and the same demo accounts. The interface is a light, high-whitespace layout with forest-green ResLink branding. Each role sees a different menu and home screen.

## Purpose

The prototype demonstrates how a residence can:

- improve student communication through a community feed, events, study groups, and a marketplace
- speed up support through maintenance tickets and in-app notifications
- strengthen safety through a panic button and a noise-complaint workflow
- reward participation with points that can be redeemed in-app
- give staff role-specific dashboards instead of a single shared login

## Team and technologies

| Layer | Technology |
| --- | --- |
| Browser prototype | HTML, CSS, and JavaScript served by the API |
| Mobile app | Kotlin, Jetpack Compose, Material 3, Retrofit |
| Backend API | ASP.NET Core 9, Entity Framework Core, SQLite, JWT, Swagger |
| Access control | Role-based authentication (Student, Admin, Security, Maintenance) |

## Prerequisites

- .NET 9 SDK (required to view the prototype in a browser)
- Android Studio and an emulator, only if you also want to run the mobile app
- Android SDK path set in `android/local.properties` (copy from `android/local.properties.example`)

## How to run

### 1. View the prototype in a browser

From the repository root:

```bash
cd backend/ResLink.Api
dotnet run --launch-profile http
```

Then open:

**http://localhost:5180**

That page is the ResLink prototype (login, role dashboards, and all features).  
API documentation remains available at [http://localhost:5180/swagger](http://localhost:5180/swagger) if needed.

Leave this terminal open while you use the prototype.

### 2. Run the Android application (optional)

1. Confirm the API is running on port 5180.
2. Copy `android/local.properties.example` to `android/local.properties` and set the Android SDK path.
3. In Android Studio, open the `android` folder and wait for Gradle sync to finish.
4. Start an emulator and run the **app** configuration.

The emulator calls the API at `http://10.0.2.2:5180/`.

To use a physical phone on the same Wi-Fi network, change `API_BASE_URL` in `android/app/build.gradle.kts` to the computer’s LAN address, for example `http://192.168.1.10:5180/`.

## Demo accounts

The database is created and seeded automatically the first time the API starts.

| Role | Email | Password |
| --- | --- | --- |
| Student | student@reslink.app | Student123! |
| Residence Admin | admin@reslink.app | Admin123! |
| Security | security@reslink.app | Security123! |
| Maintenance | maintenance@reslink.app | Maintenance123! |

New **student** accounts can be created from the student sign-up screen (name, email, student number, residence, and room).

New **staff** accounts require the access code `RESLINK-STAFF-2026`. This prevents a student from registering as an administrator, security officer, or maintenance technician.

## Role-based access

After login, the API returns a JWT that includes the user’s role. The browser and Android app then open the correct dashboard.

| Feature | Student | Admin | Security | Maintenance |
| --- | --- | --- | --- | --- |
| Community feed and polls | Yes | Yes (can moderate) | — | — |
| Events and RSVP | RSVP | Create and manage | — | — |
| Marketplace | Buy / sell | Oversight | — | — |
| Maintenance tickets | Create and track own | View all | — | Update status |
| Noise complaints | Submit (optional anonymous) | View | Inbox and resolve | — |
| Panic button | Trigger | View | Inbox and respond | — |
| Rewards and study groups | Yes | View / moderate | — | — |
| Analytics and user management | — | Yes | — | — |

## Suggested walkthrough for marking

1. **Student** (`student@reslink.app`): open Home, Community Feed, Events (RSVP), Marketplace, Maintenance, Panic, Noise complaint, Rewards, and Study Groups.
2. **Security** (`security@reslink.app`): open the security desk, acknowledge the seeded panic alert, and resolve the noise complaint.
3. **Maintenance** (`maintenance@reslink.app`): open the ticket board and move the leaking-tap ticket from Open to In Progress, then Resolved.
4. **Admin** (`admin@reslink.app`): review analytics, create an event, moderate a feed post, and deactivate or reactivate a user.
5. **Registration check:** create a new student account, then attempt staff sign-up without the access code to confirm the request is rejected.

## Automated tests

From the repository root:

```bash
dotnet test ResLink.sln
```

The tests confirm:

- a student can log in and is blocked from admin analytics
- staff registration fails without the correct access code
- security can read the emergency queue
- a new student can register and receive a JWT

## Prototype limitations

The following are simulated so the prototype can be demonstrated without extra hardware or cloud services:

- live GPS location (panic alerts use the student’s room and residence)
- camera photo uploads (an optional photo URL can be entered)
- push notifications (alerts appear in the in-app notification list)
- payments and real voucher fulfilment
- physical QR scanners (events show a demo check-in code)
- cloud hosting (the API and SQLite database run locally)

## Project structure

```text
ResLink/
  backend/ResLink.Api/          ASP.NET Core Web API and browser prototype
  backend/ResLink.Api.Tests/    Authentication and role tests
  android/                      Kotlin Jetpack Compose application
  README.md                     This guide
```
