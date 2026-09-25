# ResLink

**Find. Connect. Live.**

A student-residence app by **Dynamic Developers** (INSY72315).

**Open the app:** [https://reslink.onrender.com](https://reslink.onrender.com)

The first visit can take about a minute while the free host wakes up. Then sign in with one of the demo accounts:

| Role | Email | Password |
| --- | --- | --- |
| Student | student@reslink.app | Student123! |
| Residence Admin | admin@reslink.app | Admin123! |
| Security | security@reslink.app | Security123! |
| Maintenance | maintenance@reslink.app | Maintenance123! |

Staff self-registration uses the access code `RESLINK-STAFF-2026`.

The same API and accounts work in a local browser or in Android Studio. The emulator talks to a local API at `http://10.0.2.2:5180/`. To point a physical phone at the live backend, set `API_BASE_URL` in `android/app/build.gradle.kts` to `https://reslink.onrender.com/`.

## What the lecturer can mark

1. Open [https://reslink.onrender.com](https://reslink.onrender.com) or run the app locally (steps below).
2. Sign in with each demo account and follow the walkthrough.
3. Run `dotnet test ResLink.sln` from the repository root.
4. API documentation is at [https://reslink.onrender.com/swagger](https://reslink.onrender.com/swagger) or `http://localhost:5180/swagger`.

ResLink is one app for students, residence administrators, security, and maintenance. Each role has its own sign-up path, home screen, and permissions. The browser uses the house-and-chain logo, Nunito type, and a white sidebar. The account role sits under the person's name. Event cards use stock photos. On a phone the menu moves to a bottom bar.

## Purpose

The app shows how a residence can:

- improve student communication through a community feed, events, study groups, and a marketplace
- speed up support through maintenance tickets and in-app notifications
- strengthen safety through a panic button and a noise-complaint workflow
- reward participation with points that can be redeemed in the app
- give staff role-specific dashboards instead of a single shared login

## Team contributions

| Member | Contribution |
| --- | --- |
| Sihle | Infrastructure and operations: Docker and Render hosting, the live URL, JWT session handling when the host restarts, the browser app (logo, layout, role label, event photos), and this run guide. |
| Thami | A screen recording of the app, and a separate website that presents the project as an infographic. See Thami's work below. |
| Blessing | Backend lead. The API, roles, and demo data are in this repository. The steps still required for a real residence are written out below as Blessing's work. |

## Prerequisites

- .NET 9 SDK (required to run the app in a browser on your own machine)
- Android Studio and an emulator, only if you also want to run the mobile app
- Android SDK path set in `android/local.properties` (copy from `android/local.properties.example`)

## How to run

### 1. View the app in a browser

From the repository root:

```bash
cd backend/ResLink.Api
dotnet run --launch-profile http
```

Then open:

**http://localhost:5180**

That page is the ResLink app (sign-in, role dashboards, and all features).  
API documentation remains available at [http://localhost:5180/swagger](http://localhost:5180/swagger).

Leave this terminal open while you use the app.

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

New **staff** accounts require the access code `RESLINK-STAFF-2026`. This stops a student from registering as an administrator, security officer, or maintenance technician.

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

## Thami's work

Thami records the app and publishes a separate infographic site. These two pieces are her deliverables.

### 1. App recording

1. Open [https://reslink.onrender.com](https://reslink.onrender.com) and wait until the sign-in page loads.
2. Record the full screen (Windows Game Bar with `Win + Alt + R`, or phone screen recording). Speak or add captions so the role of each screen is clear.
3. Record this order, signing out between roles:
   - Student: sign in, home, community post, event RSVP, marketplace, maintenance request, panic button, noise complaint, rewards, study group.
   - Security: open panic alert, acknowledge it, resolve the noise complaint.
   - Maintenance: move the leaking-tap ticket from Open to In Progress, then Resolved.
   - Admin: analytics, create an event, remove or restore a feed post, deactivate and reactivate a user.
   - Registration: create a student, then show staff sign-up rejected without `RESLINK-STAFF-2026`.
4. Export one MP4, about 5 to 8 minutes. Put the file link in the submission (Google Drive or OneDrive set to “anyone with the link”).
5. Add the link under this heading when the recording is ready.

### 2. Infographic website

Build a small separate site (one page is enough) that explains ResLink to someone who has not opened the app. It is not a second copy of the app.

Include:

- the ResLink logo (`reslink.jpeg` in this repository)
- the line “Find. Connect. Live.”
- who it is for: students, admin, security, maintenance
- four short blocks: community, events, safety, maintenance
- the live app link: [https://reslink.onrender.com](https://reslink.onrender.com)
- the team names: Sihle, Thami, and Blessing

Host the page on GitHub Pages or any free static host, and paste the URL under this heading when it is live.

## Blessing's work

The API in this repository already signs users in, checks roles, and stores demo data in SQLite. A real residence still needs services that sit outside the code: a database that keeps data, file storage, email, phone alerts, maps, and payments. Those accounts and contracts have to be created by Blessing. The steps below are the order to do them.

Do not put passwords, API keys, or card numbers in Git. Set them in the Render dashboard under **Environment** (Render, 2025).

### 1. Keep data after the host restarts

The live service writes SQLite under `/tmp/reslink`. Render’s free disk is wiped when the service sleeps, so the app loads the demo accounts again. A residence needs a hosted PostgreSQL database (Microsoft, 2025c; Render, 2025).

1. In the Render dashboard, create a **PostgreSQL** instance in the same region as the `reslink` web service.
2. Copy the **Internal Database URL**.
3. On the `reslink` web service, add an environment variable named `DATABASE_URL` and paste that URL.
4. In `backend/ResLink.Api/ResLink.Api.csproj`, add the package `Npgsql.EntityFrameworkCore.PostgreSQL`.
5. In `Program.cs`, when `DATABASE_URL` is set, call `options.UseNpgsql(connectionString)` instead of `UseSqlite`.
6. Replace `EnsureCreated` with EF Core migrations: `dotnet ef migrations add InitialPostgres` and `dotnet ef database update`.
7. Redeploy the web service and sign in with `student@reslink.app`. Create a post, restart the service, and confirm the post is still there.

### 2. Move secrets out of the repository

`appsettings.json` currently holds the JWT signing key and the staff access code so the demo can run. A real venue must rotate those values and keep them only on the server (OWASP, 2023).

1. Generate a new random string of at least 32 characters for the JWT key.
2. On Render, set `Jwt__Key`, `Jwt__Issuer` (`ResLink.Api`), `Jwt__Audience` (`ResLink.Android`), and `Staff__AccessCode` to a new code that is not `RESLINK-STAFF-2026`.
3. Remove the real values from `appsettings.json` before the next public commit. Leave empty placeholders.
4. Redeploy. Old demo tokens will stop working, which is expected. Sign in again.
5. Share the new staff code only with residence staff.

### 3. Store maintenance photos

Students can type a photo URL. A real report needs a picture taken on the phone and stored in cloud storage, not on the Render disk.

1. Create a storage bucket (Cloudflare R2, Azure Blob, or Amazon S3).
2. Create an access key that can upload and read objects in that bucket only.
3. Add `Storage__Account`, `Storage__Key`, and `Storage__Bucket` on Render.
4. Add an endpoint `POST /api/maintenance/photos` that accepts one image, checks the file is a JPEG or PNG under 5 MB, uploads it, and returns the URL.
5. Point the maintenance form at that endpoint instead of the free-text photo URL field.
6. Test with a photo of a tap, then open the ticket as the maintenance user and confirm the image loads.

### 4. Tell staff when something happens

The app already writes an in-app notification row. A residence also needs a push or email when the phone is locked (Google, 2025).

1. Create a Firebase project and add an Android app with package name `com.dynamicdevelopers.reslink`.
2. Download `google-services.json` into the Android app. Do not commit the private server key.
3. Add Firebase Cloud Messaging to the Android app and send the device token to a new API field on the user.
4. On the server, when a panic alert, noise complaint, or maintenance ticket is created, send an FCM message to the security or maintenance tokens.
5. Test with the phone screen locked. The security phone should show the panic alert without opening ResLink first.

### 5. Email for password reset

There is no “forgot password” flow. Staff and students at a real venue will lock themselves out (OWASP, 2023).

1. Create an account with an email provider (for example SendGrid or Amazon SES) and verify the sender address `noreply@reslink.app` or the residence domain.
2. Store `Email__ApiKey` and `Email__From` on Render.
3. Add `POST /api/auth/forgot-password`. It emails a one-time link that expires in 30 minutes. Always return the same message, whether or not the email exists.
4. Add `POST /api/auth/reset-password` to set the new password hash and invalidate the link.
5. Send a test reset to your own inbox and confirm the old password no longer works.

### 6. Reach security outside the app

The panic button creates a row that security sees after they sign in. A real emergency also needs a phone call or SMS to the duty officer.

1. Agree the duty numbers with the residence (voice number and SMS number).
2. Create a Twilio (or similar) account and buy a number.
3. Store `Sms__AccountSid`, `Sms__Token`, and `Sms__From` on Render.
4. When a student sends a panic alert, send one SMS with the student name, room, and residence. Do not send the message again on every status change.
5. Test with your own phone before using the residence number.

### 7. Live location

Panic alerts currently store the student’s room and residence. Live GPS needs the phone’s location permission and a maps key (Google, 2025).

1. In the Android app, request location permission only when the student presses the panic button.
2. Send latitude and longitude on `POST /api/emergencies`.
3. Create a Google Maps key restricted to the ResLink Android package and the web dashboard domain.
4. Show the point on the security screen. If the student denies permission, keep the room text as the fallback.
5. Test indoors and outdoors. Do not log coordinates in Render’s public logs.

### 8. Pay for rewards

Redeeming points only reduces a number in the database. Food vouchers and laundry credit need a payment or voucher partner.

1. Choose one partner the residence already uses (cafe till or laundry vendor).
2. Use their test API. Never store card numbers in ResLink (OWASP, 2023).
3. On redeem, create a one-time code, store it against the redemption, and show it once to the student.
4. Add an admin screen to mark the code as used.
5. Run a test redemption and confirm the points do not return unless the admin voids the code.

### 9. QR check-in at the door

Events show a demo code. A real check-in needs a code that changes per event and a camera scan.

1. When an admin publishes an event, generate a random check-in code and a QR image of `https://reslink.onrender.com/check-in/{code}`.
2. The student screen scans or pastes that code and calls `POST /api/events/{id}/check-in`.
3. Award the attendance points only once per student.
4. Test with two phones: one showing the QR, one scanning it.

### 10. Backups and a custom domain

1. On the PostgreSQL instance, turn on daily backups and keep at least seven days (Render, 2025).
2. Add the residence domain in Render and set the DNS records they provide.
3. Confirm `https://` loads the app and that sign-in still works.
4. Write the restore steps in this file: which backup date, which command, and who to call.

## Project structure

```text
ResLink/
  backend/ResLink.Api/          ASP.NET Core Web API and browser app
  backend/ResLink.Api.Tests/    Authentication and role tests
  android/                      Kotlin Jetpack Compose application
  reslink.jpeg                  App logo
  README.md                     This guide
```

## References

Android Developers (2025a) *Jetpack Compose*. Available at: https://developer.android.com/compose (Accessed: 25 September 2026).

Android Developers (2025b) *Material Design 3 in Compose*. Available at: https://developer.android.com/develop/ui/compose/designsystems/material3 (Accessed: 25 September 2026).

Google (2025) *Firebase Cloud Messaging*. Available at: https://firebase.google.com/docs/cloud-messaging (Accessed: 25 September 2026).

Google Fonts (2026) *Nunito*. Available at: https://fonts.google.com/specimen/Nunito (Accessed: 25 September 2026).

Material Design (2025) *Icons*. Available at: https://m3.material.io/styles/icons/overview (Accessed: 25 September 2026).

MDN (2025) *Using the Fetch API*. Available at: https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch (Accessed: 25 September 2026).

Microsoft (2024) *PasswordHasher of ASP.NET Core Identity*. Available at: https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1 (Accessed: 25 September 2026).

Microsoft (2025a) *Overview of ASP.NET Core*. Available at: https://learn.microsoft.com/aspnet/core/introduction-to-aspnet-core?view=aspnetcore-9.0 (Accessed: 25 September 2026).

Microsoft (2025b) *Authentication and authorization in ASP.NET Core*. Available at: https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-9.0 (Accessed: 25 September 2026).

Microsoft (2025c) *SQLite EF Core database provider*. Available at: https://learn.microsoft.com/ef/core/providers/sqlite/ (Accessed: 25 September 2026).

Microsoft (2025d) *Get started with Swashbuckle and ASP.NET Core*. Available at: https://learn.microsoft.com/aspnet/core/tutorials/getting-started-with-swashbuckle (Accessed: 25 September 2026).

Microsoft (2025e) *Role-based authorization in ASP.NET Core*. Available at: https://learn.microsoft.com/aspnet/core/security/authorization/roles?view=aspnetcore-9.0 (Accessed: 25 September 2026).

OpenAPI Initiative (2024) *OpenAPI Specification*. Available at: https://spec.openapis.org/oas/latest.html (Accessed: 25 September 2026).

OWASP (2023) *Authentication cheat sheet*. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html (Accessed: 25 September 2026).

Render (2025) *Docker on Render*. Available at: https://render.com/docs/docker (Accessed: 25 September 2026).

SQLite Consortium (2025) *About SQLite*. Available at: https://www.sqlite.org/about.html (Accessed: 25 September 2026).

Square (2024) *Retrofit*. Available at: https://square.github.io/retrofit/ (Accessed: 25 September 2026).

Unsplash (2026) *License*. Available at: https://unsplash.com/license (Accessed: 25 September 2026).
