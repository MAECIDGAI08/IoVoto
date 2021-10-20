using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace AppA.Models
{
    public class LedgerValidationNumber
    {
        public List<LedgerValidationNumber> ValidationNumberLista;
        // assets: 
        // - name: ValidationNumber 

        // properties: 
        [Required]
        public string Pseudonimo { get; set; }
        // e' ID su BC

        [Required]
        public string ValidationNumber { get; set; }

        [Required]
        public string Comites { get; set; }
        // era segnato come stringa

        [Required]
        public bool Used { get; set; }
      
    }

}
