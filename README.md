# IoVoto
Lo sviluppo del Portale Voto elettronico nasce dalla volontà di la volontà di fornire una soluzione che supporti lo svolgimento della sperimentazione del voto elettronico, in aggiunta agli strumenti già in uso, in occasione delle prossime elezioni COM.IT.ES. (Comitati degli italiani all’estero). La sperimentazione coinvolgerà alcune sedi pilota che saranno scelte dall’Amministrazione.

Il Portale Voto Elettronico consiste in due applicazioni distinte, APP A e APP B che condividono le medesime basi di dati (ledgers) situate sui tenants Oracle, mantenendo separati però rigidamente i dati dell’espressione di voto dai dati relativi agli elettori e tutto quello il contenuto delle liste elettorali.
Entrambi gli applicativi sono aperti al pubblico e gestiscono le operazioni effettuate da utenti esterni (autenticazione, espressione del voto online), il processo si svolge in due applicativi separati per garantire l’anonimizzazione del processo di voto.
 
![image](https://user-images.githubusercontent.com/92863367/138122447-a875779d-ea6d-465d-a21d-c97e85ccdd40.png)
