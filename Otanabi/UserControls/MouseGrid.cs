using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
 
namespace Otanabi.UserControls;

public sealed class MouseGrid : Grid
{
    public InputCursor InputCursor
    {
        get => base.ProtectedCursor;
        set => base.ProtectedCursor = value;
    }
}
