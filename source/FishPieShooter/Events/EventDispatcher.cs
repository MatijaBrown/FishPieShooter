namespace FishPieShooter.Events;

public class EventDispatcher(Event @event)
{
    public void Dispatch(EventType type, EventHandler handler)
    {
        if (@event.Handled) return;
        if (@event.Type != type) return;
        
        @event.Handled = handler(@event);
    }
    
}