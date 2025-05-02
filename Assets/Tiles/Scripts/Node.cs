using UnityEngine;

public class Node : MonoBehaviour {
    [SerializeField]
    public Vector2 posicao { get; set; }
    [SerializeField]
    public bool ehAndavel { get; set; } = true;
    [SerializeField]
    public float recompensa = 0f;
}
