using UnityEngine;

public enum TipoTile {
    Chave, Espinho, Bau, Normal, Bloqueado
}

public class Node : MonoBehaviour {
    [SerializeField]
    private Vector2 _posicao;
    
    [SerializeField]
    private TipoTile _tipoTile;
    
    // Propriedades para acesso externo, se necessário
    public Vector2 posicao {
        get { return _posicao; }
        set { _posicao = value; }
    }
    
    public TipoTile tipoTile {
        get { return _tipoTile; }
        set { _tipoTile = value; }
    }
}
