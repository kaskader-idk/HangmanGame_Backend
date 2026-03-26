//----------------------------------------
// .Net Core WebApi project create script 
//           v10.1.3 from 2026-02-23
//   (C)Robert Grueneis/HTL Grieskirchen 
//----------------------------------------

using GrueneisR.RestClientGenerator;

using Microsoft.OpenApi;
using Microsoft.Identity.Client;


string corsKey = "_myCorsKey";
string swaggerVersion = "v1";
string swaggerTitle = "Backend";
string restClientFolder = Environment.CurrentDirectory;
string restClientFilename = "_requests.http";
string baseUrl = "hangmanbackend-f2eqd3cvexbgbchg.polandcentral-01.azurewebsites.net"; // Enter Backend URL

var builder = WebApplication.CreateBuilder(args);

#region -------------------------------------------- ConfigureServices

builder.Services
  .AddEndpointsApiExplorer()
  .AddAuthorization()
  .AddSwaggerGen(x => x.SwaggerDoc(
    swaggerVersion,
    new OpenApiInfo { Title = swaggerTitle, Version = swaggerVersion }
  ))
  .AddCors(options => options.AddPolicy(
    corsKey,
    x => x.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials()
  ))
  .AddRestClientGenerator(options => options
    .SetFolder(restClientFolder)
    .SetFilename(restClientFilename)
    .SetAction($"swagger/{swaggerVersion}/swagger.json")
  //.EnableLogging()
  );
builder.Services.AddLogging(x => x.AddCustomFormatter());

string? connectionString = builder.Configuration.GetConnectionString("HangmanDB");
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"++++ ConnectionString: {connectionString}");
Console.ResetColor();
builder.Services.AddDbContext<HangmanDBContext>(options => options.UseSqlServer(connectionString));
#endregion

var app = builder.Build();

#region -------------------------------------------- Middleware pipeline
app.UseDeveloperExceptionPage();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"++++ Swagger enabled: {baseUrl}");
app.UseSwagger();
app.UseSwaggerUI(x => x.SwaggerEndpoint($"/swagger/{swaggerVersion}/swagger.json", swaggerTitle));
Console.ResetColor();

if (app.Environment.IsDevelopment())
{
    Console.WriteLine($@"++++ RestClient generating (after first request) to {restClientFolder}\{restClientFilename}");
    app.UseRestClientGenerator();
}

app.UseCors(corsKey);
//app.UseHttpsRedirection();
app.UseAuthorization();
#endregion

app.Map("/", () => Results.Redirect("/swagger"));
app.MapDbTests();

app.MapPost("/word", (HangmanDBContext db, WordDTO word) =>
{
    var entity = new HangmanWoerter
    {
        Wort = word.Wort,
        Schwierigkeit = word.Schwierigkeit,
        Beschreibung = word.Beschreibung
    };
    db.Add(entity);
    db.SaveChanges();
});

app.MapGet("/easy", (HangmanDBContext db) =>
{
    var easyWords = db.HangmanWoerters
        .Where(x => x.Schwierigkeit == "LEICHT")
        .ToList();

    var random = easyWords[Random.Shared.Next(easyWords.Count)];
    return new WordDTO().CopyFrom(random);
});

app.MapGet("/medium", (HangmanDBContext db) =>
{
    var easyWords = db.HangmanWoerters
        .Where(x => x.Schwierigkeit == "MITTEL")
        .ToList();

    var random = easyWords[Random.Shared.Next(easyWords.Count)];
    return new WordDTO().CopyFrom(random);
});

app.MapGet("/hard", (HangmanDBContext db) =>
{
    var easyWords = db.HangmanWoerters
        .Where(x => x.Schwierigkeit == "SCHWER")
        .ToList();

    var random = easyWords[Random.Shared.Next(easyWords.Count)];
    return new WordDTO().CopyFrom(random);
});

app.MapPut("/fix-umlauts", (HangmanDBContext db) =>
{
    // Häufige fehlerhafte Encoding-Muster für deutsche Umlaute (UTF-8 als Latin-1 interpretiert)
    var replacements = new Dictionary<string, string>
    {
        { "Ã¤", "ä" }, { "Ã„", "Ä" },
        { "Ã¶", "ö" }, { "Ã–", "Ö" },
        { "Ã¼", "ü" }, { "Ãœ", "Ü" },
        { "ÃŸ", "ß" },
        // Alternative Encoding-Fehler
        { "├ñ", "ä" }, { "├ä", "Ä" },
        { "├Â", "ö" }, { "├û", "Ö" },
        { "├╝", "ü" }, { "├£", "Ü" },
    };

    var wordsToFix = db.HangmanWoerters
        .AsEnumerable() // Client-side Evaluation für String-Operationen
        .Where(w => replacements.Keys.Any(pattern => w.Beschreibung.Contains(pattern)))
        .ToList();

    if (wordsToFix.Count == 0)
    {
        return Results.Ok(new
        {
            Message = "Keine Wörter mit fehlerhaften Umlauten gefunden.",
            FixedWords = Array.Empty<WordDTO>()
        });
    }

    foreach (var word in wordsToFix)
    {
        foreach (var (wrong, correct) in replacements)
        {
            word.Beschreibung = word.Beschreibung.Replace(wrong, correct);
        }
    }

    db.SaveChanges();

    return Results.Ok(new
    {
        Message = $"{wordsToFix.Count} Wörter korrigiert.",
        FixedWords = wordsToFix.Select(w => new WordDTO().CopyFrom(w))
    });
});

app.MapPut("/updateword", (HangmanDBContext db, string wort, string beschreibung, string schwierigkeit) =>
{
    schwierigkeit = schwierigkeit.ToUpper();
    if (schwierigkeit is not ("LEICHT" or "MITTEL" or "SCHWER"))
        return Results.BadRequest("Schwierigkeit muss LEICHT, MITTEL oder SCHWER sein.");

    wort = wort.ToUpper();

    var entity = db.HangmanWoerters.FirstOrDefault(w => w.Wort == wort);
    if (entity == null)
        return Results.NotFound(new { Message = "Wort nicht gefunden." });

    entity.Beschreibung = beschreibung;
    entity.Schwierigkeit = schwierigkeit;

    db.SaveChanges();
    return Results.Ok(new WordDTO().CopyFrom(entity));
});

app.MapDelete("/deleteword", (HangmanDBContext db, string wort) =>
{
    wort = wort.ToUpper();
    var entity = db.HangmanWoerters.FirstOrDefault(w => w.Wort == wort);

    if (entity == null)
        return Results.NotFound(new { Message = "Wort nicht gefunden." });

    db.Remove(entity);
    db.SaveChanges();

    return Results.Ok(new { Message = "Wort erfolgreich gelöscht." });
});

Console.WriteLine($"Ready for clients at {DateTime.Now:HH:mm:ss} ...");
app.Run();