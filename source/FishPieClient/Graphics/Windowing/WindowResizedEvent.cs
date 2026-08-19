using FishPieShooter.Events;

namespace FishPieClient.Graphics.Windowing;

public class WindowResizedEvent(uint width, uint height)
    : Event(EventType.WindowResized)
{
    
    public uint Width => width;
    
    public uint Height => height;
    
}