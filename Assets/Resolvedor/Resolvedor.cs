using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Resolvedor : MonoBehaviour {
    public Color corAndado;
    public float velocidade = 2f;
    private Coroutine moverPorCaminhoCoroutine;

    public void ResolverLabirinto() {
        Node[,] grade = GerenciadorGrade.Instance.GetGrade(); // Pega a grade do problema


    }


    private List<Node> GetVizinhos(Node node, Node[,] grade) { // Funcao auxiliar para achar os vizinhos
        List<Node> vizinhos = new List<Node>();
        int x = (int)node.posicao.x;
        int y = (int)node.posicao.y;

        // Checa se há vizinhos nas 4 direcoes
        bool esquerda = x > 0;
        bool direita = x < grade.GetLength(0) - 1;
        bool cima = y < grade.GetLength(1) - 1;
        bool baixo = y > 0;

        bool reto = esquerda ^ baixo && esquerda ^ cima && direita ^ baixo && direita ^ cima;

        //Horizontal/Vertical
        if (esquerda) vizinhos.Add(grade[x - 1, y]); // Esquerda
        if (direita) vizinhos.Add(grade[x + 1, y]); // Direita
        if (baixo) vizinhos.Add(grade[x, y - 1]); // Baixo
        if (cima) vizinhos.Add(grade[x, y + 1]); // Cima

        //Diagonais
        if (esquerda && cima) vizinhos.Add(grade[x - 1, y + 1]); // Esquerda-Cima
        if (esquerda && baixo) vizinhos.Add(grade[x - 1, y - 1]); //Esquerda-Baixo
        if (direita && cima) vizinhos.Add(grade[x + 1, y + 1]); //Direita-Cima
        if (direita && baixo) vizinhos.Add(grade[x + 1, y - 1]); //Direita-Baixo
        return vizinhos;
    }

    private void ColorirTile(Color cor, GameObject tile) {
        tile.GetComponent<SpriteRenderer>().color = cor;
    }

    public void ResetarPosicaoResolvedor() {
        StopAllCoroutines();
        moverPorCaminhoCoroutine = null;
        transform.position = (Vector3) GerenciadorGrade.Instance.posicaoInicio + GerenciadorGrade.Instance.transform.position;
    }

    private IEnumerator MoverPorCaminhoCoroutine(List<Node> caminho) {
        if (moverPorCaminhoCoroutine != null) yield break; // Quebrando pois o jogador ja esta andando

        if (caminho == null || caminho.Count == 0) { // Quebrando pois o caminho nao existe, ou eh impossivel
            Debug.LogWarning("Caminho Impossivel");
            yield break;
        }

        for (int i = 0; i < caminho.Count; i++) {
            Node nodoAtual = caminho[i];

            ColorirTile(corAndado, nodoAtual.gameObject);

            if (i < caminho.Count - 1) {
                Node proximoNodo = caminho[i + 1];
                Vector2 posicaoAtual = nodoAtual.transform.position; // Pegando a posicao relativa ao mundo
                Vector2 posicaoDestino = proximoNodo.transform.position; // Pegando a posicao relativa ao mundo

                yield return StartCoroutine(MoverParaPosicaoCoroutine(posicaoAtual, posicaoDestino));
            }
        }
        moverPorCaminhoCoroutine = null;
    }

    private IEnumerator MoverParaPosicaoCoroutine(Vector3 posicaoAtual, Vector3 posicaoDestino) {
        float tempo = 0f;
        float distancia = Vector3.Distance(posicaoAtual, posicaoDestino);

        while (tempo < distancia / velocidade) {
            tempo += Time.deltaTime;
            float interpolacao = tempo / (distancia / velocidade);
            transform.position = Vector3.Lerp(posicaoAtual, posicaoDestino, interpolacao);
            yield return null;
        }

        transform.position = posicaoDestino;
    }
}