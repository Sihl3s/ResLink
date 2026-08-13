using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ResLink.Api.Contracts;

namespace ResLink.Api.Tests;

[TestClass]
public class AuthRoleTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private string _dbPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"reslink-tests-{Guid.NewGuid():N}.db");
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
            builder.UseSetting("Jwt:Key", "ResLink-DynamicDevelopers-Prototype-Jwt-Key-2026!");
            builder.UseSetting("Staff:AccessCode", "RESLINK-STAFF-2026");
        });
    }

    [TestCleanup]
    public void Cleanup()
    {
        _factory.Dispose();
        foreach (var suffix in new[] { "", "-shm", "-wal" })
        {
            var path = _dbPath + suffix;
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // SQLite can keep a short lock after dispose; temp files are disposable.
            }
        }
    }

    [TestMethod]
    public async Task Student_Can_Login_And_Is_Blocked_From_Admin_Analytics()
    {
        var client = _factory.CreateClient();
        var auth = await Login(client, "student@reslink.app", "Student123!");
        Assert.AreEqual("Student", auth.Role);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        var analytics = await client.GetAsync("/api/admin/analytics");
        Assert.AreEqual(HttpStatusCode.Forbidden, analytics.StatusCode);
    }

    [TestMethod]
    public async Task Staff_Register_Fails_Without_Access_Code()
    {
        var client = _factory.CreateClient();
        var residences = await client.GetFromJsonAsync<List<ResidenceDto>>("/api/auth/residences");
        Assert.IsNotNull(residences);
        Assert.IsTrue(residences.Count > 0);

        var response = await client.PostAsJsonAsync("/api/auth/register/staff", new StaffRegisterRequest(
            "New Guard",
            "guard@reslink.app",
            "Guard123!",
            "SEC-099",
            "Security",
            "WRONG-CODE",
            residences[0].Id));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Security_Can_Read_Emergency_Queue()
    {
        var client = _factory.CreateClient();
        var auth = await Login(client, "security@reslink.app", "Security123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.GetAsync("/api/emergencies");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var alerts = await response.Content.ReadFromJsonAsync<List<EmergencyAlertDto>>();
        Assert.IsNotNull(alerts);
        Assert.IsTrue(alerts.Count >= 1);
    }

    [TestMethod]
    public async Task Student_Can_Register_And_Receive_Jwt()
    {
        var client = _factory.CreateClient();
        var residences = await client.GetFromJsonAsync<List<ResidenceDto>>("/api/auth/residences");
        var response = await client.PostAsJsonAsync("/api/auth/register/student", new StudentRegisterRequest(
            "New Student",
            "new.student@reslink.app",
            "Student123!",
            "STU999",
            residences![0].Id,
            "C-101"));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.IsNotNull(auth);
        Assert.AreEqual("Student", auth.Role);
        Assert.IsFalse(string.IsNullOrWhiteSpace(auth.Token));
    }

    private static async Task<AuthResponse> Login(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.IsNotNull(auth);
        return auth;
    }
}
