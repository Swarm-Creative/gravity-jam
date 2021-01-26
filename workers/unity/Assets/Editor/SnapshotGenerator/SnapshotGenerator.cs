using System.IO;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.QueryBasedInterest;
using UnityEditor;
using BlankProject.Scripts.Config;
using UnityEngine;

using Snapshot = Improbable.Gdk.Core.Snapshot;

namespace BlankProject.Editor
{
    internal static class SnapshotGenerator{
        private static string DefaultSnapshotPath = Path.GetFullPath(
            Path.Combine(
                Application.dataPath,
                "..",
                "..",
                "..",
                "snapshots",
                "default.snapshot"));

        [MenuItem("SpatialOS/Generate snapshot")]
        public static void Generate(){
            Debug.Log("Generating snapshot.");
            var snapshot = CreateSnapshot();

            Debug.Log($"Writing snapshot to: {DefaultSnapshotPath}");
            snapshot.WriteToFile(DefaultSnapshotPath);
        }

        private static Snapshot CreateSnapshot(){
            var snapshot = new Snapshot();

            AddPlayerSpawner(snapshot);
            
            AddSpheres(snapshot);

            return snapshot;
        }

        private static void AddSpheres(Snapshot snapshot){
            AddSphere(snapshot, new Vector3(-10f, 0.5f, 10f));
            AddSphere(snapshot, new Vector3(10f, 5f, 10f));
            AddSphere(snapshot, new Vector3(25f, 0.5f, 0f));

            var rotation = new Quaternion
            {
                eulerAngles = new Vector3(90, 0, 0)
            };
            AddSphere(snapshot, new Vector3(-4f, 0.5f, 4f), rotation);
            AddSphere(snapshot, new Vector3(-4f, 0.5f, 7f));
        }

        private static void AddSphere(Snapshot snapshot, Vector3 position){
            AddSphere(snapshot, position, Quaternion.identity);
        }

        // Now takes an optional rotation argument.
        private static void AddSphere(Snapshot snapshot, Vector3 position, Quaternion rotation){
            var template = EntityTemplates.CreateSphereTemplate(rotation, position);
            snapshot.AddEntity(template);
        }

        private static void AddPlayerSpawner(Snapshot snapshot){
            var serverAttribute = UnityGameLogicConnector.WorkerType;

            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(), serverAttribute);
            template.AddComponent(new Metadata.Snapshot("PlayerCreator"), serverAttribute);
            template.AddComponent(new Persistence.Snapshot(), serverAttribute);
            template.AddComponent(new PlayerCreator.Snapshot(), serverAttribute);

            var query = InterestQuery.Query(Constraint.RelativeCylinder(500));
            var interest = InterestTemplate.Create()
                .AddQueries<Position.Component>(query);
            template.AddComponent(interest.ToSnapshot(), serverAttribute);

            template.SetReadAccess(
                UnityClientConnector.WorkerType,
                UnityGameLogicConnector.WorkerType,
                MobileClientWorkerConnector.WorkerType);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, serverAttribute);

            snapshot.AddEntity(template);
        }
    }
}
