using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
        {
            return;
        }

        if(GameManager._instance != null) 
        {
            Vector3 localPlayerPos = GameManager._instance.transform.position;
            Vector3 myPos = transform.position;
            float diffX = Mathf.Abs(localPlayerPos.x - myPos.x);
            float diffY = Mathf.Abs(localPlayerPos.y - myPos.y);

            Vector3 localPlayerDir = GameManager._instance._localPlayer.inputVector;
            float dirX = localPlayerDir.x < 0 ? -1 : 1;
            float dirY = localPlayerDir.y < 0 ? -1 : 1;

            switch (transform.tag)
            {
                case "Ground":
                    transform.Translate(localPlayerDir.x * 40, localPlayerDir.y * 40, 0);
                    break;
                case "Enemy":

                    break;
            }
        }
        
    }
}
