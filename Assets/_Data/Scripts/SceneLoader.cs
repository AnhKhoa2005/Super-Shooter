using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Fusion;
using Sirenix.OdinInspector;
using System;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private int currentScene = 0;
    public int CurrentScene => currentScene;

    public void SetCurrentScene(int sceneIndex)
    {
        currentScene = sceneIndex;
    }
    public async UniTask LoadInitRunner()
    {
        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
        await NetworkBootstrap.Instance.JoinLobby();
        UIManager.Instance.ChangeMenu(MenuType.MainMenu);
    }

    public async UniTask LoadLobby(NetworkRunner runner)
    {
        currentScene = 1; //Lobby
        await runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Additive);
        await UniTask.WaitUntil(() => SceneManager.GetSceneByBuildIndex(1).isLoaded);
        UIManager.Instance.ChangeMenu(MenuType.HUDMenu);
    }

    public async UniTask LoadLevel(NetworkRunner runner, int mapIndex)
    {
        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
        //Unload scene hiện tại
        await runner.UnloadScene(SceneRef.FromIndex(currentScene));
        currentScene = mapIndex;

        await runner.LoadScene(SceneRef.FromIndex(mapIndex), LoadSceneMode.Additive);
        await UniTask.WaitUntil(() => SceneManager.GetSceneByBuildIndex(mapIndex).isLoaded);
        UIManager.Instance.ChangeMenu(MenuType.HUDMenu);
    }

    public async UniTask LoadMainMenuForClient(int sceneToUnload)
    {
        //Unload scene hiện tại
        await SceneManager.UnloadSceneAsync(sceneToUnload);
        await Resources.UnloadUnusedAssets();
        currentScene = 0; //Main Menu

        //Đợi scene main menu được load xong trước khi chuyển menu
        await UniTask.WaitUntil(() => SceneManager.GetSceneByBuildIndex(0).isLoaded);
    }

    public async UniTask LoadMainMenuForHost(int sceneToUnload)
    {
        //Unload scene hiện tại
        if (sceneToUnload != 0)
        {
            await NetworkBootstrap.Instance.Runner.UnloadScene(SceneRef.FromIndex(sceneToUnload));

            //Đợi scene cần unload biến mất hoàn toàn trong SceneManager local.
            await UniTask.WaitUntil(() => !SceneManager.GetSceneByBuildIndex(sceneToUnload).isLoaded);
        }

        currentScene = 0; //Main Menu

        //Đợi scene main menu được load xong trước khi chuyển menu
        await UniTask.WaitUntil(() => SceneManager.GetSceneByBuildIndex(0).isLoaded);
    }
}
