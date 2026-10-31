#if NETCOREAPP3_1
namespace System.Runtime.CompilerServices;

/// <summary>Polyfill that lets records and init-only setters compile on netcoreapp3.1</summary>
internal static class IsExternalInit
{
}
#endif
