using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TileData {
    [SerializeField]
    public GameObject tile;
    [SerializeField]
    public float probabilidade = 0f;
}

[System.Serializable]
public class TilePadraoData {
    [SerializeField]
    public GameObject tile;
    [SerializeField]
    public int quantidade;
}

public class GerenciadorGrade : MonoBehaviour {
    [SerializeField]
    private int largura;
    [SerializeField]
    private int altura;
    [SerializeField]
    private float tamanhoUnidade = 1f;

    [SerializeField]
    private List<TileData> tiles = new();
    [SerializeField]
    private List<TilePadraoData> tilesPadrao = new();

    private Node[,] grade;
    public Vector2 posicaoInicio { get; private set; } // Get publico, mas set privado
    public Vector2 posicaoDestino { get; private set; } // Get publico, mas set privado

    private float maxProbabilidade = 0f;

    void Start() {
        maxProbabilidade = tiles.Aggregate(0f, (acc, tile) => acc + tile.probabilidade);
        GerarGrade();
    }

    public void GerarGrade() {
        grade = new Node[largura, altura];

        GerarTilesPadrao(grade);

        for (int x = 0; x < largura; x++) {
            for (int y = 0; y < altura; y++) {
                if (grade[x,y]) continue;

                Vector2 posicao = new(x, -y);

                GameObject tile = GetTile(posicao);

                grade[x, y] = tile.GetComponent<Node>();
                grade[x, y].posicao = posicao;
            }
        }
    }

    private void GerarTilesPadrao(Node[,] grade) {
        int linhas = grade.GetLength(0);
        int colunas = grade.GetLength(0);

        foreach (var tile in tilesPadrao) {
            int quantidadeSetada = 0;

            while (quantidadeSetada < tile.quantidade) {
                int x = Random.Range(0, linhas);
                int y = Random.Range(0, colunas);
                
                if (!grade[x,y]) {
                    Vector2 posicao = new(x, -y);

                    GameObject _tile = Instantiate(tile.tile, posicao, Quaternion.identity);

                    grade[x, y] = _tile.GetComponent<Node>();
                    grade[x, y].posicao = posicao;
            
                    quantidadeSetada += 1;
                }
            }
        }
    }

    private GameObject GetTile(Vector2 posicao) {
        GameObject prefab;
        
        float prob = Random.Range(0f, maxProbabilidade);
        float soma = 0f;

        foreach (var tile in tiles) {
            soma += tile.probabilidade;

            if (soma > prob) {
                prefab = tile.tile;
                
                GameObject tileInstance = Instantiate(prefab, posicao, Quaternion.identity);
                tileInstance.name = $"Tile {posicao.x},{ posicao.y}";
                
                return tileInstance;
            }
        }

        return null;
    }

    public Node[,] GetGrade() {
        return grade;
    }
}
