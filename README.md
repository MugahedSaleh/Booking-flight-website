# ✈ SkyBook – Book Flight App
## Step-by-Step Setup Guide for Visual Studio

---

## 📁 Project Structure

```
BookFlightApp/
├── Program.cs              ← Backend (C# Minimal API)
├── BookFlightApp.csproj    ← Project file
├── Properties/
│   └── launchSettings.json ← Port 5050 config
└── wwwroot/
    ├── index.html          ← Search flights page
    └── booking.html        ← My booking page
```

---

## 🚀 STEP 1 — Open the Project in Visual Studio

1. Open **Visual Studio 2022** (make sure .NET 8 SDK is installed)
2. Click **"Open a project or solution"**
3. Browse to the `BookFlightApp` folder
4. Select **`BookFlightApp.csproj`** and click Open

---

## 🚀 STEP 2 — Run the Project

1. Press **F5** (or click the green ▶ Run button)
2. Visual Studio will build and launch the app
3. Your browser will open automatically at: **http://localhost:5050**

---

## 🚀 STEP 3 — Test the App

### ✅ Test GET /flights/all
- Open browser → http://localhost:5050/flights/all
- You should see a JSON array of all flights

### ✅ Test GET /flights?from=Baghdad&to=Dubai
- Open browser → http://localhost:5050/flights?from=Baghdad&to=Dubai&date=2026-06-01
- Returns filtered flights as JSON

### ✅ Test the Search Page
- Open http://localhost:5050
- Select **Baghdad → Dubai**, choose **June 1, 2026**, click **Search ✈**
- Flight cards appear — click **Select** to book

### ✅ Test POST /book
- Fill in the booking form (name, email, seat class)
- Click **Confirm Booking**
- A confirmation modal shows your reference (e.g. BK123456)
- A **Cookie** is stored in your browser automatically

### ✅ Test GET /booking (reads cookie)
- Click **View Booking** or go to http://localhost:5050/booking.html
- Click **Load Profile** → your booking details appear (read from cookie)

### ✅ Test GET /cancel (deletes cookie like logout)
- On the booking page, click **Cancel Booking**
- The cookie is deleted
- Visiting booking.html again shows "No Active Booking"

---

## 🔑 What's Covered from Your Lab Files

| Concept          | Where it's used                                  | Lab Reference |
|------------------|--------------------------------------------------|---------------|
| GET endpoint     | `/flights`, `/flights/all`, `/booking`, `/cancel`| Lab 3, 5      |
| POST endpoint    | `/book` (receives JSON body)                     | Lab 5         |
| JSON response    | All endpoints return JSON                        | Lab 3         |
| Cookies (set)    | POST /book stores `bookingRef` cookie            | Lab 5         |
| Cookies (read)   | GET /booking reads `bookingRef` cookie           | Lab 5         |
| Cookies (delete) | GET /cancel deletes cookie                       | Lab 5         |
| Interfaces       | `IFlightService`, `IBookingService`              | Lab 4         |
| Services (DI)    | `AddScoped<>()` in Program.cs                    | Lab 4         |
| List of objects  | `List<Flight>`, `List<Booking>`                  | Lab 3         |
| Static files     | HTML served from wwwroot/                        | Lab 4, 5      |
| fetch() POST     | Booking form sends JSON to /book                 | Lab 5         |
| fetch() GET      | Search uses GET with query params                | Lab 5         |

---

## 🎯 Student Tasks (from your lab assignments)

1. **Add a passenger class** (Economy / Business / First Class) to the Booking
2. **Search by flight number** — add a new GET endpoint `/flights/{number}`
3. **Add a new route** (e.g. Baghdad → Paris)
4. **Modify login credentials** — change the hardcoded check in `/book`
5. **Add an admin endpoint** GET `/admin/bookings` that returns all bookings

---

## ❓ Troubleshooting

| Problem                        | Solution                                              |
|-------------------------------|-------------------------------------------------------|
| App won't start               | Check .NET 8 SDK is installed (`dotnet --version`)    |
| Port already in use           | Change port in `launchSettings.json`                  |
| Flights don't load            | Make sure backend is running (check console window)   |
| Cookie not sent               | The fetch uses `credentials: "include"` — check that  |
| CORS error                    | Already configured in Program.cs with AllowAll        |
