using System.Reflection;
using bookstore.OpenApi;
using bookstore.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

builder.Services
    .AddGrpc(options => options.EnableDetailedErrors = builder.Environment.IsDevelopment())
    .AddJsonTranscoding();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "bookstore.example.com",
        Version = "v1",
        Description = "An API for bookstore.example.com",
        Contact = new OpenApiContact
        {
            Name = "API support",
            Email = "aepsupport@aep.dev"
        }
    });

    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    options.DocumentFilter<BookstoreDocumentFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddSingleton<IBookstoreRepository, InMemoryBookstoreRepository>();

var app = builder.Build();

app.UseSwagger(options =>
{
    options.RouteTemplate = "openapi/{documentName}.json";
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Bookstore v1");
    options.RoutePrefix = "swagger";
});

app.MapGrpcService<BookstoreService>();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
