using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;
using GameTheoryAlgorithm.Methods;

namespace GameTheoryAlgorithm;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int _a;
    private int _b;

    public MainWindow()
    {
        _a = 3;
        _b = 3;

        InitializeComponent();
        GenerateTransportGrid();
        PreDefine();
    }

    private void AComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _a = int.Parse((AComboBox.SelectedItem as ComboBoxItem)!.Content.ToString()!);
        GenerateTransportGrid();
    }

    private void BComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _b = int.Parse((BComboBox.SelectedItem as ComboBoxItem)!.Content!.ToString()!);
        GenerateTransportGrid();
    }

    private void GenerateTransportGrid()
    {
        TransportGrid ??= new Grid();
        TransportGrid.RowDefinitions.Clear();
        TransportGrid.ColumnDefinitions.Clear();
        TransportGrid.Children.Clear();

        for (int i = 0; i <= _a; i++)
        {
            TransportGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        for (int j = 0; j <= _b; j++)
        {
            TransportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        // Create headers (top row and left column)
        for (int i = 1; i <= _a; i++)
        {
            var supplierHeader = new TextBox
            {
                Text = "A" + i,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 17, 153)),
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Background = Brushes.LightGray
            };
            Grid.SetRow(supplierHeader, i);
            Grid.SetColumn(supplierHeader, 0);
            TransportGrid.Children.Add(supplierHeader);
        }

        for (int j = 1; j <= _b; j++)
        {
            var consumerHeader = new TextBox
            {
                Text = "B" + j,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 17, 153)),
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Background = Brushes.LightGray
            };
            Grid.SetRow(consumerHeader, 0);
            Grid.SetColumn(consumerHeader, j);
            TransportGrid.Children.Add(consumerHeader);
        }

        // Fill grid cells with TextBoxes for input (except the first row/column for headers)
        for (int i = 1; i <= _a; i++)
        {
            for (int j = 1; j <= _b; j++)
            {
                TextBox cell = new TextBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    MinWidth = 30,
                    MinHeight = 30,
                    TextAlignment = TextAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush(Color.FromRgb(221, 204, 255)),
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(102, 17, 153))
                };

                // Attach the validation event handler to each TextBox
                cell.PreviewTextInput += Cell_PreviewTextInput;

                Grid.SetRow(cell, i);
                Grid.SetColumn(cell, j);
                TransportGrid.Children.Add(cell);
            }
        }
    }

    // Event handler for TextBox validation: allows only numeric input
    private void Cell_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+"); // Only digits are allowed
        e.Handled = regex.IsMatch(e.Text);
    }
    private void SolveButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Read game matrix from the grid
        decimal[,] gameMatrix = new decimal[_a, _b];
        for (int i = 1; i <= _a; i++)
        {
            for (int j = 1; j <= _b; j++)
            {
                TextBox cell = (TextBox)TransportGrid.Children
                    .Cast<UIElement>()
                    .FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);
                gameMatrix[i - 1, j - 1] = decimal.Parse(cell.Text);
            }
        }

        // Solve using GameTheory algorithm
        var gameTheory = new GameTheory(gameMatrix, this);
        (List<decimal> aStrategy, List<decimal> bStrategy, decimal gamePrice)  = gameTheory.Solve();
        aStrategy = aStrategy.Select(x => Math.Round(x, 2)).ToList();
        bStrategy = bStrategy.Select(x => Math.Round(x, 2)).ToList();
        gamePrice = Math.Round(gamePrice, 2);

        // Display result (total cost)
        MessageBox.Show(
            $"Strategy for A: ({string.Join("; ", aStrategy)})" +
                        $"\nStrategy for B: ({string.Join("; ", bStrategy)})" +
                        $"\nGame price: {gamePrice}", "Result");
    }

    public void CreateAndAddDynamicGridSimplex(decimal[,] constraints, decimal[] cb, Dictionary<int, decimal> plan,
        decimal[] deriv, decimal[] delta, int[] myBase)
    {
        // Create a new grid
        Grid dynamicGrid = new Grid
        {
            Margin = new Thickness(10)
        };

        int rows = constraints.GetLength(0) + 2;
        int cols = constraints.GetLength(1) + 4;

        // Define rows and columns for the grid
        for (int i = 0; i < rows; i++)
        {
            dynamicGrid.RowDefinitions.Add(new RowDefinition());
        }

        for (int j = 0; j < cols; j++)
        {
            dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }


        CreateCell(dynamicGrid, 0, 0, "Base", true);
        CreateCell(dynamicGrid, 0, 1, "Cb", true);
        CreateCell(dynamicGrid, 0, 2, "Plan", true);

        for (int i = 3; i < constraints.GetLength(1) + 3; i++)
        {
            CreateCell(dynamicGrid, 0, i, string.Format("x" + (i - 2)), true);
        }

        CreateCell(dynamicGrid, 0, cols - 1, "der", true);

        for (int i = 1; i < deriv.Length + 1; i++)
        {
            CreateCell(dynamicGrid, i, cols - 1, Math.Round(deriv[i - 1], 2).ToString());
        }

        for (int i = 1; i < myBase.Length + 1; i++)
        {
            CreateCell(dynamicGrid, i, 0, string.Format("x" + (myBase[i - 1] + 1)), true);
        }

        for (int i = 1; i < myBase.Length + 1; i++)
        {
            CreateCell(dynamicGrid, i, 1, string.Format(cb[i - 1].ToString()));
        }

        for (int i = 1; i < myBase.Length + 1; i++)
        {
            CreateCell(dynamicGrid, i, 2, string.Format(Math.Round(plan[myBase[i - 1]], 2).ToString()));
        }


        for (int i = 1; i < constraints.GetLength(0) + 1; i++)
        {
            for (int j = 3; j < constraints.GetLength(1) + 3; j++)
            {
                CreateCell(dynamicGrid, i, j, Math.Round(constraints[i - 1, j - 3], 2).ToString());
            }
        }

        CreateCell(dynamicGrid, rows - 1, 0, "F*" , true);
        CreateCell(dynamicGrid, rows - 1, 1, "0");
        CreateCell(dynamicGrid, rows - 1, 2, "0");

        for (int i = 3; i < delta.Length + 3; i++)
        {
            CreateCell(dynamicGrid, rows - 1, i, Math.Round(delta[i - 3], 2).ToString());
        }

        CreateCell(dynamicGrid, rows - 1, cols - 1, "0");

        // Add the dynamic grid to the parent container (StackPanel)
        DynamicGridContainer.Children.Add(dynamicGrid);
    }

    private void CreateCell(Grid dynamicGrid, int x, int y, string text, bool isHeader = false,  Brush? color = null)
    {
        TextBox textBox = new TextBox()
        {
            Text = text,
            Width = 40,
            Height = 40,
            Margin = new Thickness(5),
            FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
            Foreground = isHeader == true ? new SolidColorBrush(Color.FromRgb(102, 17, 153)) : Brushes.Black,
            Background = isHeader == false ? new SolidColorBrush(Color.FromRgb(221, 204, 255)) : Brushes.LightGray,
            BorderThickness = isHeader == false ? new Thickness(2) : new Thickness(0),
            BorderBrush = new SolidColorBrush(Color.FromRgb(102, 17, 153)),
            TextAlignment = TextAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            IsReadOnly = true // Make the TextBox read-only to prevent editing
        };

        dynamicGrid.Children.Add(textBox);

        Grid.SetRow(textBox, x);
        Grid.SetColumn(textBox, y);
    }

    private void PreDefine()
    {
        // Test
        // decimal[,] gameMatrix = {
        //     {1M, 4, 6, 3, 7},
        //     { 3, 1, 2, 4, 3},
        //     { 2, 3, 4, 3, 5},
        //     { 0, 1, 5, 2, 6}
        // };
        //     
        // AComboBox.SelectedValue = "4";
        // BComboBox.SelectedValue = "5";
        
        //Andriy
        // decimal[,] gameMatrix = {
        //     { 9M, 3, 1, 4, 3, 2 },
        //     { 7, 0, 3, 2, 3, 1 },
        //     { 4, 5, 2, 7, 3, 3 },
        //     { 3, 6, 7, 5, 4, 2 },
        //     { 4, 3, 5, 4, 2, 1 },
        //     { 6, 5, 4, 5, 5, 6 }
        // };
        //
        // AComboBox.SelectedValue = "6";
        // BComboBox.SelectedValue = "6";
        
        
        // Danik
        decimal[,] gameMatrix = 
        {
            { 4, 0, 3, 1, 1, 4 },
            { 8, 9, 5, 1, 4, 8 },
            { 1, 3, 4, 6, 8, 4 },
            { 4, 4, 4, 6, 7, 5 },
            { 9, 6, 3, 7, 7, 7 },
            { 2, 6, 3, 5, 6, 3 }
        };

            
        AComboBox.SelectedValue = "6";
        BComboBox.SelectedValue = "6";
        

        for (int i = 1; i <= _a; i++)
        {
            for (int j = 1; j <= _b; j++)
            {
                ((TextBox)GetGridElement(TransportGrid, i, j)).Text = gameMatrix[i - 1, j - 1].ToString();
            }
        }

    }

    private UIElement GetGridElement(Grid grid, int row, int column)
    {
        foreach (UIElement element in grid.Children)
        {
            if (Grid.GetRow(element) == row && Grid.GetColumn(element) == column)
            {
                return element;
            }
        }
        return null;
    }
}
