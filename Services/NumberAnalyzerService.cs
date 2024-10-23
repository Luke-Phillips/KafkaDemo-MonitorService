namespace MonitorService.Services;

public class NumberAnalyzerService
{
    public bool CheckIsSuspicious(int number)
    {
        // intentionally inefficient way of checking if a number is "suspicious"
        for (int i = 2; i < number; i++)
        {
            if (number % i == 0)
            {
                return true;
            }
        }
        return false;
    }
}