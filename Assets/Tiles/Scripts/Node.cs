using UnityEngine;

public enum TipoTile {
    Chave, Espinho, Bau, Normal, Bloqueado
}

public class Node : MonoBehaviour {
    [SerializeField]
    public Vector2 posicao { get; set; }
    [SerializeField]

    public TipoTile tipoTile{ get; set; }
}
