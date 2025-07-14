using UnityEngine;

public class BaseController : SceneFlowController
{
    [SerializeField] private GameObject namePromptCanvas;

    public override void Start()
    {
        base.Start();

        var state = GameManager.Instance.CurrentState;

        // if GameState is null, it is because we need to choose a name first and then initialize it for the first time.
        if (state == null)
        {
            // new game - let player choose a name.
            ShowNamePrompt();
            return;
        }

        ShowSceneEntryDialogue(state);
    }

    private void ShowNamePrompt()
    {
        if (namePromptCanvas)
        {
            // disable UI and the background image and show Name Prompt popup window.
            backgroundImage.SetActive(false);
            UIManager.Instance.SetInteractable(false);
            namePromptCanvas.SetActive(true);

            // pass a reference to the callback to NamePromptController
            var controller = namePromptCanvas.GetComponent<NamePromptController>();
            controller.OnNameChosenCallback = OnNameChosenContinue;
        }
    }

    // gets called after name is confirmed.
    private void OnNameChosenContinue(string playerName)
    {
        GameManager.Instance.CreateNewGame(playerName);
        GameManager.Instance.SaveGameState();

        namePromptCanvas.SetActive(false);
        UIManager.Instance.SetInteractable(true);

        // trigger the first intro dialogue.
        ShowIntroDialogue();
    }

    // TODO logic for actions after specific dialogues are completed.
    public override void OnDialogueComplete(string id)
    {
        base.OnDialogueComplete(id);
        Debug.Log($"[BaseController] Dialogue completed: {id}");

        if (id == "base.intro.one")
        {
            Debug.Log($"[BaseController] Next dialogue.");
            ShowArrivalDialogue();
        }
        else if (id == "base.arrival.one")
        {
            ShowLettermanDialogue();
        }
    }
    public override void ShowSceneEntryDialogue(GameState state)
    {
        // run the parent method (if there is anything to run).
        base.ShowSceneEntryDialogue(state);

        if (!state.completedDialogues.Contains("base.intro.one"))
        {
            ShowIntroDialogue();
            return;
        }

        if (!state.completedDialogues.Contains("base.arrival.one"))
        {
            ShowArrivalDialogue();
            return;
        }

        if (!state.completedDialogues.Contains("base.letterman.one"))
        {
            ShowLettermanDialogue();
            return;
        }

    }

    public void ShowIntroDialogue()
    {
        DialogueManager.Instance.ShowDialogue("base.intro.one");
    }

    public void ShowArrivalDialogue()
    {
        backgroundImage.SetActive(true);
        DialogueManager.Instance.ShowDialogue("base.arrival.one");
    }
    
        public void ShowLettermanDialogue()
    {
        DialogueManager.Instance.ShowDialogue("base.letterman.one");
    }
}
