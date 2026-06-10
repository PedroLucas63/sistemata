using UnityEngine;
using UnityEngine.Tilemaps;

namespace Sistemata.Map
{
    public class BackgroundController : MonoBehaviour
    {

        [SerializeField] private bool loopX;
        [SerializeField] private bool loopZ;

        [SerializeField] private float loopOffsetX;
        [SerializeField] private float loopOffsetZ;

        private float startPosX, startPosZ;
        private float lengthX, lengthZ;

        public GameObject cam;

        void Start()
        {

            startPosX = transform.position.x;
            startPosZ = transform.position.z;

            Tilemap tm = this.GetComponent<Tilemap>();
            lengthX = tm.localBounds.size.x;
            lengthZ = tm.localBounds.size.y; // aqui precisa usar eixo y, por causa da rota��o do grid em 90�
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            float movementX, movementZ;

            movementX = cam.transform.position.x;
            movementZ = cam.transform.position.z;

            transform.position = new Vector3(
                loopX ? startPosX : transform.position.x,
                transform.position.y,
                loopZ ? startPosZ : transform.position.z
            );

            if (loopX)
            {
                if (movementX > startPosX + lengthX)
                    startPosX += loopOffsetX;
                else if (movementX < startPosX - lengthX)
                    startPosX -= loopOffsetX;
            }

            if (loopZ)
            {
                if (movementZ > startPosZ + lengthZ * 0.5f)
                    startPosZ += loopOffsetZ;
                else if (movementZ < startPosZ - lengthZ * 0.5f)
                    startPosZ -= loopOffsetZ;
            }

        }
    }
}