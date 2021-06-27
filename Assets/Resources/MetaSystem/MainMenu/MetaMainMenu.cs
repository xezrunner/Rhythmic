using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Logger;

public class MetaMainMenu : MonoBehaviour
{
    MetaSystem MetaSystem;

    public GameObject MetaButton_Prefab;
    public Transform SongList_ScrollView_Container;
    public GameObject SongList;

    public void SetVisibility(bool value) => gameObject.SetActive(value);
    public void SetVisibility_Full(bool value) { gameObject.SetActive(value); MetaSystem.META_SetVisibility(value); }

    void Start()
    {
        MetaSystem = MetaSystem.Instance;
        SongList.SetActive(false);
    }

    public void DEBUG_StartGame()
    {
        GameState.LoadScene("RH_Main");
        MetaSystem.META_SetVisibility(false);

        SetVisibility(false);
    }

    /// Song list: 
    public void BUTTON_SongList() => ToggleSongList();
    void ToggleSongList()
    {
        SongList.SetActive(!SongList.activeSelf);
        if (SongList.activeSelf) InitSongList();
    }

    public void BUTTON_SongClick(string song_name) => StartSong(song_name);

    public List<MetaButton> SongList_Buttons = new List<MetaButton>();
    void InitSongList()
    {
        if (SongList_Buttons.Count != 0) { LogW("Already initialized!".TM(this)); return; }
        SongList_Buttons = new List<MetaButton>();

        string path = AmplitudeGame.song_ogg_path;
        string[] dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

        // TODO!: Improve! Right now, we're loading Amplitude songs only!
        // AMP / RH should probably have their own tabs or options!
        for (int i = 0; i < dirs.Length; ++i)
        {
            string[] tokens = dirs[i].Split(new string[] { "\\" }, System.StringSplitOptions.RemoveEmptyEntries);
            dirs[i] = tokens[tokens.Length - 1];
        }

        foreach (string s in dirs)
        {
            GameObject obj = Instantiate(MetaButton_Prefab, SongList_ScrollView_Container);
            obj.name = s;

            MetaButton button = obj.GetComponent<MetaButton>(); // TODO!: Slow!!!
            button.onClick.AddListener(delegate { BUTTON_SongClick(obj.name); });

            SongList_Buttons.Add(button);
        }
    }

    void StartSong(string song_name)
    {
        Log("Song requested: '%'".TM(this), song_name);

        GameState game_state = GameState.Instance;
        if (!game_state) game_state = GameState.CreateGameState();
        game_state.current_song_name = song_name;

        GameState.LoadScene("RH_Main");
        SetVisibility_Full(false);
    }
}
