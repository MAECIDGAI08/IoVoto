using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace AppB.Models
{
    [Serializable]
    public class LedgerCandidati
    {
        public List<LedgerCandidati> CandidatiLista;

        // BC assets: 
        //- name: Candidato

        [Required]
        public string UniqueID { get; set; }
        // e' ID su BC
        [Required]
        public string Nome { get; set; }
        [Required]
        public string Cognome { get; set; }
        [Required]
        public string DatadiNacita { get; set; }
        [Required]
        public string LuogodiNacita { get; set; }
        [Required]
        public int Ordine { get; set; }
        [Required]
        public string Lista { get; set; }
    }

}
