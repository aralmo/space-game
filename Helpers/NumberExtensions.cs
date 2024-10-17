using System.Runtime.CompilerServices;

public static class NumberExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt(this float v) => (int) Math.Round(v);

}