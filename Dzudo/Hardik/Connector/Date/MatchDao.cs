using System;

namespace Kurs_Dzudo.Hardik.Connector.Date;
public class Match
{
    public int GroupId { get; set;}
    public int TatamiId { get; set;}
    public UkhasnikiDao Participant1 { get; set; }
    public UkhasnikiDao Participant2 { get; set; }
    public UkhasnikiDao Winner { get; set; }
    public UkhasnikiDao Loser { get; set; }

    public Match(UkhasnikiDao participant1, UkhasnikiDao participant2)
    {
        Participant1 = participant1 ?? throw new ArgumentNullException(nameof(participant1));
        Participant2 = participant2 ?? throw new ArgumentNullException(nameof(participant2));
    }

    public string Info => $"{Participant1.Name} vs {Participant2.Name}";

    public string CategoryInfo => $"Категория: {GroupId}";
}