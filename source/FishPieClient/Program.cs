// See https://aka.ms/new-console-template for more information

namespace FishPieClient;

using FishPieClient.Graphics.Windowing;
using FishPieShooter.Events;
using FishPieShooter.Utils;

public static class Program
{

    private static Window? _window;
    private static bool _running = true;
    
    private static void OnEvent(Event @event)
    {
        var dispatcher = new EventDispatcher(@event);
        dispatcher.Dispatch(EventType.WindowClose, e =>
        {
            _running = false;
            return false;
        });
    }

    
    private static void Main(string[] args)
    {
        Log.Init();
        
        _window = new Window(new Window.Settings("Fish Pie Shooter", 1280, 720), OnEvent);
        while (_running)
        {
            _window.OnUpdate();
        }
        _window.Shutdown();
        _window.Dispose();
    }
    
}