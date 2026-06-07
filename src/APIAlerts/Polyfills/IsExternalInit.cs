#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices;

/// <summary>
/// Polyfill for the `init` accessor on netstandard2.0 builds. The full
/// implementation ships with .NET 5+; this empty internal stub satisfies the
/// compiler so init-only properties work on older targets (including Unity).
/// </summary>
internal static class IsExternalInit { }
#endif
