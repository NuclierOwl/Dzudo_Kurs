using Dzudo.Hardik.Connector.Date;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs_Dzudo.Hardik.Connector.Date
{
    public class Match
    {
        public string participant1_name { get; set; }
        public string participant2_name { get; set; }
        public string? winner_name { get; set; }
        public string? loser_name { get; set; }
        public int GroupId { get; set; }
        public int tatamiid { get; set; }

        public virtual UkhasnikiDao Participant1 { get; set; }
        public virtual UkhasnikiDao Participant2 { get; set; }
        public virtual UkhasnikiDao Winner { get; set; }
        public virtual UkhasnikiDao Loser { get; set; }
        public virtual GroupDao_2 Group { get; set; }

        [NotMapped]
        public string Info => $"{Participant1?.Name} vs {Participant2?.Name}";

        [NotMapped]
        public string CategoryInfo => $"Категория: {GroupId}";
    }
}