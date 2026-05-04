var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = Path.Combine(
        Directory.GetParent(AppContext.BaseDirectory)!
            .Parent!.Parent!.Parent!.FullName, "wwwroot")
});

// Register CORS so the frontend can talk to the backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Register our custom services (Dependency Injection)
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

// Middleware
app.UseCors("AllowAll");
app.UseDefaultFiles();       // Serves index.html automatically
app.UseStaticFiles();        // Serves files from wwwroot/

// -----------------------------------------------
// GET /flights?from=Baghdad&to=Dubai&date=2026-06-01
// Returns list of available flights as JSON
// -----------------------------------------------
app.MapGet("/flights", (string? from, string? to, string? date, IFlightService flightService) =>
{
    var flights = flightService.SearchFlights(from, to, date);
    return Results.Ok(flights);
});

// -----------------------------------------------
// GET /flights/all
// Returns ALL flights (used to populate dropdowns)
// -----------------------------------------------
app.MapGet("/flights/all", (IFlightService flightService) =>
{
    var flights = flightService.GetAllFlights();
    return Results.Ok(flights);
});

// -----------------------------------------------
// POST /book
// Receives booking data from the HTML form (JSON body)
// Stores booking reference in a Cookie
// -----------------------------------------------
app.MapPost("/book", (HttpContext context, BookingRequest request, IBookingService bookingService) =>
{
    // Validate the request
    if (string.IsNullOrWhiteSpace(request.PassengerName) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        request.FlightId <= 0)
    {
        return Results.BadRequest(new
        {
            Status = "Error",
            Message = "Please fill in all required fields."
        });
    }

    // Create the booking
    var booking = bookingService.CreateBooking(request);

    // Store booking reference in a cookie (like the login cookie in Lab 5)
    context.Response.Cookies.Append("bookingRef", booking.Reference, new CookieOptions
    {
        Path = "/",
        HttpOnly = false,   // false so JS can read it
        SameSite = SameSiteMode.Lax
    });

    return Results.Ok(new
    {
        Status = "Success",
        Message = $"Booking confirmed! Your reference is {booking.Reference}.",
        Booking = booking
    });
});

// -----------------------------------------------
// GET /booking
// Reads the booking reference from Cookie
// Returns booking details
// -----------------------------------------------
app.MapGet("/booking", (HttpContext context, IBookingService bookingService) =>
{
    var reference = context.Request.Cookies["bookingRef"];

    if (string.IsNullOrEmpty(reference))
    {
        return Results.BadRequest(new
        {
            Status = "Error",
            Message = "No active booking found. Please book a flight first."
        });
    }

    var booking = bookingService.GetBookingByReference(reference);

    if (booking == null)
    {
        return Results.NotFound(new
        {
            Status = "Error",
            Message = "Booking not found."
        });
    }

    return Results.Ok(new
    {
        Status = "Success",
        Booking = booking
    });
});

// -----------------------------------------------
// GET /cancel
// Deletes the booking cookie (like logout in Lab 5)
// -----------------------------------------------
app.MapGet("/cancel", (HttpContext context) =>
{
    context.Response.Cookies.Delete("bookingRef", new CookieOptions
    {
        Path = "/",
        SameSite = SameSiteMode.Lax
    });

    return Results.Ok(new
    {
        Status = "Success",
        Message = "Booking cancelled successfully."
    });
});

app.Run();

// =============================================
// MODELS
// =============================================

public class Flight
{
    public int Id { get; set; }
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string Date { get; set; } = "";
    public string DepartureTime { get; set; } = "";
    public string ArrivalTime { get; set; } = "";
    public string Airline { get; set; } = "";
    public decimal Price { get; set; }
    public int SeatsAvailable { get; set; }
    public string FlightNumber { get; set; } = "";
}

public class BookingRequest
{
    public int FlightId { get; set; }
    public string PassengerName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PassportNumber { get; set; } = "";
    public string SeatClass { get; set; } = "Economy";
}

public class Booking
{
    public string Reference { get; set; } = "";
    public int FlightId { get; set; }
    public string PassengerName { get; set; } = "";
    public string Email { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string Date { get; set; } = "";
    public string DepartureTime { get; set; } = "";
    public string Airline { get; set; } = "";
    public string FlightNumber { get; set; } = "";
    public decimal Price { get; set; }
    public string SeatClass { get; set; } = "";
    public string BookedAt { get; set; } = "";
}

// =============================================
// INTERFACES (like IGreetingService in Lab 4)
// =============================================

public interface IFlightService
{
    List<Flight> SearchFlights(string? from, string? to, string? date);
    List<Flight> GetAllFlights();
}

public interface IBookingService
{
    Booking CreateBooking(BookingRequest request);
    Booking? GetBookingByReference(string reference);
}

// =============================================
// SERVICE IMPLEMENTATIONS
// =============================================

public class FlightService : IFlightService
{
    // In-memory flight data (simulates a database / JSON file like Lab 3)
    private static readonly List<Flight> _flights = new()
    {
        new Flight { Id = 1, From = "Baghdad", To = "Dubai", Date = "2026-06-01", DepartureTime = "08:00", ArrivalTime = "10:30", Airline = "Iraqi Airways", Price = 250, SeatsAvailable = 45, FlightNumber = "IA-101" },
        new Flight { Id = 2, From = "Baghdad", To = "Dubai", Date = "2026-06-01", DepartureTime = "15:00", ArrivalTime = "17:30", Airline = "FlyDubai", Price = 180, SeatsAvailable = 12, FlightNumber = "FZ-202" },
        new Flight { Id = 3, From = "Baghdad", To = "Istanbul", Date = "2026-06-01", DepartureTime = "09:30", ArrivalTime = "12:45", Airline = "Turkish Airlines", Price = 320, SeatsAvailable = 30, FlightNumber = "TK-303" },
        new Flight { Id = 4, From = "Baghdad", To = "London", Date = "2026-06-02", DepartureTime = "23:00", ArrivalTime = "06:30", Airline = "British Airways", Price = 780, SeatsAvailable = 8, FlightNumber = "BA-404" },
        new Flight { Id = 5, From = "Dubai", To = "Baghdad", Date = "2026-06-03", DepartureTime = "14:00", ArrivalTime = "16:30", Airline = "Iraqi Airways", Price = 240, SeatsAvailable = 50, FlightNumber = "IA-505" },
        new Flight { Id = 6, From = "Istanbul", To = "Baghdad", Date = "2026-06-04", DepartureTime = "11:00", ArrivalTime = "14:15", Airline = "Turkish Airlines", Price = 310, SeatsAvailable = 25, FlightNumber = "TK-606" },
        new Flight { Id = 7, From = "Baghdad", To = "Cairo", Date = "2026-06-05", DepartureTime = "07:00", ArrivalTime = "09:00", Airline = "EgyptAir", Price = 290, SeatsAvailable = 35, FlightNumber = "MS-707" },
        new Flight { Id = 8, From = "Baghdad", To = "Amman", Date = "2026-06-05", DepartureTime = "13:00", ArrivalTime = "14:30", Airline = "Royal Jordanian", Price = 150, SeatsAvailable = 60, FlightNumber = "RJ-808" },
    };

    public List<Flight> SearchFlights(string? from, string? to, string? date)
    {
        var result = _flights.AsQueryable();

        if (!string.IsNullOrWhiteSpace(from))
            result = result.Where(f => f.From.ToLower().Contains(from.ToLower()));

        if (!string.IsNullOrWhiteSpace(to))
            result = result.Where(f => f.To.ToLower().Contains(to.ToLower()));

        if (!string.IsNullOrWhiteSpace(date))
            result = result.Where(f => f.Date == date);

        return result.ToList();
    }

    public List<Flight> GetAllFlights() => _flights;
}

public class BookingService : IBookingService
{
    // In-memory storage of bookings (like List<Student> in Lab 3)
    private static readonly List<Booking> _bookings = new();
    private static readonly List<Flight> _flights = new FlightService().GetAllFlights();

    public Booking CreateBooking(BookingRequest request)
    {
        var flight = _flights.FirstOrDefault(f => f.Id == request.FlightId);

        var booking = new Booking
        {
            Reference = "BK" + new Random().Next(100000, 999999),
            FlightId = request.FlightId,
            PassengerName = request.PassengerName,
            Email = request.Email,
            From = flight?.From ?? "",
            To = flight?.To ?? "",
            Date = flight?.Date ?? "",
            DepartureTime = flight?.DepartureTime ?? "",
            Airline = flight?.Airline ?? "",
            FlightNumber = flight?.FlightNumber ?? "",
            Price = flight?.Price ?? 0,
            SeatClass = request.SeatClass,
            BookedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        _bookings.Add(booking);
        return booking;
    }

    public Booking? GetBookingByReference(string reference)
    {
        return _bookings.FirstOrDefault(b => b.Reference == reference);
    }
}
