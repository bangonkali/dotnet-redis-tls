// src/Program.cs

using StackExchange.Redis;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// --- Redis Connection Setup ---
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost:6379";
    var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
    
    Console.WriteLine("REDIS_HOST: " + redisHost);
    Console.WriteLine("REDIS_PASSWORD: " + redisPassword);
    
    var configuration = new ConfigurationOptions
    {
        // Get the Redis host from environment variables (set in docker-compose.yml)
        EndPoints = { redisHost },
        // Use SSL/TLS
        Ssl = true,
        // Set the username and password
        User = "admin",
        Password = redisPassword,
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
        Console.WriteLine("Validating Redis server certificate...");
        
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            Console.WriteLine("Certificate validation successful.");
            return true;
        }

        // For self-signed certificates, it's common to encounter chain errors.
        // We also explicitly allow name mismatches as requested.
        SslPolicyErrors expectedErrors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch;

        // Check if the encountered errors are only the ones we expect.
        // This is safer than simply returning 'true' for all errors.
        if ((sslPolicyErrors & ~expectedErrors) == SslPolicyErrors.None)
        {
            Console.WriteLine($"Allowing expected SSL errors: {sslPolicyErrors}");
            return true;
        }
        
        // If there are other, unexpected errors, fail the validation.
        Console.WriteLine($"Certificate validation failed with unexpected errors: {sslPolicyErrors}");
        return false;
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

app.MapGet("/set/{key}", async (string key, [FromQuery(Name = "value")] string value, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();

    await db.StringSetAsync(key, value);

    var v = await db.StringGetAsync(key);

    return v.IsNullOrEmpty
        ? Results.NotFound($"Key '{key}' not found in Redis.")
        : Results.Ok($"Value for '{key}': {value}");
});

// POST endpoint to set a key-value pair in Redis
app.MapPost("/set", async (KeyValuePair<string, string> item, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    await db.StringSetAsync(item.Key, item.Value);
    return Results.Ok($"Set '{item.Key}' to '{item.Value}'");
});

app.Run();