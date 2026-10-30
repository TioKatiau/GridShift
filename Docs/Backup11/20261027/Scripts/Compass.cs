using UnityEngine;
using TMPro;

public class Compass : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform directions;

    private TMP_Text compassText;
    private float cycleWidth;

    private void Start()
    {
        compassText = directions.GetComponent<TMP_Text>();

        compassText.enableWordWrapping = false;
        compassText.ForceMeshUpdate();

        cycleWidth = compassText.preferredWidth / 3f;
    }

    private void Update()
    {
        float angle = player.eulerAngles.y;

        float normalizedAngle = Mathf.Repeat(angle + 180f, 360f) - 180f;

        float x = -(normalizedAngle / 360f) * cycleWidth;

        directions.anchoredPosition = new Vector2(
            x,
            directions.anchoredPosition.y
        );
    }
}