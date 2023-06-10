using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    //Declarando as variáveis
    public Transform player;
    public Transform bulletSpawn;
    public Slider healthBar;   
    public GameObject bulletPrefab;
    public LayerMask mascaraObstaculos;

    NavMeshAgent agent;
    public Vector3 destination;
    public Vector3 target;      
    float health = 100.0f;
    float shotRange = 40.0f;
    public GameObject gw;

    //Criando os métodos para a árvore de comportamento    
    [Task]
    //Método para pegar um destino aleatório no mapa
    public void PickRandomDestination()
    {        
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
        agent.SetDestination(dest);
        Task.current.Succeed();
    }
    [Task]
    //Método para o NPC andar para um destino
    public void MoveToDestination()
    {
        if (Task.isInspected)
        Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    //Método para retornar com a vida dele
    public bool IsHealthLessThan(float health)
    {
        return this.health < health;
    }

    [Task]
    //Método para destruir o NPC e ativar a tela de vitória
    public bool Explode()
    {
        gw.SetActive(true);
        Destroy(healthBar.gameObject);
        Destroy(this.gameObject);
        return true;
    }

    [Task]
    //Método para verificar se o NPC está vendo o player
    public bool SeePlayer(){
        RaycastHit hit;
        Vector3 direcaoJogador = player.position - transform.position;
        if (Physics.Linecast(transform.position, player.position, out hit, mascaraObstaculos))
        {
            return false;
        }
        else
        {
            Debug.Log("Tem visão direta");
            return true;
        }
    }

    [Task]
    //Método para o NPC olhar diretamente para o Player
    public bool LookAtTarget(){
        Vector3 direcaoJogador = player.position - transform.position;
        direcaoJogador.y = 0f;
        if (direcaoJogador != Vector3.zero)
        {
            Quaternion novaRotacao = Quaternion.LookRotation(direcaoJogador);
            transform.rotation = Quaternion.Slerp(transform.rotation, novaRotacao, Time.deltaTime * 5f);
            return true;
        }
        else return false;
    }

    [Task]
    //Método para sair o tiro
    public void Fire(){           
        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward*2000);            
    }

    void Start()
    {
        //Declarando o agente e ataulizando a vida
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        InvokeRepeating("UpdateHealth",5,0.5f);
    }

    void Update()
    {
        //Atualizando a barra de vida
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)health;
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
    }

    //Método para atualizar a vida
    void UpdateHealth()
    {
       if(health < 100)
        health ++;
    }

    //Se colidir com algo com a tag "bullet", a vida diminui em 10
    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }
        
}
