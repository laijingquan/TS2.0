using UnityEngine;
using System.Collections;
using TrueSync;
namespace PoolEngine
{
    public class BallTriggerTest : TrueSyncBehaviour
    {

        // Use this for initialization
        private TSVector moveDir = TSVector.right*-1;
        public override void OnSyncedUpdate()
        {
            tsTransform.Translate(moveDir * Time.deltaTime);
        }

        public void OnSyncedTriggerEnter(TSCollision collision)
        {
            moveDir = collision.contacts[0].normal;
        }
    }
}
