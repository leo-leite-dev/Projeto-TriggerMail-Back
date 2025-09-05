using System.Threading.Channels;
using TriggerMail.Core.Application.Features.Triggers;
using TriggerMail.Core.Application.Ports.Email;
using TriggerMail.Core.Application.Ports.Messaging;
using TriggerMail.Core.Application.Ports.Persistence;
using TriggerMail.Core.Application.Ports.Security;
using TriggerMail.Core.Domain.Entities;
using TriggerMail.Service.Infra.Persistence;
using TriggerMail.Service.Infra.Queue;
using TriggerMail.Service.Infra.Security;
using TriggerMail.Service.Infra.Templates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IFireEmailTrigger, FireEmailTriggerImpl>();

builder.Services.AddSingleton<ITriggerRepository, InMemoryTriggerRepository>();
builder.Services.AddSingleton<ITemplateRepository, InMemoryTemplateRepository>();
builder.Services.AddSingleton<ISignatureVerifier, SignatureVerifier>();
builder.Services.AddSingleton<ITemplateRenderer, SimpleTemplateRenderer>();

var channel = Channel.CreateUnbounded<TriggerMail.Core.Contracts.Queue.EmailJob>();
builder.Services.AddSingleton(channel);
builder.Services.AddSingleton<IQueuePublisher, InMemoryQueue>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { ok = true, service = "TriggerMail" }));
app.MapControllers();

Seed(app.Services);

app.Run();

static void Seed(IServiceProvider sp)
{
    using var scope = sp.CreateScope();
    var triggers = scope.ServiceProvider.GetRequiredService<ITriggerRepository>();
    var templates = scope.ServiceProvider.GetRequiredService<ITemplateRepository>();

    var t = EmailTemplate.Create(
        key: "digimon.release.ptBR",
        subject: "Nova carta: {{name}} ({{rarity}})",
        html: "<h2>{{name}}</h2><p>Raridade: {{rarity}}</p><p>Set: {{set}}</p><p><a href='{{buyUrl}}'>Comprar</a></p>",
        text: "Nova carta: {{name}}",
        version: 1,
        isActive: true
    );
    templates.UpsertAsync(t, CancellationToken.None).GetAwaiter().GetResult();

    var trig = EmailTrigger.Create(
        alias: "digimon-release",
        templateKey: "digimon.release.ptBR",
        lang: "pt-BR",
        authType: "none",
        authSecret: null,
        defaultRecipients: new[] { "leo.passos@example.com" }, 
        mappingConfigJson: null,
        enabled: true
    );
    triggers.AddAsync(trig, CancellationToken.None).GetAwaiter().GetResult();
}