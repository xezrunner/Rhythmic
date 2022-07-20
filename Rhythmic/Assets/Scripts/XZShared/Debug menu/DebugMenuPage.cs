using System;

using static Logging;

public interface IDebugMenu_Page
{
    public void layout();
}

public enum DebugMenuPageType { Static, Dynamic }
[AttributeUsage(AttributeTargets.Class)]
public class DebugMenuPageAttribute : Attribute {
    public DebugMenuPageAttribute(DebugMenuPageType page_type = DebugMenuPageType.Static, float page_refresh_ms = 120f) {
        this.page_type = page_type;
        this.page_refresh_ms = page_refresh_ms;
    }
    public DebugMenuPageType page_type;
    public float page_refresh_ms;
}

public class DebugMenu_Page {
    public DebugMenu_Page() {
        Type type = GetType();
        attrib = (DebugMenuPageAttribute)Attribute.GetCustomAttribute(type, typeof(DebugMenuPageAttribute));
        if (attrib == null) {
            log_warn("The debug menu page '%' does not bear a DebugMenuPage attribute! Adding default.".interp(type.Name));
            attrib = new();
        }

        debugmenu_instance = DebugMenu.get_instance();
    }
    public DebugMenuPageAttribute attrib;
    DebugMenu debugmenu_instance;

    // Lines API:
    public DebugMenuEntry queue_new_line(DebugMenuEntry entry)     => debugmenu_instance.queue_entry(entry);
    
    public DebugMenuEntry write_line_separator() => debugmenu_instance.queue_entry(new(" ") { entry_type = DebugMenuEntryType.Separator });
    public DebugMenuEntry write_line(string text)                => debugmenu_instance.write_line(text);
    public DebugMenuEntry write_line(string text, Action action) => debugmenu_instance.write_line(text, action);
    public DebugMenuEntry write_line(string text, Ref var_ref)   => debugmenu_instance.write_line(text, var_ref);
}