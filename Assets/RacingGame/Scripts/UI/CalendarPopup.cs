using UnityEngine;
using UnityEngine.UI;

public class CalendarPopup : PopupBase
{
    public Button closeButton;
    public RectTransform listContent;
    public GameObject rowTemplate;

    private bool wired;
    private bool built;

    private void OnEnable()
    {
        Wire();
        if (!built) BuildRows();
    }

    private void Wire()
    {
        if (wired) return;
        wired = true;
        closeButton.onClick.AddListener(Hide);
    }

    private void BuildRows()
    {
        built = true;
        GameState st = GameManager.Instance.State;
        if (st == null)
        {
            Debug.LogWarning("CalendarPopup: GameState is null");
            return;
        }

        for (int i = 0; i < st.season.calendar.Count; i++)
        {
            RaceData r = st.season.calendar[i];
            GameObject row = Instantiate(rowTemplate, listContent);
            row.SetActive(true);
            CalendarRow cr = row.GetComponent<CalendarRow>();
            cr.roundText.text = "R" + r.round;
            cr.trackText.text = r.trackName;
            cr.countryText.text = r.country;
            cr.infoText.text = r.laps + " laps   Wet " + Mathf.RoundToInt(r.weatherWetChance * 100f) + "%";
        }
    }
}
