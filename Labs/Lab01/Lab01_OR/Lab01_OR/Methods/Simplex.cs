using Lab01_OR.Methods;

namespace Lab01_OR;

public class Simplex : Method
{

    public Simplex(decimal[] objectiveFunction, decimal[,] constraints, string[] inequalities, decimal[] results,
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
        _inequalities = inequalities;
        _results = results;
        
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

        _deriv = new decimal[results.Length];
        _cb = new decimal[results.Length];

        this._plan = new Dictionary<int, decimal>();
        this._base = new int[results.Length];
        for (int i = objectiveFunction.Length; i < _delta.Length; i++)
        {
            _plan.Add(i, results[i - objectiveFunction.Length]);
            _base[i - objectiveFunction.Length] = i;
        }
        
    }

    public override decimal[] Solve()
    {
        if (CheckIfSolved())
        {
            _mainWindow.CreateAndAddDynamicGridSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);
            decimal[] result = new decimal[_objectiveFunction.Length + 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _plan.GetValueOrDefault(i);
            }

            for (int i = 0; i < _objectiveFunction.Length; i++)
            {
                result[result.Length - 1] += result[i] * _objectiveFunction[i];
            }

            return result;
        }

        decimal min = 0;
        int minIndexColumn = 0;
        for (int i = 0; i < _delta.Length; i++)
        {
            if (_delta[i] < min)
            {
                min = _delta[i];
                minIndexColumn = i;
            }
        }

        for (int i = 0; i < _deriv.Length; i++)
        {
            if (_constraints[i, minIndexColumn] != 0)
                _deriv[i] = _plan.GetValueOrDefault(_base[i]) / _constraints[i, minIndexColumn];
            else
                _deriv[i] = 0;
        }

        min = decimal.MaxValue;
        int minIndexRow = 0;
        for (int i = 0; i < _deriv.Length; i++)
        {
            if (_deriv[i] < min && _deriv[i] > 0)
            {
                min = _deriv[i];
                minIndexRow = i;
            }
        }

        var table = new Table(_constraints, _cb, _plan, _deriv, _delta, _base);
        _mainWindow.CreateAndAddDynamicGridSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);

        RebuildTable(minIndexRow, minIndexColumn);
        FindDelta();

        return Solve();
    }

    public Table GetTable()
    {
        return new Table(_constraints, _cb, _plan, _deriv, _delta, _base);
    }

    protected override bool CheckIfSolved()
    {
        return !_delta.Any(x => x < 0);
    }
}