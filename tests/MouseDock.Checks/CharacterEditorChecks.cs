using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MouseDock.Controls;
using MouseDock.Models;

internal static class CharacterEditorChecks
{
    internal static void Draft()
    {
        using var fixture = new AppearanceFixture();
        var type = typeof(AppSettings).Assembly.GetType("MouseDock.Views.CharacterEditorWindow");
        Require(type != null, "Unified character editor is missing.");
        var settings = new AppSettings { IconStyle = "ring", ReferenceCharacter = "rei", PhotoZoom = 1.4, TaskbarOffset = 47 };
        var editor = (Window)Activator.CreateInstance(type!, [settings])!;
        var draft = (AppSettings)type!.GetProperty("Draft")!.GetValue(editor)!;
        draft.ReferenceCharacter = "asuka";
        draft.PhotoZoom = 2.2;
        editor.Close();
        Require(settings.ReferenceCharacter == "rei" && settings.PhotoZoom == 1.4, "Canceling character editing changes saved settings.");
        type.GetMethod("ApplyTo")!.Invoke(editor, [settings]);
        Require(settings.ReferenceCharacter == "asuka" && settings.PhotoZoom == 2.2 && settings.TaskbarOffset == 47,
            "Applying character editing loses unrelated preferences or ignores the draft.");
    }

    internal static void PhotoDetails()
    {
        var device = new DeviceInfo { Id = "key", Name = "Keyboard", Kind = DeviceKind.Keyboard, Level = 76 };
        var icon = new CharacterBatteryIcon { Character = "photo", Width = 240, Height = 260, Devices = new[] { device } };
        var original = Snapshot(icon);
        device.Name = "Renamed R98";
        var renamed = Snapshot(icon);
        Require(!original.SequenceEqual(renamed), "Photo overlay does not display the device name.");
        device.Charging = true;
        Require(!renamed.SequenceEqual(Snapshot(icon)), "Photo overlay does not display live device status.");
    }

    private static byte[] Snapshot(FrameworkElement element)
    {
        element.Measure(new Size(element.Width, element.Height));
        element.Arrange(new Rect(0, 0, element.Width, element.Height));
        element.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)element.Width, (int)element.Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);
        var pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
        return pixels;
    }
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
