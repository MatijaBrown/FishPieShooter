namespace FishPieShooter.Events;

public class Event(EventType type)
{
    
    public EventType Type => type;

    internal bool Handled { get; set; } = false;

}