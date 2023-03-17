# Giardino

## Ambiente di Sviluppo

Visual Studio ci offre un ambiente di sviluppo del MAUI (Multi-platform App UI).
MAUI è un framework di sviluppo software sviluppato da Microsoft per la creazione di applicazioni native multipiattaforma.
Il framework consente di scrivere simultaneamente il codice dell'interfaccia grafica dell'applicazione e il codice in linguaggio C# integrabile con la UI. 
Lo scopo principale di MAUI è poter eseguire applicazioni su più piattaforme senza la necessità di scrivere codice specifico per ogni sistema operativo.

## Obiettivo del Nostro Progetto

Sviluppare un'applicazione che abbia un giardino di 10x10 celle con degli spezzoni d'erba che copre ognuna di esse.
Al click di una delle celle questa viene scoperta, e se c'è una cacca perdi un paio di scarpe, altrimenti viene rivelata solamente come erbetta.
Attorno alla cacca ci dovranno essere delle mosche che segnalano il pericolo.
Hai a disposizione 3 paia di scarpe e una volta esaurite hai perso.
Per vincere devi essere passato su tutti i blocchi che contengono solamente l'erba verde.

## Setup del Progetto

## Processo di Sviluppo

### Creazione del Campo

Come prima cosa mi sono concentrato nel creare in un progetto a parte in C# Console, per simulare la creazione del campo d'erba in un ambiente in cui poter effettuare più rapidamente il debug del codice.

La matrice è istanziata con una `Rows = 10` righe e `Cols = 10` colonne.

```cs
int[,] grid = new int[Rows, Cols];
```

La matrice in questione verrà riempita di numeri interi, ognuno simboleggiante un elemento del campo d'erba :

- $\large\textcolor{green}{2}$ $\rightarrow$ Casella con Erba 
- $\large\textcolor{yellow}{1}$ $\rightarrow$ Casella con una Mosca
- $\large\textcolor{red}{0}$ $\rightarrow$ Casella con una Cacca

In ogni cella della `grid`, una volta che questa verrà iterata, avrà un 10% di possibilità di essere sostituita da una cacca e per far ciò utilizziamo una funzione di `Random` :

```cs
Random rnd = new Random();
if(rnd.Next(1, 101) < 10) grid[i, j] = 0; // Aggiunta delle Cacche
```

E alla fine attraverso un sistema di outlining la cacca verrà circondata con delle mosche :

```cs
int[] outline = {
	top, j, // Coordinate Casella in Alto
	top, right, // Coordinate Casella in Alto a Destra
	i, right, // Coordinate Casella a Destra
	bottom, right, // Coordinate Casella in Bassso a Destra
	bottom, j, // Coordinate Casella in Basso
	bottom, left, // Coordinate Casella in Basso a Sinistra
	i, left, // Coordinate Casella a Sinistra
	top, left // Coordinate Casella in Alto a Sinistra
};

for (int k = 0; k < outline.Length; k += 2) {
	if(grid[outline[k], outline[k + 1]] == 0) continue; // Salta le Caselle Contenenti gia delle Cacche
	if(grid[i, j] == 0) grid[outline[k], outline[k + 1]] = 1; // Aggiunta delle Mosche
}
```

Un possibile output in console su Visual Studio Code sarà :

![[matrix grid.png|350]]

### Creazione della Griglia

La matrice creata dovrò poi essere trasposta su MAUI con l'utilizzo di una griglia, definita in XAML dal costrutto `Grid`.
In questa griglia viene definito il numero di righe e colonne che il campo d'erba dovrà avere, quindi verranno create 10 `RowDefinition` e 10 `ColumnDefinition`.
La griglia avrà un `x:Name` che sarà "GridField", ossia il suo nome per venir identificato in codice C#.

```xml
<Grid x:Name="GridField">
	<Grid.RowDefinitions>
		<RowDefinition Height="*"></RowDefinition>
		<!-- RowDefinition Ripetuto Altre 9 Volte -->
	</Grid.RowDefinitions>
	<Grid.ColumnDefinitions>
		<ColumnDefinition Width="2*"></ColumnDefinition>
		<!-- ColumnDefinition Ripetuto Altre 9 Volte -->
    </Grid.ColumnDefinitions>
	
	<!-- Main Grid -->
</Grid>
```

E successivamente riempiremo la griglia con degli degli `ImageButton`, ideali per rispondere a dei comandi ogni volta che vengono cliccati.
L'immagine che sarà inserita come  `Source` sarà una figura di un ciuffo d'erba in bianco e nero (`erba_bw.png`).

```cs
ImageButton imgBtn = new() {
	Source = "erba_bw.png"
};
```

Poi al button verrà associato un `EventHandler`, ossia un gestore dell'evento click sul button stesso.

```cs
imgBtn.Clicked += HandleStep(imgBtn);
```

E infine il alla griglia si aggiunge l'`ImageButton`, con la primitiva `Add` , alla colonna `j` e alla riga `i`.

```cs
GridField.Add(imgBtn, j, i);
```

### Gestione dei Passi sul Campo
