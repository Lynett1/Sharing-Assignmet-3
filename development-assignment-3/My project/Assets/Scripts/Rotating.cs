using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rotating : MonoBehaviour
{
    [SerializeField] private GameObject Cube;
    private Coroutine rotateCoroutine;
    private Coroutine countDownCoroutine;

    public void OnEnable()
    {
        rotateCoroutine = StartCoroutine(Rotate());
        countDownCoroutine = StartCoroutine(CountDown());
    }
    
    private IEnumerator Rotate()
    {
        Vector3 rotationChange = Random.rotationUniform.eulerAngles;
        
        while (true)
        {
            transform.Rotate(rotationChange * Time.deltaTime);
            yield return null;
        }
    }
    
    private IEnumerator CountDown()
    {
        yield return new WaitForSeconds(3);
        StopCoroutine(rotateCoroutine);
        StopCoroutine(countDownCoroutine);
        Debug.Log("Destroyed Cube");
        Destroy(gameObject);
    }
        
    
}
