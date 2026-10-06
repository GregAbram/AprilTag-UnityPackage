using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AprilTag.Interop {

public sealed class Family : SafeHandleZeroOrMinusOneIsInvalid
{
    #region SafeHandle implementation

    // Which native family this handle holds, so the matching destroy function is
    // called (each family has its own; calling both double-frees the handle).
    enum Kind { TagStandard41h12, Tag36h11 }

    Kind _kind;

    Family() : base(true) {}

    protected override bool ReleaseHandle()
    {
        switch (_kind)
        {
            case Kind.Tag36h11:
#if UNITY_IOS && !UNITY_EDITOR
                // Unreachable: CreateTag36h11 throws on iOS.
#else
                _DestroyTag36h11(handle);
#endif
                break;
            default:
                _DestroyTagStandard41h12(handle);
                break;
        }
        return true;
    }

    #endregion

    #region Public methods

    public static Family CreateTagStandard41h12()
    {
        var family = _CreateTagStandard41h12();
        family._kind = Kind.TagStandard41h12;
        return family;
    }

    // Only the Android library currently includes tag36h11; elsewhere this
    // throws (EntryPointNotFoundException, or PlatformNotSupportedException on
    // iOS, where the static library must not reference missing symbols).
    public static Family CreateTag36h11()
    {
#if UNITY_IOS && !UNITY_EDITOR
        throw new PlatformNotSupportedException("tag36h11 is not included in the iOS libAprilTag.a");
#else
        var family = _CreateTag36h11();
        family._kind = Kind.Tag36h11;
        return family;
#endif
    }

    #endregion

    #region Unmanaged interface

    [DllImport(Config.DllName, EntryPoint = "tagStandard41h12_create")]
    private static extern Family _CreateTagStandard41h12();

    [DllImport(Config.DllName, EntryPoint = "tagStandard41h12_destroy")]
    private static extern void _DestroyTagStandard41h12(IntPtr ptr);

#if !(UNITY_IOS && !UNITY_EDITOR)
    [DllImport(Config.DllName, EntryPoint = "tag36h11_create")]
    private static extern Family _CreateTag36h11();

    [DllImport(Config.DllName, EntryPoint = "tag36h11_destroy")]
    private static extern void _DestroyTag36h11(IntPtr ptr);
#endif

    #endregion
}

} // namespace AprilTag.Interop
