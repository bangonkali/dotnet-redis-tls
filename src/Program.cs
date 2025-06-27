// src/Program.cs
using StackExchange.Redis;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// --- Redis Connection Setup ---
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = new ConfigurationOptions
    {
        // Get the Redis host from environment variables (set in docker-compose.yml)
        EndPoints = { Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost:6379" },
        // Use SSL/TLS
        Ssl = true,
        // Set the username and password
        User = "admin",
        Password = Environment.GetEnvironmentVariable("REDIS_PASSWORD"),
        // Allow the admin user to execute commands
        AllowAdmin = true,
    };

    // --- Certificate Validation for Self-Signed Certs ---
    // This callback is crucial for development environments using self-signed certificates.
    // It tells the client to trust the certificate presented by the Redis server.
    // In a production environment, you would typically use a proper certificate
    // authority and would not need to bypass validation like this.
    configuration.CertificateValidation += (sender, certificate, chain, sslPolicyErrors) =>
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            Console.WriteLine("Certificate validation successful.");
            return true;
        }

        // In a real application, you should check the certificate subject or issuer.
        // For this example, we accept any self-signed certificate.
        Console.WriteLine($"Certificate error: {sslPolicyErrors}");
        // This line trusts the self-signed certificate.
        return true; 
    };

    Console.WriteLine("Connecting to Redis...");
    var multiplexer = ConnectionMultiplexer.Connect(configuration);
    Console.WriteLine("Successfully connected to Redis.");
    return multiplexer;
});


var app = builder.Build();

// --- API Endpoints ---

// GET endpoint to retrieve a value from Redis by key
app.MapGet("/get/{key}", async (string key, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var value = await db.StringGetAsync(key);

    if (value.IsNullOrEmpty)
    {
        return Results.NotFound($"Key '{key}' not found in Redis.");
    }

    return Results.Ok($"Value for '{key}': {value}");
});

// POST endpoint to set a key-value pair in Redis
app.MapPost("/set", async (KeyValuePair<string, string> item, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    await db.StringSetAsync(item.Key, item.Value);
    return Results.Ok($"Set '{item.Key}' to '{item.Value}'");
});

app.Run();
