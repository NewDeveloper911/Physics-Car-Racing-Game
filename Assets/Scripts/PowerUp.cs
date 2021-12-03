using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] GameObject car;
    [SerializeField] string[] pos_abilities;
    [SerializeField] ParticleSystem collection;

    // Start is called before the first frame update
    void Start()
    {
        collection.gameObject.SetActive(false);

        //Here, I'll initiate all possible abilities to collect from powerup
        pos_abilities[0] = "Imagine a red shell";
        pos_abilities[1] = "Imagine a blue shell";
        pos_abilities[2] = "Imagine a green shell";
        pos_abilities[3] = "Imagine a red mushroom";
        pos_abilities[4] = "Imagine three red mushrooms";
        pos_abilities[5] = "Imagine a golden mushroom mushroom";
        pos_abilities[6] = "Imagine a banana";
        pos_abilities[7] = "Imagine three bananas";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            //When the player touches the powerup box, it'll receive a random powerup
            Debug.Log(pos_abilities[Random.Range(0, pos_abilities.Length)]);
            //Powerup occurs and the box self-destructs
            collection.gameObject.SetActive(true);
            Destroy(this.gameObject, 2f);
        }
    }
}
