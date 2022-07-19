namespace DebugMenu_Pages {
    [DebugMenuPage]
    public class MainPage : DebugMenu_Page, IDebugMenu_Page {
        public void layout() {
            write_line("Hello world from the MainPage!", () => Logging.log("You have clicked the first entry!"));
            write_line("Hello world from the MainPage! 2");
            write_line("Hello world from the MainPage! 3");
            write_line_separator();
            write_line("Switch to TestPage ...", DebugMenu.cmd_debugmenu_test_page);
        }
    }
}