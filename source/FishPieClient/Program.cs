// See https://aka.ms/new-console-template for more information

using FishPieClient.Graphics;
using FishPieShooter.Utils;

Log.Init();

var window = new Window(new Window.Settings("Fish Pie Shooter", 1280, 720));
while (true)
{
    window.OnUpdate();
}
window.Dispose();