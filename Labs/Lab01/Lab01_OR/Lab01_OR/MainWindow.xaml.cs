// File: MainWindow.xaml.cs
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Lab01_OR.Methods;
using static System.Net.Mime.MediaTypeNames;

namespace Lab01_OR
{
    public partial class MainWindow : Window
    {
        private int numberOfConstraints = 3;
        private int numberOfVariables = 3;

        public MainWindow()
        {
            InitializeComponent();
            SetupGrid();
            PreDefine();
        }

        private void ConstraintsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            numberOfConstraints = Convert.ToInt32(((ComboBoxItem)((ComboBox)sender).SelectedItem).Content);
            SetupGrid();
        }

        private void VariablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            numberOfVariables = Convert.ToInt32(((ComboBoxItem)((ComboBox)sender).SelectedItem).Content);
            SetupGrid();
        }

        private void SetupGrid()
        {
            // Clear previous elements
            if(ObjectiveFunctionGrid == null)
                ObjectiveFunctionGrid = new Grid();

            if (ConstraintsGrid == null)
                ConstraintsGrid = new Grid();

            ObjectiveFunctionGrid.Children.Clear();
            ConstraintsGrid.Children.Clear();

            // Create grid for the objective function
            ObjectiveFunctionGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < numberOfVariables; i++)
            {
                ObjectiveFunctionGrid.ColumnDefinitions.Add(new ColumnDefinition());

                TextBox variableLabel = new TextBox
                {
                    Width = 30,
                    Height = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };
                ObjectiveFunctionGrid.Children.Add(variableLabel);
                Grid.SetColumn(variableLabel, i);

            }

            // Create grid for the constraints
            ConstraintsGrid.RowDefinitions.Clear();
            ConstraintsGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < numberOfConstraints; i++)
            {
                ConstraintsGrid.RowDefinitions.Add(new RowDefinition());
                ConstraintsGrid.ColumnDefinitions.Add(new ColumnDefinition());

                for (int j = 0; j < numberOfVariables; j++)
                {
                    TextBox constraintTextBox = new TextBox
                    {
                        Width = 40,
                        Margin = new Thickness(5)
                    };
                    ConstraintsGrid.Children.Add(constraintTextBox);
                    Grid.SetRow(constraintTextBox, i);
                    Grid.SetColumn(constraintTextBox, j);
                }

                ConstraintsGrid.ColumnDefinitions.Add(new ColumnDefinition());

                // Add inequality selector for each row
                ComboBox inequalityComboBox = new ComboBox
                {
                    Width = 50,
                    Margin = new Thickness(5)
                };
                inequalityComboBox.Items.Add(new ComboBoxItem{Content = "<="});
                inequalityComboBox.Items.Add(new ComboBoxItem{Content = ">="});
                inequalityComboBox.SelectedValuePath = "Content";
                inequalityComboBox.SelectedIndex = 0;
                ConstraintsGrid.Children.Add(inequalityComboBox);
                Grid.SetRow(inequalityComboBox, i);
                Grid.SetColumn(inequalityComboBox, numberOfVariables);

                // Add result TextBox for each row
                TextBox resultTextBox = new TextBox
                {
                    Width = 40,
                    Margin = new Thickness(5)
                };
                ConstraintsGrid.Children.Add(resultTextBox);
                Grid.SetRow(resultTextBox, i);
                Grid.SetColumn(resultTextBox, numberOfVariables + 1);
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            // Collect input data and call Simplex method
            decimal[] objectiveFunction = new decimal[numberOfVariables];
            for (int i = 0; i < numberOfVariables; i++)
            {
                objectiveFunction[i] = Convert.ToDecimal(((TextBox)ObjectiveFunctionGrid.Children[i]).Text);
            }

            decimal[,] constraints = new decimal[numberOfConstraints, numberOfVariables];
            decimal[] results = new decimal[numberOfConstraints];
            string[] inequalities = new string[numberOfConstraints];

            for (int i = 0; i < numberOfConstraints; i++)
            {
                for (int j = 0; j < numberOfVariables; j++)
                {
                    constraints[i, j] = Convert.ToDecimal(((TextBox)GetGridElement(ConstraintsGrid, i, j)).Text);
                }

                inequalities[i] = ((ComboBox)GetGridElement(ConstraintsGrid, i, numberOfVariables)).Text;
                results[i] =
                    Convert.ToDecimal(((TextBox)GetGridElement(ConstraintsGrid, i, numberOfVariables + 1)).Text);
            }

            //Clear all tables
            DynamicGridContainer.Children.Clear();

            // Call Simplex algorithm to solve
            Method? method = null;
            var methodString = MethodBox.Text;
            if (methodString == "Simplex")
            {
                method = new Simplex(objectiveFunction, constraints, inequalities, results, this);
            }
            else if (methodString == "DualSimplex")
            {
                method = new DualSimplex(objectiveFunction, constraints, inequalities, results, this);
            }
            else if (methodString == "BigM")
            {
                method = new BigM(objectiveFunction, constraints, inequalities, results, this);
            }
            else if (methodString == "CuttingPlane")
            {
                method = new CuttingPlane(objectiveFunction, constraints, inequalities, results, this);
            }

            decimal[]? solution = method?.Solve();
            double[] variables = new double[numberOfVariables];

            for (int i = 0; i < numberOfVariables; i++)
            {
                if (solution != null) variables[i] = Convert.ToDouble(solution[i]);
            }

            // Display the solution (can be improved to display more clearly)
            MessageBox.Show("Optimal solution: " + string.Join(", ", variables) + "\nF* = " +
                            solution?[^1]);
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
        public void CreateAndAddDynamicGridSimplex(decimal[,] constraints, decimal[] cb, Dictionary<int, decimal> plan, decimal[] deriv, decimal[] delta, int[] myBase)
        {
            // Create a new grid
            Grid dynamicGrid = new Grid
            {
                Margin = new Thickness(10),
                ShowGridLines = true // Optional: Show grid lines
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


            CreateCell(dynamicGrid, 0, 0, "Base");
            CreateCell(dynamicGrid, 0, 1, "Cb");
            CreateCell(dynamicGrid, 0, 2, "Plan");

            for(int i = 3; i < constraints.GetLength(1)+3; i++)
            {
                CreateCell(dynamicGrid, 0, i, string.Format("x" + (i - 2)));
            }

            CreateCell(dynamicGrid, 0, cols - 1, "der");

            for(int i = 1; i < deriv.Length + 1; i++)
            {
                CreateCell(dynamicGrid, i, cols - 1, deriv[i - 1].ToString());
            }

            for(int i = 1; i < myBase.Length + 1; i++)
            {
                CreateCell(dynamicGrid, i, 0, string.Format("x" + (myBase[i - 1] + 1)));
            }

            for(int i = 1; i < myBase.Length + 1; i++)
            {
                CreateCell(dynamicGrid, i, 1, string.Format(cb[i - 1].ToString()));
            }

            for(int i = 1; i < myBase.Length + 1; i++)
            {
                CreateCell(dynamicGrid, i, 2, string.Format(plan[myBase[i - 1]].ToString()));
            }


            for (int i = 1; i < constraints.GetLength(0) + 1; i++)
            {
                for(int j = 3; j < constraints.GetLength(1) + 3; j++)
                {
                    CreateCell(dynamicGrid, i, j, constraints[i - 1, j -3].ToString());
                }
            }

            CreateCell(dynamicGrid, rows - 1, 0, "F*");
            CreateCell(dynamicGrid, rows - 1, 1, "0");
            CreateCell(dynamicGrid, rows - 1, 2, "0");

            for(int i = 3; i < delta.Length + 3; i++)
            {
                CreateCell(dynamicGrid, rows - 1, i, delta[i - 3].ToString());
            }

            CreateCell(dynamicGrid, rows - 1, cols - 1, "0");

            // Add the dynamic grid to the parent container (StackPanel)
            DynamicGridContainer.Children.Add(dynamicGrid);
        }
        public void CreateAndAddDynamicGridDualSimplex(decimal[,] constraints, decimal[] cb, Dictionary<int, decimal> plan, decimal[] deriv, decimal[] delta, int[] myBase)
        {
            // Create a new grid
            Grid dynamicGrid = new Grid
            {
                Margin = new Thickness(10),
                ShowGridLines = true // Optional: Show grid lines
            };

            int rows = constraints.GetLength(0) + 3;
            int cols = constraints.GetLength(1) + 3;

            // Define rows and columns for the grid
            for (int i = 0; i < rows; i++)
            {
                dynamicGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int j = 0; j < cols; j++)
            {
                dynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }


            CreateCell(dynamicGrid, 0, 0, "Base");
            CreateCell(dynamicGrid, 0, 1, "Cb");
            CreateCell(dynamicGrid, 0, 2, "Plan");

            for(int i = 3; i < constraints.GetLength(1)+3; i++)
            {
                CreateCell(dynamicGrid, 0, i, string.Format("x" + (i - 2)));
            }

            CreateCell(dynamicGrid, rows-1, 0, "der");

            for(int i = 3; i < deriv.Length + 3; i++)
            {
                CreateCell(dynamicGrid, rows - 1, i, deriv[i - 3].ToString());
            }

            for(int i = 1; i < myBase.Length + 1; i++)
            {
                CreateCell(dynamicGrid, i, 0, string.Format("x" + (myBase[i - 1] + 1)));
            }

            for(int i = 1; i < myBase.Length + 1; i++)
            {
                CreateCell(dynamicGrid, i, 1, string.Format(cb[i - 1].ToString()));
            }

            for(int i = 1; i < myBase.Length + 1; i++)
            {
                CreateCell(dynamicGrid, i, 2, string.Format(plan[myBase[i - 1]].ToString()));
            }


            for (int i = 1; i < constraints.GetLength(0) + 1; i++)
            {
                for(int j = 3; j < constraints.GetLength(1) + 3; j++)
                {
                    CreateCell(dynamicGrid, i, j, constraints[i - 1, j -3].ToString());
                }
            }

            CreateCell(dynamicGrid, rows - 2, 0, "F*");
            CreateCell(dynamicGrid, rows - 2, 1, "0");
            CreateCell(dynamicGrid, rows - 2, 2, "0");

            for(int i = 3; i < delta.Length + 3; i++)
            {
                CreateCell(dynamicGrid, rows - 2, i, delta[i - 3].ToString());
            }

            CreateCell(dynamicGrid, rows - 2, cols - 1, "0");
            CreateCell(dynamicGrid, rows - 1, 1, "0");
            CreateCell(dynamicGrid, rows - 1, 2, "0");

            // Add the dynamic grid to the parent container (StackPanel)
            DynamicGridContainer.Children.Add(dynamicGrid);
        }

        private void CreateCell(Grid dynamicGrid, int x, int y, string text)
        {
            TextBox textBox = new TextBox()
            {
                Text = text,
                Width = 60,
                Height = 30,
                Margin = new Thickness(5),
                IsReadOnly = true // Make the TextBox read-only to prevent editing
            };

            dynamicGrid.Children.Add(textBox);

            Grid.SetRow(textBox, x);
            Grid.SetColumn(textBox, y);
        }

        private void PreDefine()
        {
            decimal[] myObjectiveFunction = new[] { 1M, 1 };
            decimal[,] myConstraints = new [,]
            {
                {2M, 1},
                {1, 2},
            };
            decimal[] myResults = new[] { 5M, 5 };
            string[] myInequalities = new[] { "<=", "<="};
            
            MethodBox.SelectedValue = "CuttingPlane";
            NumberOfConstraints.SelectedValue = "2";
            NumberOfVariables.SelectedValue = "2";

            for (int i = 0; i < numberOfVariables; i++)
            {
                ((TextBox)GetGridElement(ObjectiveFunctionGrid, 0, i)).Text = myObjectiveFunction[i].ToString();
            }

            for (int i = 0; i < numberOfConstraints; i++)
            {
                for (int j = 0; j < numberOfVariables; j++)
                {
                    ((TextBox)GetGridElement(ConstraintsGrid, i, j)).Text = myConstraints[i, j].ToString();
                }
                ((ComboBox)GetGridElement(ConstraintsGrid, i, numberOfVariables)).SelectedValue = myInequalities[i];
                ((TextBox)GetGridElement(ConstraintsGrid, i, numberOfVariables + 1)).Text = myResults[i].ToString();
            }
        }
    }

}
