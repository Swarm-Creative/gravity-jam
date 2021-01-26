using BlankProject;
using BlankProject.Scripts.Config;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.Subscriptions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Player
{
    [WorkerType(UnityClientConnector.WorkerType)]
    public class SphereSpawnerBehaviour : MonoBehaviour{
        [Require] private PositionWriter positionWriter;
        [Require] private WorldCommandSender worldCommandSender;

        private void Update(){
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendSpawnSphereCommand();
            }
        }

        private void SendSpawnSphereCommand(){
            var randomPosition = new Vector3{
                x = Random.Range(-50f, 50f),
                y = Random.Range(0.5f, 5f),
                z = Random.Range(-50f, 50f)
            };

            var q = new Quaternion();
            var sphereTemplate = EntityTemplates.CreateSphereTemplate(q,randomPosition);

            worldCommandSender.SendCreateEntityCommand(new WorldCommands.CreateEntity.Request(sphereTemplate));
        }
    }
}