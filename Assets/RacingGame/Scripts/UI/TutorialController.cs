using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialController : MonoBehaviour
{
    public Button backButton;
    public TMP_Text pageText;
    public TMP_Text chapterTitle;
    public TMP_Text bodyText;
    public ScrollRect bodyScroll;
    public Button prevButton;
    public Button nextButton;

    private int page = 0;

    private static readonly string[] Titles =
    {
        "Goal of the Game",
        "The Hub",
        "Staff",
        "R&D",
        "Car: Build",
        "Car: Setup",
        "Facilities",
        "Race Weekend",
        "Finances & Sponsors",
        "Standings",
        "Season & Progress",
        "Tips"
    };

    private static readonly string[] Bodies =
    {
        "You are the manager of a racing team — you don't drive, you make the decisions.\n\nYour job: build the fastest car, hire the best drivers and staff, and win races to climb the Drivers' and Constructors' championships across a 22-race season.\n\nEverything costs money. Spend wisely, earn from results and sponsors, and grow your team season after season.",

        "The Hub is your home base.\n\nTop bar: your Money (left) and Reputation (right). Tap Money to open Finances. Tap Reputation to open Standings.\n\nBottom navigation: HQ (overview), Staff, R&D, Car and Base.\n\nThe Next Race card shows the upcoming round — tap it to start the race weekend.",

        "Manage your people across tabs: Drivers, Engineers, Leaders, Market and Academy.\n\nHire talent from the Market, release or promote your roster, and set your two Race Drivers — only they score points on weekends. Academy prospects can be signed cheaply for the future.\n\nHigher ratings mean better performance, but everyone draws a salary — keep an eye on costs.",

        "Research & Development improves your cars.\n\nDepartments: upgrade them to raise their output rating.\n\nTech Tree: unlock nodes in each tree (Aerodynamics, Engine, Materials, Electronics). Each node permanently boosts both your cars.\n\nNodes cost money — or Research Points, which you earn from racing. Spend Research Points when you have enough to progress faster.",

        "On the BUILD tab you develop each of your two cars.\n\nSpend money to Develop an area — Aerodynamics, Engine, Chassis or Reliability. Each Develop raises that area's rating and increases the car's Overall Performance.\n\nA stronger car is faster on track. Costs rise as ratings climb.",

        "On the SETUP tab you tune each car for the next track.\n\nAdjust three sliders: Downforce, Balance and Reliability. The Setup Rating shows how close your setup is to the track's optimum — higher is better.\n\nTap Auto Setup to match the optimum instantly. Tune before every race; a good setup adds real pace.",

        "The Base screen holds your facilities: Factory, Wind Tunnel, Simulator, Material Lab and Data Center.\n\nUpgrade each to raise its level (up to 10). Better facilities strengthen your team's foundation.\n\nUpgrades cost money, and facilities have an upkeep that is deducted each race.",

        "A race weekend runs in four phases:\n\n1. Practice — run a session to prepare.\n2. Qualifying — sets the starting grid by pace.\n3. Race — watch the 2D race from above. Control speed (1x / 2x / 4x) or Skip to the end.\n4. Results — the final classification.\n\nThe top 10 score points: 25, 18, 15, 12, 10, 8, 6, 4, 2, 1.",

        "Open Finances by tapping Money in the Hub.\n\nOverview: your balance and the per-race breakdown — income versus expenses (salaries and facility upkeep).\n\nSponsors: sign deals to earn money every race. Each needs a minimum reputation and pays a signing bonus up front. You can end a deal anytime.\n\nAfter every race you are paid prize money plus sponsor income, minus salaries and upkeep.",

        "Open Standings by tapping Reputation in the Hub.\n\nDrivers: the drivers' championship, ranked by points.\nTeams: the constructors' championship.\n\nYour drivers and team are highlighted, and points accumulate across the season.",

        "Finish all 22 races to roll into a new season:\n\n- Prize money is paid by final constructor position.\n- A fresh calendar is generated and standings reset.\n- Drivers age by a year and contracts tick down.\n- Rival AI teams develop their cars.\n\nYour reputation rises with strong results — and higher reputation unlocks bigger sponsors.",

        "- Balance spending against income so you don't run dry.\n- Tune your car setup before every race.\n- Build reputation to unlock better sponsors.\n- Save Research Points for tech unlocks.\n- Use 2x / 4x or Skip to play through races quickly.\n\nGood luck, Team Principal!"
    };

    private void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(GoBack);
        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);

        ShowPage(0);
        TransitionManager.Instance.FadeIn();
    }

    private void GoBack()
    {
        SoundManager.Instance.PlayClick();
        TransitionManager.Instance.LoadScene("MainMenu");
    }

    private void Prev()
    {
        if (page <= 0) return;
        SoundManager.Instance.PlayClick();
        ShowPage(page - 1);
    }

    private void Next()
    {
        if (page >= Titles.Length - 1) return;
        SoundManager.Instance.PlayClick();
        ShowPage(page + 1);
    }

    private void ShowPage(int index)
    {
        page = Mathf.Clamp(index, 0, Titles.Length - 1);
        chapterTitle.text = Titles[page];
        bodyText.text = Bodies[page];
        pageText.text = (page + 1) + " / " + Titles.Length;

        prevButton.interactable = page > 0;
        nextButton.interactable = page < Titles.Length - 1;

        if (bodyScroll != null) bodyScroll.verticalNormalizedPosition = 1f;
    }
}
