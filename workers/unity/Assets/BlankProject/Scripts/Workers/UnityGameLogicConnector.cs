using BlankProject.Scripts.Config;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Representation;
using Improbable.Gdk.GameObjectCreation;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Worker.CInterop;
using Improbable.Gdk.TransformSynchronization;
using Scripts.Worker;
using UnityEngine;

namespace BlankProject
{
    public class UnityGameLogicConnector : WorkerConnector{
        [SerializeField] private EntityRepresentationMapping entityRepresentationMapping = default;
        [SerializeField] private GameObject level;

        public const string WorkerType = "UnityGameLogic";
        private GameObject levelInstance;

        private async void Start(){
            PlayerLifecycleConfig.CreatePlayerEntityTemplate = EntityTemplates.CreatePlayerEntityTemplate;

            IConnectionFlow flow;
            ConnectionParameters connectionParameters;

            if (Application.isEditor){
                flow = new ReceptionistFlow(CreateNewWorkerId(WorkerType));
                connectionParameters = CreateConnectionParameters(WorkerType);
            }
            else{
                flow = new ReceptionistFlow(CreateNewWorkerId(WorkerType),
                    new CommandLineConnectionFlowInitializer());
                connectionParameters = CreateConnectionParameters(WorkerType,
                    new CommandLineConnectionParameterInitializer());
            }

            var builder = new SpatialOSConnectionHandlerBuilder()
                .SetConnectionFlow(flow)
                .SetConnectionParameters(connectionParameters);

            await Connect(builder, new ForwardingDispatcher()).ConfigureAwait(false);
        }

        protected override void HandleWorkerConnectionEstablished(){
            Worker.World.GetOrCreateSystem<MetricSendSystem>();
            PlayerLifecycleHelper.AddServerSystems(Worker.World);
            var gameObjectCreator = new GameObjectCreatorFromTransform(WorkerType, transform.position);
            GameObjectCreationHelper.EnableStandardGameObjectCreation(Worker.World, gameObjectCreator, entityRepresentationMapping);
            TransformSynchronizationHelper.AddServerSystems(Worker.World);
            // if no level make one....
            if (level == null){
                return;
            }

            levelInstance = Instantiate(level, transform.position, transform.rotation);
        }

        public override void Dispose(){
            
            if (levelInstance != null){
                Destroy(levelInstance);
            }

            base.Dispose();
        }
    }
}
