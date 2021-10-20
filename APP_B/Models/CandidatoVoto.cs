using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

// modello per invio dei candidati nel metodo SetVoto

namespace AppB.Models
{
    public class CandidatoVoto
    {
        [Required]
        private string NomeCandidato;

        [Required]
        private string CognomeCandidato;

        [Required]
        private string DataNascitaCandidato;

        [Required]
        private string LuogoNascitaCandidato;

        [Required]
        private int Ordine;

        [Required]
        private string Comites;

        public string nomeCandidato             { get => NomeCandidato; set => NomeCandidato = value; }

        public string cognomeCandidato          { get => CognomeCandidato; set => CognomeCandidato = value; }

        public string dataNascitaCandidato      { get => DataNascitaCandidato; set => DataNascitaCandidato = value; }

        public string luogoNascitaCandidato     { get => LuogoNascitaCandidato; set => LuogoNascitaCandidato = value; }

        public int ordine                       { get => Ordine; set => Ordine = value; }

        public string comites                   { get => Comites; set => Comites = value; }

        

    }

}
