using System.Reflection;
using System.Runtime.CompilerServices;

namespace FishPieClient.Utils;

public static class ListExtensions
{

    public static Memory<T> AsMemory<T>(this List<T> list, int start = 0, int length = -1)
    {
        var items =
            (T[])list.GetType().GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(list)!;
        return new Memory<T>(items, start, length < -1 ? items.Length : length);
    }
    
}