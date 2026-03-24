namespace HangmanBackend.Maps;
public static class DbTestApi
{
  public record struct OkMessage(bool IsOk, int Nr);
  public static IEndpointRouteBuilder MapDbTests(this IEndpointRouteBuilder routes)
  {
    routes.MapGet("/dbtest/HangmanDB", (HangmanDBContext db, ILoggerFactory logger) =>
    {
      int nr = db.HangmanWoerters.Count();
      logger.Log($"{nr} HangmanDB");
      return new OkMessage { IsOk = true, Nr = nr };
    });
    return routes;
  }
}