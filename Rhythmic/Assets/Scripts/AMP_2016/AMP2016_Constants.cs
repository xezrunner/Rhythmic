namespace AMP_2016 {
    static class AMP2016_Constants {
        public static int get_lane_index_from_note_number(int note_number) {
            // @Performance
            for (int i = 0; i < note_numbers_for_difficulties.Length; ++i) {
                for (int x = 0; x < 3; x++) {
                    int n = note_numbers_for_difficulties[i,x];
                    if (n == note_number) return x;
                }
            }
            return -1;
        }

        public static readonly int[,] note_numbers_for_difficulties = {
            { 96,  98,  100 }, // Beginner
            { 102, 104, 106 }, // Intermediate
            { 108, 110, 112 }, // Advanced
            { 114, 116, 118 }, // Expert
        };
    }
}
