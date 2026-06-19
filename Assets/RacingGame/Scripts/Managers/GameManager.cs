using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;
        DOTween.Init();
        DOTween.SetTweensCapacity(500, 100);
    }

    public void StartNewCareer(Difficulty difficulty)
    {
        State = WorldGenerator.Generate(difficulty);
        SaveManager.Instance.SaveGame(State);
    }

    public bool LoadCareer()
    {
        GameState loaded = SaveManager.Instance.LoadGame();
        if (loaded != null)
        {
            State = loaded;
            return true;
        }
        return false;
    }
}
