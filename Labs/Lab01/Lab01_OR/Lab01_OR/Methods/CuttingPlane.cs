namespace Lab01_OR.Methods;

public class CuttingPlane : Method
{
    public CuttingPlane(double[] objectiveFunction, double[,] constraints, string[] inequalities, double[] results,
        MainWindow mainWindow)
    {
        _objectiveFunction = objectiveFunction;
        _constraints = constraints;
        _inequalities = inequalities;
        _results = results;
        _mainWindow = mainWindow;
    }

    public override double[] Solve()
    {
        var simplex = new Simplex(_objectiveFunction, _constraints, _inequalities, _results, _mainWindow);
        var result = simplex.Solve();
        var table = simplex.GetTable();
        _plan = new Dictionary<int, double>(table.plan);

        if (CheckIfSolved())
        {
            return result;
        }

        var newPlan = result.Select(x => x - Math.Truncate(x)).ToArray();
        double max = 0;
        int index = -1;
        for (int i = 0; i < newPlan.Length - 1; i++)
        {
            if (newPlan[i] > max)
            {
                max = newPlan[i];
                index = i;
            }
        }
        
        double[,] newConstraints = new double[_constraints.GetLength(0)+1, _constraints.GetLength(1)+1];
        for (int i = 0; i < _constraints.GetLength(0); i++)
        {
            for (int j = 0; j < _constraints.GetLength(1); j++)
            {
                newConstraints[i, j] = _constraints[i, j];
            }

            newConstraints[i, _constraints.GetLength(1)] = 0;
        }

        for (int i = 0; i < newConstraints.GetLength(1); i++)
        {
            double constraint = _constraints[index, i];
            if (constraint >= 0.0000001)
            {
                newConstraints[newConstraints.GetLength(0) - 1, i] = constraint;
            }
            else
            {
                newConstraints[newConstraints.GetLength(0) - 1, i] = result[index] / (1 - result[index]) * Math.Abs(constraint);
            }
        }
        var newRes = new double[_results.GetLength(0) + 1];
        for (int i = 0; i < _results.GetLength(1); i++)
        {
            newRes[i] = _results[i];
        }
        newRes[_results.GetLength(0)] = newPlan[index];

        while (true)
        {
            for
        }
        
    }

    protected override bool CheckIfSolved()
    {
        return !_plan.Values.Any(x => Math.Abs(x) > double.Epsilon * 100);
    }
}