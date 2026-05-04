public struct ResolveResult
{
    public bool needsTarget;
    public int nextModuleIndex; // der Index des Moduls, das JETZT Target braucht (0 oder 1)

    public static ResolveResult Done() => new ResolveResult { needsTarget = false, nextModuleIndex = 2 };
    public static ResolveResult Need(int moduleIndex) => new ResolveResult { needsTarget = true, nextModuleIndex = moduleIndex };
}
