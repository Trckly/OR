// File: MainWindow.xaml.cs
using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            double[] objectiveFunction = new double[numberOfVariables];
            for (int i = 0; i < numberOfVariables; i++)
            {
                objectiveFunction[i] = Convert.ToDouble(((TextBox)ObjectiveFunctionGrid.Children[i]).Text);
            }

            double[,] constraints = new double[numberOfConstraints, numberOfVariables];
            double[] results = new double[numberOfConstraints];
            string[] inequalities = new string[numberOfConstraints];

            for (int i = 0; i < numberOfConstraints; i++)
            {
                for (int j = 0; j < numberOfVariables; j++)
                {
                    constraints[i, j] = Convert.ToDouble(((TextBox)GetGridElement(ConstraintsGrid, i, j)).Text);
                }

                inequalities[i] = ((ComboBox)GetGridElement(ConstraintsGrid, i, numberOfVariables)).Text;
                results[i] =
                    Convert.ToDouble(((TextBox)GetGridElement(ConstraintsGrid, i, numberOfVariables + 1)).Text);
            }

            //Clear all tables
            DynamicGridContainer.Children.Clear();

            // Call Simplex algorithm to solve
            IMethod? method = null;
            var methodString = MethodBox.Text;
            if (methodString == "Simplex")
            {
                method = new Simplex(objectiveFunction, constraints, inequalities, results, this);
            }
            else if (methodString == "DualSimplex")
            {
                method = new DualSimplex(objectiveFunction, constraints, inequalities, results, this);
            }

            double[]? solution = method?.Solve();
            double[] variables = new double[solution.Length - 1];

            for (int i = 0; i < solution.Length - 1; i++)
            {
                if (solution != null) variables[i] = solution[i];
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
        public void CreateAndAddDynamicGridSimplex(double[,] constraints, double[] cb, Dictionary<int, double> plan, double[] deriv, double[] delta, int[] myBase)
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
                CreateCell(dynamicGrid, i, 0, string.Format("x" + myBase[i - 1]));
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
        public void CreateAndAddDynamicGridDualSimplex(double[,] constraints, double[] cb, Dictionary<int, double> plan, double[] deriv, double[] delta, int[] myBase)
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
                CreateCell(dynamicGrid, i, 0, string.Format("x" + myBase[i - 1]));
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
            double[] myObjectiveFunction = new[] { 2.0, 1 };
            double[,] myConstraints = new [,]
            {
                {4.0, 2},
                {5, -1},
                {-1, 5},
                {1, 1}
            };
            double[] myResults = new[] { 1.0, 0, 0, 6 };
            string[] myInequalities = new[] { ">=", "<=", ">=", "<="};
            
            MethodBox.SelectedValue = "DualSimplex";
            NumberOfConstraints.SelectedValue = "4";
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
