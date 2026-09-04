using UnityEngine;

public class Compass : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform directions;

    [SerializeField] private float compassWidth = 400f;

    private void Update()
    {
        float angle = player.eulerAngles.y;

        // Converte a rotação do jogador em movimento horizontal
        float x = -(angle / 360f) * compassWidth;

        directions.anchoredPosition = new Vector2(
            x,
            directions.anchoredPosition.y
        );
    }
}