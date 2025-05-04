using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Resolvedor : MonoBehaviour {
    public Color corAndado;
    public float velocidade = 2f;
    private Coroutine moverPorCaminhoCoroutine;
    private int hasKey = 0;

    void Start(){
        ResetarPosicaoResolvedor();
    }
    
    public void RodarQLearning(){
        StartCoroutine(QLearning());
    }

    private IEnumerator QLearning(){
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
        int estados = 50;
        int acoes = 4;
        float taxaAprendizado = 0.1f;
        float[,] Q = new float[estados,acoes];
        int episodios = 20;
        float recompensa = 0;

        int contador = 0;
        Node[,] grade = GerenciadorGrade.Instance.GetGrade();
        int altura = GerenciadorGrade.Instance.altura;
        int largura = GerenciadorGrade.Instance.largura;
        
        // Initialize Q table
        for (int m = 0; m < altura; m++){
            for (int n = 0; n < largura; n++){
                if(grade[m,n].tipoTile == TipoTile.Bloqueado){
                    Q[contador,0] = float.NegativeInfinity;
                    Q[contador,1] = float.NegativeInfinity;
                    Q[contador,2] = float.NegativeInfinity;
                    Q[contador,3] = float.NegativeInfinity;

                    Q[contador + 25,0] = float.NegativeInfinity;
                    Q[contador + 25,1] = float.NegativeInfinity;
                    Q[contador + 25,2] = float.NegativeInfinity;
                    Q[contador + 25,3] = float.NegativeInfinity;
                }
                else{
                    Q[contador,0] = 0;
                    Q[contador,1] = 0;
                    Q[contador,2] = 0;
                    Q[contador,3] = 0;

                    Q[contador + 25,0] = 0;
                    Q[contador + 25,1] = 0;
                    Q[contador + 25,2] = 0;
                    Q[contador + 25,3] = 0;
                }
                contador++;
            }
        }

        GerenciadorGrade gg = GerenciadorGrade.Instance;
        for (int i = 0; i < episodios; i++){ // Para cada Episódio
            Vector2 posAtual = gg.posicaoAtual;
            int s = (int)(posAtual.y * largura + posAtual.x) + hasKey;
            int objetivo = (int)(gg.posicaoBau.y * largura + gg.posicaoBau.x) + (estados / 2);

            while(s != objetivo){
                int melhorAcao = EncontrarMelhorAcao(s, acoes, Q);
                /*
                    0 - Esquerda
                    1 - Direita
                    2 - Cima
                    3 - Baixo
                */
                Vector2 destino = gg.posicaoAtual;
                switch(melhorAcao){
                    case 0:
                        destino = gg.posicaoAtual + new Vector2(-1, 0);
                        break;
                    case 1:
                        destino = gg.posicaoAtual + new Vector2(1, 0);
                        break;
                    case 2:
                        destino = gg.posicaoAtual + new Vector2(0, 1);
                        break;
                    case 3:
                        destino = gg.posicaoAtual + new Vector2(0, -1);
                        break;
                }
                
                // Check if destination is within grid bounds
                if (destino.x < 0 || destino.x >= largura || destino.y < 0 || destino.y >= altura) {
                    // Destination is out of bounds, skip this action
                    continue;
                }
                
                yield return StartCoroutine(MoverParaPosicaoCoroutine(gg.posicaoAtual, destino));

                int sLinha = (int)(destino.y * largura + destino.x) + hasKey;
                
                // Ensure correct indices for the grid
                int gridY = (int)destino.y;
                int gridX = (int)destino.x;
                
                // Make sure we're not accessing out of bounds
                if (gridY >= 0 && gridY < altura && gridX >= 0 && gridX < largura) {
                    Node nodeLinha = grade[gridY, gridX];

                    switch(nodeLinha.tipoTile){
                        case TipoTile.Bau:
                            if(hasKey == 25){
                                recompensa = 10;
                            }
                            else{
                                recompensa = -5;
                            }
                            break;
                        case TipoTile.Chave:
                            recompensa = 2.5f;
                            hasKey = 25;
                            break;
                        case TipoTile.Espinho:
                            recompensa = -10;
                            break;
                        case TipoTile.Normal:
                            recompensa = -0.1f;
                            break;
                    }
                    
                    // Make sure sLinha is valid for Q table
                    if (sLinha >= 0 && sLinha < estados) {
                        int bestActionForSLinha = EncontrarMelhorAcao(sLinha, acoes, Q);
                        Q[s, melhorAcao] = Q[s, melhorAcao] + taxaAprendizado * 
                            (recompensa + 0.5f * (Q[sLinha, bestActionForSLinha] - Q[s, melhorAcao]));
                        s = sLinha;
                    }
                }
            }
        }
    }
    
    private int EncontrarMelhorAcao(int s, int acoes, float[,] Q){
        int melhorAcao = 0;

        for(int j = 0; j < acoes; j++){
            if(Q[s, j] > Q[s, melhorAcao]){
                melhorAcao = j;
            }
        }
        return melhorAcao;
    }

    private List<Node> GetVizinhos(Node node, Node[,] grade) { // Funcao auxiliar para achar os vizinhos
        List<Node> vizinhos = new List<Node>();
        int x = (int)node.posicao.x;
        int y = (int)node.posicao.y;

        // Checa se há vizinhos nas 4 direcoes
        bool esquerda = x > 0;
        bool direita = x < grade.GetLength(1) - 1;  // Updated to use correct dimension
        bool cima = y < grade.GetLength(0) - 1;     // Updated to use correct dimension
        bool baixo = y > 0;

        bool reto = esquerda ^ baixo && esquerda ^ cima && direita ^ baixo && direita ^ cima;

        //Horizontal/Vertical
        if (esquerda) vizinhos.Add(grade[y, x - 1]); // Esquerda
        if (direita) vizinhos.Add(grade[y, x + 1]); // Direita
        if (baixo) vizinhos.Add(grade[y - 1, x]); // Baixo
        if (cima) vizinhos.Add(grade[y + 1, x]); // Cima

        //Diagonais
        if (esquerda && cima) vizinhos.Add(grade[y + 1, x - 1]); // Esquerda-Cima
        if (esquerda && baixo) vizinhos.Add(grade[y - 1, x - 1]); //Esquerda-Baixo
        if (direita && cima) vizinhos.Add(grade[y + 1, x + 1]); //Direita-Cima
        if (direita && baixo) vizinhos.Add(grade[y - 1, x + 1]); //Direita-Baixo
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
        GerenciadorGrade.Instance.posicaoAtual = posicaoDestino;
        transform.position = posicaoDestino;
    }
}