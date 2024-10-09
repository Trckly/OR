namespace Lab01_OR;

public class DualSimplex : IMethod
{
    private double[] _objectiveFunction;
    private double[,] _constraints;
    private string[] _inequalities;
    private double[] _delta;
    private double[] _cb;
    private double[] _deriv;
    private Dictionary<int, double> _plan;
    private double[] _results;
    private int[] _base;
    MainWindow _mainWindow;

    public DualSimplex(double[] objectiveFunction, double[,] constraints, string[] inequalities, double[] results,
        MainWindow mainWindow)
    {
        for (int i = 0; i < inequalities.Length; i++)
        {
            if (inequalities[i] == ">=")
            {
                for (int j = 0; j < constraints.GetLength(1); j++)
                {
                    constraints[i, j] = -constraints[i, j];
                }
                
                results[i] = -results[i];
            }
        }
        
        _mainWindow = mainWindow;
        _objectiveFunction = objectiveFunction;
        _constraints = constraints;
        _inequalities = inequalities;
        _results = results;

        ApplyDualSimplexTransform();
        Initialize(_objectiveFunction, _constraints, _inequalities, _results);
    }
    public void Initialize(double[] objectiveFunction, double[,] constraints, string[] inequalities, double[] results)
    {
        
        this._constraints = new double[constraints.GetLength(0), 2 * constraints.GetLength(0) + objectiveFunction.Length];
        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            for (int j = 0; j < constraints.GetLength(1); j++)
            {
                _constraints[i, j] = constraints[i, j];
            }
        }

        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            _constraints[i, constraints.GetLength(1) + i] = -1;
            _constraints[i, constraints.GetLength(1) + i + constraints.GetLength(0)] = 1;
        }

        this._delta = new double[2 * results.Length + objectiveFunction.Length];
        for (int i = 0; i < objectiveFunction.Length; i++)
        {
            _delta[i] = -objectiveFunction[i];
        }

        _cb = new double[results.Length];
        _deriv = new double[_delta.Length];

        this._inequalities = inequalities;
        this._plan = new Dictionary<int, double>();
        this._base = new int[results.Length];
        for (int i = _constraints.GetLength(1) - results.Length; i < _constraints.GetLength(1); i++)
        {
            _plan.Add(i, results[i - (_constraints.GetLength(1) - results.Length)]);
            _base[i - (_constraints.GetLength(1) - results.Length)] = i;
        }
        
    }

    // Check whether to use the primal or dual simplex method based on inequalities
    public double[] Solve()
    {
        FindDelta();
        if (CheckIfSolved())
        {
            _mainWindow.CreateAndAddDynamicGridDualSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);
            
            double[] result = new double[_delta.Length + 1];
            for (int i = 0; i < _delta.Length; i++)
            {
                result[i] = _plan.GetValueOrDefault(i);
            }

            for (int i = 0; i < _base.Length; i++)
            {
                result[result.Length - 1] += _plan.GetValueOrDefault(_base[i]) * _cb[i];
            }

            return result;
        }

        double max = 0;
        int maxIndexRow = 0;
        for (int i = 0; i < _base.Length; i++)
        {
            if (_plan.GetValueOrDefault(_base[i]) > max)
            {
                max = _plan.GetValueOrDefault(_base[i]);
                maxIndexRow = i;
            }
        }

        for (int i = 0; i < _deriv.Length; i++)
        {
            _deriv[i] = _plan.GetValueOrDefault(_base[i]) / _constraints[i, maxIndexColumn];
        }

        max = 0;
        int maxIndexColumn = 0;
        for (int i = 0; i < _deriv.Length; i++)
        {
            if (_deriv[i] > max)
            {
                max = _deriv[i];
                maxIndexColumn = i;
            }
        }

        _mainWindow.CreateAndAddDynamicGridDualSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);

        RebuildTable(maxIndexRow, maxIndexColumn);

        return Solve();
    }
    
    // Transform the problem to use dual Simplex
    private void ApplyDualSimplexTransform()
    {
        // Transpose the constraints array
        double[,] transposedConstraints = new double[_constraints.GetLength(1), _constraints.GetLength(0)];
        for (int i = 0; i < _constraints.GetLength(0); i++)
        {
            for (int j = 0; j < _constraints.GetLength(1); j++)
            {
                transposedConstraints[j, i] = _constraints[i, j];
            }
        }

        _constraints = transposedConstraints;

        // Swap the objective function with the results array
        (_objectiveFunction, _results) = (_results, _objectiveFunction);

    }


    private void FindDelta()
    {
        double[] newDelta = new double[_delta.Length];

        for (int i = 0; i < newDelta.Length; i++)
        {
            double localDelta = 0;
            for (int j = 0; j < _base.Length; j++)
            {
                localDelta += _cb[j] * _constraints[j, i];
            }

            if (i >= _objectiveFunction.Length)
            {
                newDelta[i] = localDelta;
            }
            else
            {
                newDelta[i] = localDelta - _objectiveFunction[i];
            }
        }

        _delta = newDelta;
    }

    private void RebuildTable(int maxIndexRow, int maxIndexColumn)
    {
        double rate = _constraints[maxIndexRow, maxIndexColumn];

        if (maxIndexColumn < _objectiveFunction.Length)
        {
            _cb[maxIndexRow] = _objectiveFunction[maxIndexColumn];
        }

        Dictionary<int, double> newPlan = _plan.ToDictionary();
        double[,] newConstraints = new double[_constraints.GetLength(0), _constraints.GetLength(1)];

        newPlan.Add(maxIndexColumn, _plan[_base[maxIndexRow]] / rate);
        newPlan.Remove(_base[maxIndexRow]);

        for (int i = 0; i < _delta.Length; i++)
        {
            newConstraints[maxIndexRow, i] = _constraints[maxIndexRow, i] / rate;
        }

        for (int i = 0; i < _base.Length; i++)
        {
            if (i != maxIndexRow)
            {
                newPlan[_base[i]] =
                    _plan[_base[i]] - _constraints[i, maxIndexColumn] * _plan[_base[maxIndexRow]] / rate;

                for (int j = 0; j < _delta.Length; j++)
                {
                    if (j != maxIndexColumn)
                    {
                        newConstraints[i, j] = _constraints[i, j] -
                                               _constraints[maxIndexRow, j] * _constraints[i, maxIndexColumn] / rate;
                    }
                    else
                    {
                        newConstraints[i, j] = 0;
                    }
                }
            }
        }

        _constraints = newConstraints;
        _plan = newPlan;
        _base[maxIndexRow] = maxIndexColumn;

    }

    private bool CheckIfSolved()
    {
        return !_deriv.Any(x => x > 0.000001);
    }
}