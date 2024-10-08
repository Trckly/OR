using Lab01_OR.Methods;

namespace Lab01_OR;

public class DualSimplex : Method
{

    public DualSimplex(double[] objectiveFunction, double[,] constraints, string[] inequalities, double[] results,
        MainWindow mainWindow)
    {
        for (int i = 0; i < inequalities.Length; i++)
        {
            if (inequalities[i] == ">=")
            {
                for (int j = 0; j < constraints.GetLength(0); j++)
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
        
        this._constraints = new double[constraints.GetLength(0), constraints.GetLength(0) + objectiveFunction.Length];
        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            for (int j = 0; j < constraints.GetLength(0); j++)
            {
                _constraints[i, j] = constraints[i, j];
            }
        }

        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            _constraints[i, constraints.GetLength(0) + i] = 1;
        }

        this._delta = new double[results.Length + objectiveFunction.Length];
        for (int i = 0; i < objectiveFunction.Length; i++)
        {
            _delta[i] = -objectiveFunction[i];
        }

        _cb = new double[results.Length];
        _deriv = new double[_delta.Length];

        this._inequalities = inequalities;
        this._plan = new Dictionary<int, double>();
        this._base = new int[results.Length];
        for (int i = objectiveFunction.Length; i < _delta.Length; i++)
        {
            _plan.Add(i, results[i - objectiveFunction.Length]);
            _base[i - objectiveFunction.Length] = i;
        }
        
    }

    // Check whether to use the primal or dual simplex method based on inequalities
    public override double[] Solve()
    {
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

        double min = 0;
        int minIndexRow = 0;
        for (int i = 0; i < _base.Length; i++)
        {
            if (_plan.GetValueOrDefault(_base[i]) < min)
            {
                min = _plan.GetValueOrDefault(_base[i]);
                minIndexRow = i;
            }
        }

        for (int i = 0; i < _deriv.Length; i++)
        {
            _deriv[i] = -_delta[i] / _constraints[minIndexRow, i];
        }

        min = Double.MaxValue;
        int minIndexColumn = 0;
        for (int i = 0; i < _deriv.Length; i++)
        {
            if (_deriv[i] < min && _deriv[i] > 0)
            {
                min = _deriv[i];
                minIndexColumn = i;
            }
        }

        _mainWindow.CreateAndAddDynamicGridDualSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);

        RebuildTable(minIndexRow, minIndexColumn);
        FindDelta();

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
                transposedConstraints[j, i] = -_constraints[i, j];
            }
        }

        _constraints = transposedConstraints;

        // Swap the objective function with the results array
        (_objectiveFunction, _results) = (_results.Select(x => -x).ToArray(), _objectiveFunction.Select(x => -x).ToArray());

    }

    protected override bool CheckIfSolved()
    {
        return !_plan.Values.Any(x => x < 0);
    }
}