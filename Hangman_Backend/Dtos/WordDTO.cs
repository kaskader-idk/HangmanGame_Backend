namespace Hangman_Backend.Dtos
{
    public class WordDTO
    {
        public string Wort { get; set; } = null!;

        public string Schwierigkeit { get; set; } = null!;

        public string Beschreibung { get; set; } = null!;
    }
}