namespace AppB.Models
{
    public class ListaPartiti
    {
        #region  Campi
        public int idPartito { get; set; }
        public int idCandidato { get; set; }
        public string nomePartito { get; set; }
        public string imgPartito { get; set; }

        #endregion

        #region  Costruttore
        public ListaPartiti(int idPartito, string nomePartito, string logoPartito)
        {
            this.idPartito = idPartito;
            this.nomePartito = nomePartito;
            imgPartito = logoPartito;
        }

        //public ListaPartiti(int idPartito, string logoPartito)
        //{
        //    this.idPartito = idPartito;           
        //    this.imgPartito = logoPartito;
        //}
        #endregion
    }

    public class Candidati
    {
        #region  Campi
        public int idCandidato { get; set; }
        public string nomeCandidato { get; set; }
        public string dataNascita { get; set; }
        public string luogoNascita { get; set; }
        #endregion
        #region  costruttore
        public Candidati(int idCandidato, string nomeCandidato, string data, string luogo)
        {
            this.idCandidato = idCandidato;
            this.nomeCandidato = nomeCandidato;
            this.dataNascita = data;
            this.luogoNascita = luogo;
        }



        #endregion
    }

    public class DatiRiepilogo
    {
        public int lista { get; set; }
        public int[] candidati { get; set; }
    }
}
