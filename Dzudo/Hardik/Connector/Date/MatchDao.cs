using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs_Dzudo.Hardik.Connector.Date
{
    public class Match
    {
        public int GroupId { get; set; }
        public int TatamiId { get; set; }

        [ForeignKey("Participant1")]
        public string Participant1Name { get; set; }
        public UkhasnikiDao Participant1 { get; set; }

        [ForeignKey("Participant2")]
        public string Participant2Name { get; set; }
        public UkhasnikiDao Participant2 { get; set; }

        [ForeignKey("Winner")]
        public string WinnerName { get; set; }
        public UkhasnikiDao Winner { get; set; }

        [ForeignKey("Loser")]
        public string LoserName { get; set; }
        public UkhasnikiDao Loser { get; set; }

        public Match(UkhasnikiDao participant1, UkhasnikiDao participant2)
        {
            Participant1 = participant1 ?? throw new ArgumentNullException(nameof(participant1));
            Participant2 = participant2 ?? throw new ArgumentNullException(nameof(participant2));
            Participant1Name = participant1.Name;
            Participant2Name = participant2.Name;
        }

        [NotMapped]
        public string Info => $"{Participant1?.Name} vs {Participant2?.Name}";

        [NotMapped]
        public string CategoryInfo => $"Категория: {GroupId}";
    }
}