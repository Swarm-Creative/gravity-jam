using System;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Representation;
using Improbable.Gdk.GameObjectCreation;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.TransformSynchronization;
using Scripts.Worker;
using UnityEngine;

namespace BlankProject{
    public class UnityClientConnector : WorkerConnector{
        [SerializeField] private EntityRepresentationMapping entityRepresentationMapping = default;
        [SerializeField] private GameObject level;

        public const string WorkerType = "UnityClient";

        private GameObject levelInstance;
        private async void Start(){
            
            var connParams = CreateConnectionParameters(WorkerType);

            var builder = new SpatialOSConnectionHandlerBuilder()
                .SetConnectionParameters(connParams);

            if (!Application.isEditor){
                var initializer = new CommandLineConnectionFlowInitializer();
                switch (initializer.GetConnectionService()){
                    case ConnectionService.Receptionist:
                        builder.SetConnectionFlow(new ReceptionistFlow(CreateNewWorkerId(WorkerType), initializer));
                        break;
                    case ConnectionService.Locator:
                        builder.SetConnectionFlow(new LocatorFlow(initializer));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else{
                builder.SetConnectionFlow(new ReceptionistFlow(CreateNewWorkerId(WorkerType)));
            }

            await Connect(builder, new ForwardingDispatcher()).ConfigureAwait(false);
        }

        protected override void HandleWorkerConnectionEstablished(){
            Worker.World.GetOrCreateSystem<MetricSendSystem>();
            PlayerLifecycleHelper.AddClientSystems(Worker.World);
            var gameObjectCreator = new GameObjectCreatorFromTransform(WorkerType, transform.position);
            GameObjectCreationHelper.EnableStandardGameObjectCreation(Worker.World, gameObjectCreator, entityRepresentationMapping);
            TransformSynchronizationHelper.AddClientSystems(Worker.World);

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
