namespace DebugMenu_Pages {
    [DebugMenuPage]
    public class TestPage : DebugMenu_Page, IDebugMenu_Page {
        public void layout() {
            write_line("Switch to MainPage ...", DebugMenu.cmd_debugmenu_main_page);
            write_line_separator();
            write_line($"{nameof(integer_value)}", new Ref(() => integer_value, (v) => integer_value = (int)v));
            write_line($"{nameof(float_value)}", new Ref(() => float_value, (v) => float_value = (float)v));
            write_line($"{nameof(boolean_value)}", new Ref(() => boolean_value, (v) => boolean_value = (bool)v));
            write_line($"{nameof(enum_value)}", new Ref(() => enum_value, (v) => enum_value = (DebugMenuEntryType)v));
        }

        int   integer_value = 15;
        float float_value   = 20;
        bool  boolean_value = true;
        DebugMenuEntryType enum_value = DebugMenuEntryType.Variable;
    }
}