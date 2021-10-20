using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace AppB.Models
{
    public class LedgerVoti
    {
        public List<LedgerVoti> VotiLista;
        // assets: 
        // - name: Elettore

        // properties: 
        [Required]
        public string UniqueID { get; set; }
        // e' ID su BC
        [Required]
        public string Lista { get; set; }
        [Required]
        public string Nome { get; set; }
        [Required]
        public string Cognome { get; set; } 
        [Required]
        public string DatadiNascita { get; set; }
        [Required]
        public string LuogodiNascita { get; set; }
        [Required]
        public int Ordine { get; set; }


        // siamo sicuri che basti un ID per il voto? ho qualche dubbio....

    }

}
