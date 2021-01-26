using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.QueryBasedInterest;
using Improbable.Gdk.TransformSynchronization;
using UnityEngine;

namespace BlankProject.Scripts.Config{
    public static class EntityTemplates{
        public static EntityTemplate CreatePlayerEntityTemplate(EntityId entityId, string workerId, byte[] serializedArguments){
            var clientAttribute = EntityTemplate.GetWorkerAccessAttribute(workerId);
            var serverAttribute = UnityGameLogicConnector.WorkerType;

            var position = new Vector3(0, 1f, 0);
            var coords = Coordinates.FromUnityVector(position);

            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(coords), clientAttribute);
            template.AddComponent(new Metadata.Snapshot("Player"), serverAttribute);

            PlayerLifecycleHelper.AddPlayerLifecycleComponents(template, workerId, serverAttribute);
            TransformSynchronizationHelper.AddTransformSynchronizationComponents(template, clientAttribute, position);

            const int serverRadius = 500;
            var clientRadius = workerId.Contains(MobileClientWorkerConnector.WorkerType) ? 100 : 500;

            var serverQuery = InterestQuery.Query(Constraint.RelativeCylinder(serverRadius));
            var clientQuery = InterestQuery.Query(Constraint.RelativeCylinder(clientRadius));

            var interest = InterestTemplate.Create()
                .AddQueries<Metadata.Component>(serverQuery)
                .AddQueries<Position.Component>(clientQuery);
            template.AddComponent(interest.ToSnapshot(), serverAttribute);

            template.SetReadAccess(UnityClientConnector.WorkerType, MobileClientWorkerConnector.WorkerType, serverAttribute);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, serverAttribute);

            return template;
        }

        public static EntityTemplate CreateSphereTemplate(Quaternion rotation, Vector3 position = default){
           
            var serverAttribute = UnityGameLogicConnector.WorkerType;

            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(Coordinates.FromUnityVector(position)), serverAttribute);
            template.AddComponent(new Metadata.Snapshot("Sphere"), serverAttribute);
            template.AddComponent(new Persistence.Snapshot(), serverAttribute);

            TransformSynchronizationHelper.AddTransformSynchronizationComponents(template, serverAttribute, rotation, position);

            const int serverRadius = 500;

            var query = InterestQuery.Query(Constraint.RelativeCylinder(serverRadius));
            var interest = InterestTemplate.Create().AddQueries<Position.Component>(query);
            template.AddComponent(interest.ToSnapshot());

            template.SetReadAccess(UnityClientConnector.WorkerType, MobileClientWorkerConnector.WorkerType, serverAttribute);

            return template;
        }
    }
}
