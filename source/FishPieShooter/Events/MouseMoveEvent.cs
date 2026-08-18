namespace FishPieShooter.Events;

public class MouseMoveEvent(float deltaX, float deltaY)
    : Event(EventType.MouseMove)
{

    public float DeltaX => deltaX;
    
    public float DeltaY => deltaY;

}