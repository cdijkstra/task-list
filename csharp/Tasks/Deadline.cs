using System;

namespace Tasks;

public class Deadline
{
    public Deadline(string rawDeadline)
    {
        var receivedDeadline = DateOnly.ParseExact(rawDeadline, "dd-MM-yyyy");
        if (receivedDeadline < DateOnly.FromDateTime(DateTime.Now))
        {
            throw new Exception("Deadline cannot be added; is in the past");
        }
	
        deadline = receivedDeadline;
    }
    private DateOnly deadline;

    public bool IsToday()
    {
        return DateOnly.FromDateTime(DateTime.Now) == deadline;
    }
}