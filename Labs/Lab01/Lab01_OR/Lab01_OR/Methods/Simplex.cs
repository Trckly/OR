using Lab01_OR.Methods;

namespace Lab01_OR;

public class Simplex : Method
{

    public Simplex(double[] objectiveFunction, double[,] constraints, string[] inequalities, double[] results,
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

        _deriv = new double[results.Length];
        _cb = new double[results.Length];

        this._plan = new Dictionary<int, double>();
        this._base = new int[results.Length];
        for (int i = objectiveFunction.Length; i < _delta.Length; i++)
        {
            _plan.Add(i, results[i - objectiveFunction.Length]);
            _base[i - objectiveFunction.Length] = i;
        }
        
    }

    public override double[] Solve()
    {
        if (CheckIfSolved())
        {
            _mainWindow.CreateAndAddDynamicGridSimplex(_constraints, _cb, _plan, _deriv, _delta, _base);
            double[] result = new double[_objectiveFunction.Length + 1];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = _plan.GetValueOrDefault(i);
            }

            for (int i = 0; i < _base.Length; i++)
            {
                result[result.Length - 1] += result[i] * _objectiveFunction[i];
            }

            return result;
        }

        double min = 0;
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
            _deriv[i] = _plan.GetValueOrDefault(_base[i]) / _constraints[i, minIndexColumn];
        }

        min = Double.MaxValue;
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