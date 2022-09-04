using XZShared;

namespace DebugMenu_Pages {
    [DebugMenuPage]
    public class MainPage : DebugMenu_Page, IDebugMenu_Page {
        public void layout() {
            write_line("Allow Unity logging redirection", 
                new Ref<bool>(() => DebugConsole.CONSOLE_RedirectUnityLogging, (v) => DebugConsole.CONSOLE_RedirectUnityLogging = v));
            write_line("Open debug console...", () => DebugConsole.get_instance()?.open());
            write_line("Close debug menu...", DebugMenu.get_instance().close);
            write_line_separator();
            write_line("Switch to TestPage ...", DebugMenu.cmd_debugmenu_test_page);
        }
    }
}