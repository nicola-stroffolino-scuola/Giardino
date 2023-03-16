using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Diagnostics;
using System.Linq;

namespace Giardino;

public partial class App : Application {
    protected override Window CreateWindow(IActivationState activationState) {
        Window window = base.CreateWindow(activationState);

        window.Height = 696;
        window.Width = 896;

        return window;
    }
}

public partial class MainPage : ContentPage {

    int Rows { get; }
    int Cols { get; }
    char Gender { get; set; }
    Image[] Shoes { get; set; }
    int Lives { get; set; }
    int Grass { get; set; }
    int Flies { get; set; }
    int Poos { get; set; }
    int[,] Field { get; set; }
    
    public MainPage() {
		InitializeComponent();
        Shoes = new Image[3] { Shoe1, Shoe2, Shoe3 };
        Rows = 10;
        Cols = 10;
        for (int i = 0; i < Rows; i++) GridField.RowDefinitions.Add(new RowDefinition());
        for (int i = 0; i < Cols; i++) GridField.ColumnDefinitions.Add(new ColumnDefinition());
        
        InitializeGame();
    }
    public void InitializeGame() {
        Lives = 3;
        Field = GenerateField();
        // Riempi i contatori
        Grass = Field.Cast<int>().Count(x => x == 2);
        Flies = Field.Cast<int>().Count(x => x == 1);
        Poos = Field.Cast<int>().Count(x => x == 0);

        SpawnGridInPage();
        UpdateInfo();
    }

    private int[,] GenerateField() {
        int[,] grid = new int[Rows, Cols];
        Random rnd = new();

        for (int i = 0; i < Rows; i++) {
            for (int j = 0; j < Cols; j++) {
                grid[i, j] = 2;
            }
        }

        for (int i = 0; i < Rows; i++) {
            for (int j = 0; j < Cols; j++) {
                if (rnd.Next(1, 101) < 10) grid[i, j] = 0;

                int top = i - 1, bottom = i + 1,
                    left = j - 1, right = j + 1;

                // Controllo delle Coordinate Disponibili
                if (top < 0) top = bottom;
                if (bottom > Rows - 1) bottom = top;
                if (left < 0) left = right;
                if (right > Cols - 1) right = left;

                int[] outline = {
                    top, j,
                    top, right,
                    i, right,
                    bottom, right,
                    bottom, j,
                    bottom, left,
                    i, left,
                    top, left
                };

                for (int k = 0; k < outline.Length; k += 2) {
                    if (grid[outline[k], outline[k + 1]] == 0) continue;
                    if (grid[i, j] == 0) grid[outline[k], outline[k + 1]] = 1;
                }
            }
        }

        return grid;
    }

    private void SpawnGridInPage() {
        for (int i = 0; i < Rows; i++) {
            for (int j = 0; j < Cols; j++) {

                ImageButton imgBtn = new() {
                    Source = "erba_bw.png"
                };

                // Metodo che gestisce tutto il funzionamento del gioco
                // HandleStep gestisce l'esito dei passi che compi sull'erba
                imgBtn.Clicked += HandleStep(imgBtn);

                GridField.Add(imgBtn, j, i);
            }
        }
    }

    private EventHandler HandleStep(ImageButton imgBtn) {
        return new ((s, e) => {
            int imgRow = GridField.GetRow(imgBtn);
            int imgCol = GridField.GetColumn(imgBtn);

            // Controllo se sei ancora in vita e se non hai ancora
            // finito tutte le caselle grass o flies disponibili
            if (Lives != 0 && (Grass != 0 || Flies != 0)) {
                switch (Field[imgRow, imgCol]) {
                // Se pesti la cacca
                case 0:
                    if (Lives != 0) {
                        Lives--;
                        Shoes[Lives].Source = Gender == 'm' ? "cacca_pestata.png" : "donna_che_pesta.png";
                    }
                    Poos--;
                    imgBtn.Source = "cacca.png";
                    break;
                // Se pesti sulle mosche
                case 1:
                    Flies--;
                    imgBtn.Source = "cartoon_fly.png";
                    break;
                // Se pesti l'erba
                case 2:
                    Grass--;
                    imgBtn.Source = "erba.png";
                    break;
                }
            }

            if (Lives == 0) {
                DisplayAlert("Errore", "Vite Terminate, Impossibile Proseguire", "OK");
                return;
            }

            if (Grass == 0 && Flies == 0) {
                DisplayAlert("Congratulazioni", "Hai Vinto!", "OK");
                return;
            }

            Field[imgRow, imgCol] = -1;
            UpdateInfo();
        });
    }

    private void UpdateInfo() {
        GrassLbl.Text = $"Grass: {Grass}";
        FliesLbl.Text = $"Flies: {Flies}";
        PoosLbl.Text  = $"Poos: {Poos}";
    }

    private void ManClicked(object sender, EventArgs e) {
        Gender = 'm';
        foreach (var shoe in Shoes) {
            shoe.Source = "scarpa.png";
        }
        ChooseGenderPage.IsVisible = false;
    }

    private void WomanClicked(object sender, EventArgs e) {
        Gender = 'f';
        foreach (var shoe in Shoes) {
            shoe.Source = "high_heel_shoes.png";
        }
        ChooseGenderPage.IsVisible = false;
    }

    private void Restart(object sender, EventArgs e) {
        Thread x = new (() => {
            for (int i = 0; i < Rows; i++) {
                for (int j = 0; j < Cols; j++) {
                    Dispatcher.Dispatch(() => {
                        GridField.RemoveAt(0);
                    });
                    Thread.Sleep(1);
                }
            }
        });
        x.Start();
        InitializeGame();
        if (Gender == 'm') ManClicked(sender, e);
        else WomanClicked(sender, e);
    }
}

