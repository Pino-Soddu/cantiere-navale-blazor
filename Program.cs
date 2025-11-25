using CNP.Data;
using CNP.Login.Services;
using CNP.Pubblica.Services;
using CNP.SchedaLavorazione.Services;
using CNP.Amministrazione.Services;
using CNP.Segreteria.Services;
using CNP.Clienti.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Configurazione dati aziendali da appsettings.json
builder.Services.Configure<CNP.Models.Configurazione.DatiAzienda>(
    builder.Configuration.GetSection("DatiAzienda"));

// Add services to the container.
builder.Services.AddRazorPages();

// Registrazione servizio chat pubblica
builder.Services.AddHttpClient<IChatPubblicaService, ChatPubblicaService>();

//  Registrazione servizi database ADO.NET
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// Registra il servizio di Gestione Anagrafi della segreteria
builder.Services.AddScoped<CNP.Segreteria.Services.AnagraficheService>();

// Registra il servizio di Gestione della Scheda di Lavorazione (vecchio)
builder.Services.AddScoped<CNP.SchedaLavorazione.Services.PreventiviService>();

// Registra il servizio per Area Amministrazione (nuovo)
builder.Services.AddScoped<CNP.Amministrazione.Services.PreventiviService>();

// Registra i servizi di Gestione dei Nuovi Preventivi
builder.Services.AddScoped<CNP.Amministrazione.Services.ClientiService>();
builder.Services.AddScoped<CNP.Amministrazione.Services.ImbarcazioniService>();
builder.Services.AddScoped<CNP.Amministrazione.Services.DettaglioPreventiviService>();


// Registra il servizio di Gestione Chat Clienti 
builder.Services.AddHttpClient<IAssistenteClienteService, AssistenteClienteService>();

// Registra i servizi di Gestione del Login
builder.Services.AddScoped<CredenzialiClienteService>();
builder.Services.AddScoped<ILoginService, LoginService>();

// Registra il servizio di Gestione dei modelli dei preventivi 
builder.Services.AddScoped<ModelliPreventiviService>();

// Registra i servizi di Gestione caricamento griglia preventivi 
builder.Services.AddScoped<AttivitaService>();
builder.Services.AddScoped<SubAttivitaService>();

// Registra il servizio di Gestione caricamento della tabella Materiali
builder.Services.AddScoped<MaterialiService>();

// Registra il servizio di Gestione caricamento della tabella Unità Misura
builder.Services.AddScoped<UnitaMisuraService>();

// Registra il servizio di calcolo degli algoritmi dei preventivi 
builder.Services.AddScoped<CalcoloService>();

// Registra il servizio di gestione invio Email dei preventivi 
builder.Services.AddScoped<EmailService>();

// Registra il servizio di generazione del preventivo in formato .pdf
builder.Services.AddScoped<PdfService>();

// Registra il servizio di generazione della Scheda di Lavorazione
builder.Services.AddScoped<SchedaLavorazioneService>();

// REGISTRAZIONE SINGLETON PER STATO CONDIVISO
builder.Services.AddSingleton<GestoreStatoLogin>();
builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<GestoreStatoLogin>());


// Abilita il sistema di autorizzazione
builder.Services.AddAuthorizationCore();

builder.Services.AddServerSideBlazor();
var app = builder.Build();


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();