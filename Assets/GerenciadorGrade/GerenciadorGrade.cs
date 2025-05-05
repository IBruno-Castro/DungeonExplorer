using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Tiles gerados por probabilidade
[System.Serializable]
public class TileData {
    [SerializeField]
    public GameObject tile;
    [SerializeField]
    public float probabilidade = 0f;
}

// Tiles com número fixo de aparições
[System.Serializable]
public class TilePadraoData {
    [SerializeField]
    public GameObject tile;
    [SerializeField]
    public int quantidade;
}

public class GerenciadorGrade : MonoBehaviour {
    public static GerenciadorGrade Instance { get; private set; }
    public Transform pastaTiles;

    [SerializeField]
    public int largura;
    [SerializeField]
    public int altura;
    [SerializeField]
    private float tamanhoUnidade = 1f;
    private float larguraOffset => (largura * tamanhoUnidade / -2) + tamanhoUnidade / 2;
    private float alturaOffset => (altura * tamanhoUnidade / -2) + tamanhoUnidade / 2;

    [SerializeField]
    private List<TileData> tiles = new();
    [SerializeField]
    private List<TilePadraoData> tilesPadrao = new();
    // Soma das probabilidades de cada tipo de tile
    private float maxProbabilidade = 0f;

    private GameObject[,] moldeGrade;
    private GameObject[,] instancias;
    public Node[,] grade { get; private set; }

    public Vector2 posicaoInicio { get; set; } // Get publico, mas set privado
    public Vector2 posicaoAtual { get; set; } // Get publico, mas set privado
    public Vector2 posicaoBau { get; set; } // Get publico, mas set privado

    [SerializeField]
    private int seed;


    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        instancias = new GameObject[largura, altura];
        grade = new Node[largura, altura];

        Random.InitState(seed == 0 ? System.DateTime.Now.GetHashCode() : seed);
        maxProbabilidade = tiles.Aggregate(0f, (acc, tile) => acc + tile.probabilidade);
        
        GerarGrade();
    }

    public void GerarGrade() {
        moldeGrade = new GameObject[largura, altura];

        GerarTilesPadrao();
        GerarTilesAleatorios();

        ResetGrade();
    }

    public void ResetGrade() {
        LimparGrade();

        for (int x = 0; x < largura; x++) {
            for (int y = 0; y < altura; y++) {
                Vector2 posicao = GetPosicao(x, y);
                
                GameObject instancia = Instantiate(moldeGrade[x,y], posicao, Quaternion.identity, pastaTiles);
                instancias[x,y] = instancia;

                grade[x,y] = instancia.GetComponent<Node>();
                grade[x,y].posicao = posicao;
            }
        }
    }

    public void MudarTile(int x, int y, GameObject tile) {
        Vector2 posicao = GetPosicao(x, y);
        
        Destroy(instancias[x,y]);
        instancias[x,y] = Instantiate(tile, posicao, Quaternion.identity, pastaTiles);
        
        grade[x,y] = instancias[x,y].GetComponent<Node>();
        grade[x,y].posicao = posicao;
    }

    private void LimparGrade() {
        for (int x = 0; x < largura; x++) {
            for (int y = 0; y < altura; y++) {
                if (instancias[x,y])
                    Destroy(instancias[x,y]);                
            }
        }
    }

    private void GerarTilesPadrao() {
        int linhas = moldeGrade.GetLength(0);
        int colunas = moldeGrade.GetLength(0);

        foreach (var tile in tilesPadrao) {
            int quantidadeSetada = 0;
            int index = tilesPadrao.IndexOf(tile);

            while (quantidadeSetada < tile.quantidade) {
                int x = Random.Range(0, linhas);
                int y = Random.Range(0, colunas);
                
                if (!moldeGrade[x,y]) {
                    moldeGrade[x,y] = tile.tile;
                    quantidadeSetada += 1;

                    
                }

                if(1 == tilesPadrao.IndexOf(tile)){
                    posicaoInicio = new Vector2(x,y);
                    posicaoAtual = posicaoInicio;
                    moldeGrade[x,y].GetComponent<Node>().tipoTile = TipoTile.Normal;
                }
                if(0 == tilesPadrao.IndexOf(tile)){
                    posicaoBau = new Vector2(x,y);
                    moldeGrade[x,y].GetComponent<Node>().tipoTile = TipoTile.Bau;
                }
                if(2 == tilesPadrao.IndexOf(tile)){
                    moldeGrade[x,y].GetComponent<Node>().tipoTile = TipoTile.Espinho;
                }
                if(3 == tilesPadrao.IndexOf(tile)){
                    moldeGrade[x,y].GetComponent<Node>().tipoTile = TipoTile.Chave;
                }
            }
        }
    }

    private void GerarTilesAleatorios() {
        for (int x = 0; x < largura; x++) {
            for (int y = 0; y < altura; y++) {
                if (moldeGrade[x,y]) continue;

                moldeGrade[x,y] = GetTilePrefab(x, y);
            }
        }
    }

    private GameObject GetTilePrefab(int x, int y) {        
        float prob = Random.Range(0f, maxProbabilidade);
        float soma = 0f;

        foreach (var tile in tiles) {
            soma += tile.probabilidade;
            int index = tiles.IndexOf(tile);

            if (soma > prob){
                moldeGrade[x,y] = tile.tile;

                if(index == 0)
                    moldeGrade[x,y].GetComponent<Node>().tipoTile = TipoTile.Normal;
                else
                    moldeGrade[x,y].GetComponent<Node>().tipoTile = TipoTile.Bloqueado;
                return moldeGrade[x,y];
            } 
        }
        return null;
    }

    public Node[,] GetGrade() {
        return grade;
    }

    public Vector2 GetPosicao(int x, int y) {
    Vector2 posicao = new Vector2(x, y) * tamanhoUnidade;
    posicao.x += larguraOffset;
    posicao.y += alturaOffset;
    return posicao;
    }

    private Vector2 GetIndex(Vector2 posicao) {
        int x = (int) ((posicao.x - larguraOffset) / tamanhoUnidade);
        int y = (int) ((posicao.y - alturaOffset) / tamanhoUnidade);
        return new Vector2(x, y);
    }
}
