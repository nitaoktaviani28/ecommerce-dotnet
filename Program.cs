using Ecommerce.MonitoringApp.Observability;
using Ecommerce.MonitoringApp.Handlers;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// OBSERVABILITY INIT (setara observability.Init() di Go)
// ======================================================
ObservabilityInit.Init(builder);

// ======================================================
// BUILD APP
// ======================================================
var app = builder.Build();

// ======================================================
// HTTP ROUTES (setara http.HandleFunc di Go)
// ======================================================
app.MapGet("/", HomeHandler.Handle);
app.MapPost("/checkout", CheckoutHandler.Handle);
app.MapGet("/success", SuccessHandler.Handle);

// ======================================================
// START SERVER
// ======================================================
app.Run();
