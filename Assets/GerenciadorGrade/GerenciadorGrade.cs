using UnityEngine;
using UnityEditor;
using System.IO;

public class GerenciadorGrade : MonoBehaviour {
    public int largura;
    public int altura;
    public float tamanhoUnidade = 1f;
    public GameObject tileBranco;
    public GameObject tilePreto;
    public GameObject tileInicio;
    public GameObject tileDestino;

    private Node[,] grade;
    public Vector2 posicaoInicio { get; private set; } // Get publico, mas set privado
    public Vector2 posicaoDestino { get; private set; } // Get publico, mas set privado
    private Transform pastaTiles;

    void GerarGrade() {
        grade = new Node[largura, altura];

        for (int x = 0; x < largura; x++) {
            for (int y = 0; y < altura; y++) {
                Vector2 posicaoAtual = new Vector2(x, y);

                //GameObject tile = Instantiate(tilePrefab, transform.position + new Vector3(x * tamanhoUnidade, y * tamanhoUnidade, 0), Quaternion.identity, pastaTiles);
                Node node = tile.GetComponent<Node>();
                node.posicao = posicaoAtual;

                grade[x, y] = node;

                tile.name = $"Tile {x},{y}";
            }
        }
    }

    public Node[,] GetGrade() { return grade; }
}
