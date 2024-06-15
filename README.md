# trenitalia-quadri-orario

Per un progettino personale mi serviva avere una semplice lista degli orari dei treni di Trenitalia, ma, non trovando modo di ottenerli da nessuna parte, mi ero messo a studiare l'applicazione Quadri Orario.

Quadri Orario, alla prima apertura, scarica un database con cui costruisce tutti i quadri e lo copia nella cartella C:/ProgramData. Dentro l'archivio "dati.zip" sono presenti i file relativi ai vari quadri.

Per ottenere i dati a noi interessano solo i file "*_data.xml". Questi file hanno al loro interno tre nodi di nostro interesse, TIME, TRAIN e TABLELOCALITY.

Le stringhe sono tutte encodate in base64, ma decodificandole non otterrete alcun testo leggibile. 

Per capire cosa fossero ci ho messo 3 giorni (e tuttora ho capito solo cos'è un dato per parte) e la scoperta è stata totalmente casuale perché a un certo punto stavo per mollare ma mi è venuto in mente un articolo della wiki di KDE Itinerary che avevo letto in passato ( https://community.kde.org/KDE_PIM/KItinerary/Trenitalia_Barcode ) che parlava di come decodificare i codici AZTEC dei biglietti Trenitalia e allora ho pensato di convertire ognuna delle 3 tipologie di dati in binario e provare se coincidevano con valori noti (es. numero treno, id stazione)

Alla fine:
- il numero treno si ottiene prendendo i 4 byte da indice 4 a 7 delle stringhe in TRAIN e convertendoli in un int32, poi usando questo indice per prendere il numero dall'array di stringhe che si ottiene da stringTable.xml
- la località (che è rappresentata dal codice UIC della stazione) si ottiene prendendo gli ultimi 4 byte delle stringhe in TABLELOCALITY e convertendoli in un int32 (poi si può ottenere il nome facendo un dictionary<int, string> usando i dati nel file "localityTable.xml"
- il tempo si ottiene prendendo gli ultimi 2 byte delle stringhe in TIME e convertendoli in un int16 (si ottiene il tempo in minuti, è stato tosto capire cos'era questo qua)

Questo è tutto quel che ho capito. Probabilmente studiando di più era possibile capire cosa fossero gli altri byte di quelle stringhe (presumo abbiano a che fare coi collegamenti legati a periodicità e avvisi), ma per quel che dovevo fare mi basta numero treno, stazione e ora di partenza.

Vi lascio il sorgente del programma che avevo fatto per ottenere una lista in JSON partendo da questi dati. 



Update
Quando il tempo è -1 o -2 vuol dire che il treno non ferma in una stazione

Ho realizzato anche che, se ci sono indicati treni in coincidenza, non vedo modo di distinguere fermate effettive del treno in esame e quelle del treno in coincidenza, quindi il programma li considera come fossero fermate dello stesso treno. Servirebbero rifiniture, ma non trovo come fare...
C'è anche la questione dei treni riportati più volte in base a modifiche di percorso, lavori, ecc...
