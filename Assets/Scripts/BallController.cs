using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private Controller _controller;
    
    // Start is called before the first frame update
    void Start()
    {
        _controller = FindObjectOfType<Controller>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Muerte"))
        {
            _controller.GameOverScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Agujero"))
        {
            _controller.playing = false;
            StartCoroutine(CaerCoroutine());
        }
    }

    IEnumerator CaerCoroutine()
    {
        float i = 0;
        while (i < 0.5)
        {
            gameObject.transform.position += new Vector3(0, 0, 0.11f);
            i += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1);
        _controller.GameOverScene();
    }
}
