public static class ArrayExtensions {
    public static int[] Fill(this int[] array, int value) {
        for (int i = 0; i < array.Length; ++i) {
            array[i] = value;
        }
        return array;
    }
    public static string String(this int[] array, string delimiter = " ") {
        string s = null;
        for (int i = 0; i < array.Length; ++i) {
            s += array[i] + delimiter;
        }
        s = s.Substring(0, s.Length - delimiter.Length);
        return s;
    }
}