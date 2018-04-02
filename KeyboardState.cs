using System.Collections.Generic;
using OpenTK.Input;

//This class is needed because keyboard is broken with dotnetcore
internal static class KeyboardState
{
    public static HashSet<Key> Keys = new HashSet<Key>();

    public static bool IsKeyDown(Key key) => Keys.Contains(key);
}