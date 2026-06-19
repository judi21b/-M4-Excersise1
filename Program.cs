using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// AUTHENTICATION + AUTHORIZATION
// ==========================
builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>("Basic", options => { });

builder.Services.AddAuthorization();

var app = builder.Build();

// ==========================
// MIDDLEWARE ORDER
// ==========================
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ==========================
// PROTECTED ENDPOINT
// ==========================
app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    });
})
.RequireAuthorization();

app.Run();


// ==========================
// FAKE AUTH HANDLER (SIMULATION)
// ==========================
public class FakeAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public FakeAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()

    {
        // If no Authorization header → reject user
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
        }

        // Fake successful login
        var claims = new[] { new Claim(ClaimTypes.Name, "Student") };
        var identity = new ClaimsIdentity(claims, "Basic");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Basic");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}