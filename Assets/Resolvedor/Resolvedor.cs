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
    private int hasKey = 0;

    [Header("Data")]
    [SerializeField] private int estados = 50;
    [SerializeField] private int acoes = 4;
    [SerializeField] private float taxaAprendizado = 1f;
    [SerializeField] private float taxaDesconto = 0.5f;
    [SerializeField] private float epsilon = 0.8f;
    [SerializeField] private int episodios = 100;

    void Start(){
        ResetarPosicaoResolvedor();
    }
    
    public void RodarQLearning(){
        ResetarPosicaoResolvedor(); // Garantir que comece na posição correta
        StartCoroutine(QLearning());
    }

    private IEnumerator QLearning(){
        float[,] Q = new float[estados, acoes];
        
        Node[,] grade = GerenciadorGrade.Instance.GetGrade();
        int altura = GerenciadorGrade.Instance.altura;
        int largura = GerenciadorGrade.Instance.largura;
        
        //inicalização tabela Q
        for (int m = 0; m < altura; m++){
            for (int n = 0; n < largura; n++){
                int estadoBase = m * largura + n;
                
                if(grade[m,n].tipoTile == TipoTile.Bloqueado){
                    for(int a = 0; a < acoes; a++) {
                        Q[estadoBase, a] = float.NegativeInfinity;
                        Q[estadoBase + 25, a] = float.NegativeInfinity;
                    }
                }
                else{
                    for(int a = 0; a < acoes; a++) {
                        Q[estadoBase, a] = Random.Range(-0.1f, 0.1f);
                        Q[estadoBase + 25, a] = Random.Range(-0.1f, 0.1f);
                    }
                }
            }
        }
    
        GerenciadorGrade gg = GerenciadorGrade.Instance;
        
        //para cada episódio
        for (int i = 0; i < episodios; i++){ 
            Debug.Log($"Iniciando episódio {i + 1}");
            
            //inicializar estado
            gg.posicaoAtual = gg.posicaoInicio;
            

            float tamTile = 1.0f;
            Vector3 posicaoMundo = new Vector3(gg.posicaoInicio.x * tamTile, gg.posicaoInicio.y * tamTile, 0f);
            
            transform.position = posicaoMundo;
            hasKey = 0;
            
            int maxPassos = altura * largura * 4;
            int passoAtual = 0;
            
            int estadoAtual = (int)(gg.posicaoAtual.y * largura + gg.posicaoAtual.x) + hasKey;
    
            bool objetivoAlcancado = false;

            taxaAprendizado -= 0.05f;
            epsilon -= 0.05f;
            
            while(!objetivoAlcancado && passoAtual < maxPassos){
                passoAtual++;
                
                // Epsilon Greedy
                int acaoEscolhida;
                if (Random.value < epsilon) {
                    List<int> acoesValidas = new List<int>();
                    for (int a = 0; a < acoes; a++) {
                        Vector2 target = CalcularDestino(gg.posicaoAtual, a);
                        if (EhPosicaoValida(target, altura, largura) && 
                            grade[(int)target.y, (int)target.x].tipoTile != TipoTile.Bloqueado) {
                            acoesValidas.Add(a);
                        }
                    }
                    
                    if (acoesValidas.Count > 0) {
                        acaoEscolhida = acoesValidas[Random.Range(0, acoesValidas.Count)];
                    } 
                    else {
                        break;
                    }
                } 
                else {
                    acaoEscolhida = EncontrarMelhorAcao(estadoAtual, acoes, Q, altura, largura, grade, gg.posicaoAtual);
                }
                
                Vector2 destino = CalcularDestino(gg.posicaoAtual, acaoEscolhida);
                
                if (!EhPosicaoValida(destino, altura, largura) || 
                    grade[(int)destino.y, (int)destino.y].tipoTile == TipoTile.Bloqueado) {
                    Q[estadoAtual, acaoEscolhida] = float.NegativeInfinity;
                    continue;
                }

                gg.posicaoAtual = destino;
                yield return StartCoroutine(MoverParaPosicaoCoroutine(gg.posicaoAtual, destino));
                
                int novoEstado = (int)(destino.y * largura + destino.x) + hasKey;
                float recompensa = CalcularRecompensa(grade[(int)destino.x, (int)destino.y].tipoTile);

                Debug.Log(grade[(int)destino.x, (int)destino.y].tipoTile + " - Recompensa: " + recompensa + (hasKey == 25 ? " - Tem chave" : " - Não tem chave"));
                
                if(grade[(int)destino.x, (int)destino.y].tipoTile == TipoTile.Chave && hasKey == 0){
                    hasKey = 25;
                    novoEstado = (int)(destino.y * largura + destino.x) + hasKey;
                    Debug.Log("Chave coletada!");
                }
                
                if(grade[(int)destino.x, (int)destino.y].tipoTile == TipoTile.Bau && hasKey == 25){
                    Debug.Log("Objetivo alcançado! Jogo finalizado!");
                    recompensa = 10;
                    objetivoAlcancado = true;
                }
                
                int melhorAcaoProximoEstado = EncontrarMelhorAcao(novoEstado, acoes, Q, altura, largura, grade, destino);
                float valorQ = Q[estadoAtual, acaoEscolhida];
                float valorQProximoEstado = Q[novoEstado, melhorAcaoProximoEstado];
                
                Q[estadoAtual, acaoEscolhida] = valorQ + taxaAprendizado * (recompensa + taxaDesconto * valorQProximoEstado - valorQ);
                
                estadoAtual = novoEstado;
                
                if (objetivoAlcancado) {
                    Debug.Log($"Episódio {i+1} finalizado com sucesso! O agente encontrou o baú com a chave!");
                    break;
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            if (passoAtual >= maxPassos) {
                Debug.LogWarning($"Episódio {i+1} terminado por atingir limite máximo de passos");
            } else if (!objetivoAlcancado) {
                Debug.Log($"Episódio {i+1} completado em {passoAtual} passos");
            }
            
            yield return new WaitForSeconds(0.5f);

            gg.ResetGrade();
        }
        
        Debug.Log("Treinamento Q-Learning concluído!");
    }
    
    private Vector2 CalcularDestino(Vector2 posicaoAtual, int acao) {
        switch(acao){
            case 0: return posicaoAtual + new Vector2(-1, 0); // Esquerda
            case 1: return posicaoAtual + new Vector2(1, 0);  // Direita
            case 2: return posicaoAtual + new Vector2(0, 1);  // Cima
            case 3: return posicaoAtual + new Vector2(0, -1); // Baixo
            default: return posicaoAtual;
        }
    }
    
    private bool EhPosicaoValida(Vector2 posicao, int altura, int largura) {
        return posicao.x >= 0 && posicao.x < largura && posicao.y >= 0 && posicao.y < altura;
    }
    
    private float CalcularRecompensa(TipoTile tipoTile) {
        switch(tipoTile) {
            case TipoTile.Bau:
                return hasKey == 25 ? 10 : -5;
            case TipoTile.Chave:
                if(hasKey == 0){
                    return 5f;
                }
                return -0.1f;
            case TipoTile.Espinho:
                return -10; // Sempre deve dar recompensa negativa
            case TipoTile.Normal:
                return -0.1f;
            case TipoTile.Bloqueado:
                return float.NegativeInfinity;
            default:
                return -0.2f;
        }
    }
    private int EncontrarMelhorAcao(int estadoAtual, int acoes, float[,] Q, int altura, int largura, Node[,] grade, Vector2 posicaoAtual) {
        int melhorAcao = 0;
        float melhorValor = float.NegativeInfinity;
    
        for(int a = 0; a < acoes; a++) {
            Vector2 destino = CalcularDestino(posicaoAtual, a);
            
            if (EhPosicaoValida(destino, altura, largura) && 
                grade[(int)destino.y, (int)destino.y].tipoTile != TipoTile.Bloqueado) {
                if (Q[estadoAtual, a] > melhorValor) {
                    melhorValor = Q[estadoAtual, a];
                    melhorAcao = a;
                }
            }
        }
        
        return melhorAcao;
    }

    public void ResetarPosicaoResolvedor() {
        StopAllCoroutines();
        
        float tamTile = 1.0f;
        
        GerenciadorGrade gg = GerenciadorGrade.Instance;
        Vector3 posicaoMundo = new Vector3(gg.posicaoInicio.x * tamTile, gg.posicaoInicio.y * tamTile, 0f);
        
        transform.position = posicaoMundo;
    }

    private IEnumerator MoverParaPosicaoCoroutine(Vector2 posicaoAtual, Vector2 posicaoDestino) {
        GerenciadorGrade gg = GerenciadorGrade.Instance;
        
        float tamTile = 1.0f; 
        
        Vector3 posicaoMundoAtual = new Vector3(posicaoAtual.x * tamTile, posicaoAtual.y * tamTile, 0f) + gg.transform.position;
        Vector3 posicaoMundoDestino = new Vector3(posicaoDestino.x * tamTile, posicaoDestino.y * tamTile, 0f) + gg.transform.position;
        
        
        float tempo = 0f;
        float distancia = Vector3.Distance(posicaoMundoAtual, posicaoMundoDestino);

        while (tempo < distancia / velocidade) {
            tempo += Time.deltaTime;
            float interpolacao = tempo / (distancia / velocidade);
            transform.position = Vector3.Lerp(posicaoMundoAtual, posicaoMundoDestino, interpolacao);
            yield return null;
        }
        
        gg.posicaoAtual = posicaoDestino;
        transform.position = posicaoMundoDestino;
    }

    public void Acelerar(){
        Time.timeScale = 5;
    }
    public void Desacelerar(){
        Time.timeScale = 1;

    }
}