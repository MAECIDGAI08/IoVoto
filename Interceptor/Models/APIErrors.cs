using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Interceptor.Models
{
    // classe generica per gestire gli errori standard dati dalle API
    [Serializable]
    public class APIErrors
    {
        private int errorCode;
        
        private string errorDescription;

        public int ErrorCode { get => errorCode; set => errorCode = value; }

        public string ErrorDescription { get => errorDescription; set => errorDescription = value; }

    }
}
