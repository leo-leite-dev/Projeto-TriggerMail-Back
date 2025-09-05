var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: registrar adapters (DbContext Sqlite, InMemoryQueue, SmtpEmailProvider, HandlebarsRenderer)
// TODO: registrar EmailWorker (BackgroundService)

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapGet("/health", () => Results.Ok(new { ok = true, service = "TriggerMail" }));
app.MapControllers();
app.Run();