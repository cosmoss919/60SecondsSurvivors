using _60SecondsSurvivors.Player;
using UnityEngine;

public class RePosition : MonoBehaviour
{
    Collider2D coll;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area"))
            return;

        var playerPos = PlayerController.Instance.Position;
        var myPos = transform.position;

        var diffX = Mathf.Abs(playerPos.x - myPos.x);
        var diffY= Mathf.Abs(playerPos.y - myPos.y);

        Vector3 playerDir = PlayerController.Instance.InputVec;
        var dirX = playerDir.x < 0 ? -1 : 1;
        var dirY = playerDir.y < 0 ? -1 : 1;

        switch (transform.tag)
        {
            case "Tile":
                if(diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 60);
                } else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * 60);
                }
                break;
            case "Enemy":
                if(coll.enabled)
                {
                    transform.Translate(playerDir * 30 + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f)));
                }
                break;
        }
    }
}
