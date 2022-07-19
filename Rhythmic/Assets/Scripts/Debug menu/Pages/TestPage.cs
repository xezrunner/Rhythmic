namespace DebugMenu_Pages {
    [DebugMenuPage]
    public class TestPage : DebugMenu_Page, IDebugMenu_Page {
        public void layout() {
            write_line("Switch to MainPage ...", DebugMenu.cmd_debugmenu_main_page);
            write_line_separator();
            write_line($"{nameof(int_a)}", new Ref(() => int_a, (v) => int_a = (int)v));
            write_line($"{nameof(float_b)}", new Ref(() => float_b, (v) => float_b = (float)v));
            write_line($"{nameof(boolean_c)}", new Ref(() => boolean_c, (v) => boolean_c = (bool)v));
            write_line($"{nameof(entry_type_d)}", new Ref(() => entry_type_d, (v) => entry_type_d = (DebugMenuEntryType)v));
        }

        int   int_a = 15;
        float float_b = 20;
        bool  boolean_c = true;
        DebugMenuEntryType entry_type_d = DebugMenuEntryType.Variable;
    }
}