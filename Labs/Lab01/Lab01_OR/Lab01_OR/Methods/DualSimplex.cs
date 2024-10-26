using System.Xml;
using Lab01_OR.Methods;

namespace Lab01_OR;

public class DualSimplex : Method
{
    public DualSimplex(decimal[] objectiveFunction, decimal[] results, MainWindow mainWindow,
        Table table)
    {
        _objectiveFunction = objectiveFunction;
        _results = results;
        _mainWindow = mainWindow;
        _constraints = table.constraints;
        _cb = table.cb;
        _plan = table.plan;
        _deriv = table.deriv;
        _delta = table.delta;
        _base = table.myBase;
    }

    public DualSimplex(decimal[] objectiveFunction, decimal[,] constraints, string[] inequalities, decimal[] results,
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
    public void Initialize(decimal[] objectiveFunction, decimal[,] constraints, string[] inequalities, decimal[] results)
    {
        
        this._constraints = new decimal[constraints.GetLength(0), constraints.GetLength(0) + objectiveFunction.Length];
        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            for (int j = 0; j < constraints.GetLength(1); j++)
            {
                _constraints[i, j] = constraints[i, j];
            }
        }

        for (int i = 0; i < constraints.GetLength(0); i++)
        {
            _constraints[i, constraints.GetLength(1) + i] = 1;
        }

        this._delta = new decimal[results.Length + objectiveFunction.Length];
        for (int i = 0; i < objectiveFunction.Length; i++)
        {
            _delta[i] = -objectiveFunction[i];
        }

        _cb = new decimal[results.Length];
        _deriv = new decimal[_delta.Length];

        this._inequalities = inequalities;
        this._plan = new Dictionary<int, decimal>();
        this._base = new int[results.Length];
        for (int i = objectiveFunction.Length; i < _delta.Length; i++)
        {
            _plan.Add(i, results[i - objectiveFunction.Length]);
            _base[i - objectiveFunction.Length] = i;
        }
        
    }

    // Check whether to use the primal or dual simplex method based on inequalities
    public override decimal[] Solve()
    {
        if (CheckIfSolved())
        {
            _mainWindow.CreateAndAddDynamicGridDualSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);
            
            decimal[] result = new decimal[_delta.Length + 1];
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

        decimal min = 0;
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
            if (_constraints[minIndexRow, i] == 0)
            {
                _deriv[i] = 0;
            }
            else
            {
                _deriv[i] = -_delta[i] / _constraints[minIndexRow, i];
            }
        }

        min = decimal.MaxValue;
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
        decimal[,] transposedConstraints = new decimal[_constraints.GetLength(1), _constraints.GetLength(0)];
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

    public Table GetTable()
    {
        return new Table(_constraints, _cb, _plan, _deriv, _delta, _base);
    }

    protected override bool CheckIfSolved()
    {
        return !_plan.Values.Any(x => x < 0);
    }
}