namespace DebugMenu_Pages {
    [DebugMenuPage]
    public class TestPage : DebugMenu_Page, IDebugMenu_Page {
        public void draw_page() {
            write_line("Switch to MainPage ...", DebugMenu.cmd_debugmenu_main_page);
            write_line_separator();
            write_line("a", new Ref(() => a, (v) => a = (int)v));
            write_line("b", new Ref(() => b, (v) => b = (float)v));
            write_line("c", new Ref(() => c, (v) => c = (bool)v));
            write_line("d", new Ref(() => d, (v) => d = (DebugMenuEntryType)v));
        }

        int   a = 15;
        float b = 20;
        bool  c = true;
        DebugMenuEntryType d = DebugMenuEntryType.Variable;
    }
}