using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace AppA.Models
{
    [Serializable]
    public class LedgerElettori
    {
        public List<LedgerElettori> ElettoriLista;
        // assets: 
        // - name: Elettore

        // properties: 
        [Required]
        public string UniqueID { get; set; }
        // e' ID su BC

        [Required]
        public string Nome { get; set; }
     
        [Required]
        public string Cognome { get; set; }
       
        // not required
        public string CodiceFiscale { get; set; }

        [Required]
        public string Sesso { get; set; }

        [Required]
        public string DatadiNascita { get; set; }

        [Required]
        public string LuogodiNascita { get; set; }

        [Required]
        public string CodiceElettore { get; set; }

        [Required]
        public string COMITES { get; set; }

        [Required]
        public string Pseudonimo { get; set; }

        [Required]
        public int UTC { get; set; }

    }

}
