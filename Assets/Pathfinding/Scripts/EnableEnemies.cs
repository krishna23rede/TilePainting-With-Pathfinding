using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableEnemies : MonoBehaviour
{
    public Enemy[] enemies;

    private void Awake()
    {
        foreach(Enemy i in enemies)
        {
            i.enabled = false; 
        }
    }
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        foreach (Enemy i in enemies)
        {
            i.enabled = true;
        }
    }

}
