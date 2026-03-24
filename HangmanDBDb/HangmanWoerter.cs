using System;
using System.Collections.Generic;

namespace HangmanDBDb;

public partial class HangmanWoerter
{
    public int Id { get; set; }

    public string Wort { get; set; } = null!;

    public string Schwierigkeit { get; set; } = null!;

    public string Beschreibung { get; set; } = null!;
}
