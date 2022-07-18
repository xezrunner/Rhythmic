namespace DebugMenu_Pages {
    [DebugMenuPage]
    public class TestPage : DebugMenu_Page, IDebugMenu_Page {
        public void draw_page() {
            write_line("Switch to MainPage ...", DebugMenu.cmd_debugmenu_main_page);
            for (int i = 0; i < 19; ++i) {
                write_line("Hello world from the TestPage now instead! %".interp(i + 1));
            }
        }
    }
}