using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace AppB.Models
{
    
    public class LedgerListe
    {
        [Required]
        public string Name { get; set; }
        // e' ID su BC

        [Required]
        public string Pic { get; set; }
        // basata 64
      
        [Required]
        public string Comites { get; set; }

        [Required]
        public int TimeZone { get; set; }

        [Required]
        public int MaxSel { get; set; }

        [Required]
        public int Ordine { get; set; }
    }



}


