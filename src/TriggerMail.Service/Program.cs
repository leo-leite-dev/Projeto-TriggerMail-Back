using Prometheus;
using System.Threading.Channels;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TriggerMail.Core.Application.Ports.Email;
using TriggerMail.Core.Application.Ports.Messaging;
using TriggerMail.Core.Application.Ports.Persistence;
using TriggerMail.Core.Application.Ports.Security;
using TriggerMail.Core.Domain.Entities;
using TriggerMail.Service.Infra.Email;
using TriggerMail.Service.Infra.Health;
using TriggerMail.Service.Infra.Idempotency;
using TriggerMail.Service.Infra.Persistence;
using TriggerMail.Service.Infra.Persistence.Repositories;
using TriggerMail.Service.Infra.Queue;
using TriggerMail.Service.Infra.Security;
using TriggerMail.Service.Workers;
using UseCaseIFireEmailTrigger = TriggerMail.Core.Application.Features.Triggers.IFireEmailTrigger;
using TriggerMail.Core.Application.Features.Triggers; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("TriggerMail");
    opt.UseNpgsql(cs);
});

// ===== Use case (Features.Triggers) + Porta (adapter) =====
builder.Services.AddScoped<UseCaseIFireEmailTrigger, FireEmailTriggerImpl>();
builder.Services.AddScoped<IEmailTriggerPort, FireEmailTriggerPortAdapter>(); // HookController injeta esta porta
builder.Services.AddScoped<ITriggerRepository, EfTriggerRepository>();
builder.Services.AddScoped<ITemplateRepository, EfTemplateRepository>();
builder.Services.AddScoped<IEmailLogRepository, EfEmailLogRepository>();

builder.Services.AddSingleton<ISignatureVerifier, SignatureVerifier>();

builder.Services.AddScoped<ITemplateRenderer, SimpleTemplateRenderer>();

var channel = Channel.CreateUnbounded<TriggerMail.Core.Contracts.Queue.EmailJob>();
builder.Services.AddSingleton(channel);
builder.Services.AddSingleton<IQueuePublisher, InMemoryQueue>();
builder.Services.AddSingleton<IQueueConsumer, InMemoryQueueConsumer>();

builder.Services.AddScoped<IEmailProvider, SmtpEmailProvider>();

builder.Services.Configure<RetryOptions>(builder.Configuration.GetSection("Retry"));

builder.Services.AddHostedService<EmailWorker>();

builder.Services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opts.AddPolicy("per-alias", httpContext =>
    {
        var alias = httpContext.Request.RouteValues.TryGetValue("alias", out var v)
            ? v?.ToString() ?? "global"
            : "global";

        return RateLimitPartition.GetTokenBucketLimiter(alias, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 60,
            TokensPerPeriod = 60,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });
    });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("TriggerMail")!, name: "postgres")
    .AddCheck<SmtpHealthCheck>("smtp");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();

app.UseRateLimiter();

app.MapGet("/health", () => Results.Ok(new { ok = true, service = "TriggerMail" }));
app.MapControllers().RequireRateLimiting("per-alias");
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapMetrics();

await EnsureDatabase(app.Services);

app.Run();


// ---------------- helpers ----------------
static async Task EnsureDatabase(IServiceProvider sp)
{
    using var scope = sp.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.EmailTemplates.AnyAsync() && !await db.EmailTriggers.AnyAsync())
    {
        var t = EmailTemplate.Create(
            key: "digimon.release.ptBR",
            subject: "Nova carta: {{name}} ({{rarity}})",
            html: "<h2>{{name}}</h2><p>Raridade: {{rarity}}</p><p>Set: {{set}}</p><p><a href='{{buyUrl}}'>Comprar</a></p>",
            text: "Nova carta: {{name}}",
            version: 1,
            isActive: true
        );
        db.EmailTemplates.Add(t);

        var trig = EmailTrigger.Create(
            alias: "digimon-release",
            templateKey: "digimon.release.ptBR",
            lang: "pt-BR",
            authType: "none",
            authSecret: null,
            defaultRecipients: new[] { "fallback.@gmail.com" }, // fallback
            mappingConfigJson: null,
            enabled: true
        );
        db.EmailTriggers.Add(trig);

        await db.SaveChangesAsync();
    }
}
