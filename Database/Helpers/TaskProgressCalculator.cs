namespace Database.Helpers;

public static class TaskProgressCalculator
{
    public static bool TryAverage(IEnumerable<int> progressPercents, out int result)
    {
        var sum = 0L;
        var count = 0;
        foreach (var percent in progressPercents)
        {
            sum += percent;
            count++;
        }

        if (count == 0)
        {
            result = 0;
            return false;
        }

        result = Math.Clamp(
            (int)Math.Round(sum / (double)count, MidpointRounding.AwayFromZero),
            0,
            100);
        return true;
    }
}
