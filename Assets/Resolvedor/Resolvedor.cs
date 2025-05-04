using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Resolvedor : MonoBehaviour {
    public Color corAndado;
    public float velocidade = 2f;
    private Coroutine moverPorCaminhoCoroutine;
    private bool hasKey;

    void Start(){

        ResetarPosicaoResolvedor();
    }

    private void QLearning(){
        /*
        1: function APRENDIZAGEM QLearning
        2: Inicializar a Função Ação-Valor Q (por exemplo, 0 para todos os estados)
        3: for cada episódio do:
        4: Inicializar o estado inicial s
        5: for cada passo do episódio do:
        6: a ← acao para o estado s derivada da Tabela Q (por exemplo, estrategia épsilon-greedy)
        7: executar ação a, observar novo estado s0 e recompensa r
        8: Q(s, a) ← Q(s, a) + α[r + γmaxa0Q(s0, a0) − Q(s, a)]
        9: s ← s0
        10: if estado s e terminal ´ then break
        */
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

    public void ResetarPosicaoResolvedor() {
        StopAllCoroutines();
        moverPorCaminhoCoroutine = null;
        transform.position = (Vector3) GerenciadorGrade.Instance.posicaoInicio + GerenciadorGrade.Instance.transform.position;
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