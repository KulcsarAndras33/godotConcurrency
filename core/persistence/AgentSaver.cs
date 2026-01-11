using System.Linq;
using Dapper;
using Godot;

namespace Core.Persistence
{
    public class AgentSaver(string pathToDBFile) : SaverBase(pathToDBFile)
    {

        private static AgentSaver Instance = null;


        public static AgentSaver GetInstance()
        {
            // TODO FIXME Magic string
            Instance ??= new AgentSaver("testDB");

            return Instance;
        }

        public void SaveAgent(int communityId, int id, Vector3I coords, int descriptorId)
        {
            AgentRecord record = new()
            {
                CommunityId = communityId,
                Id = id,
                X = coords.X,
                Y = coords.Y,
                Z = coords.Z,
                DescriptorId = descriptorId,
            };
            RunWithConnection((conn) => conn.Execute(INSERT_COMMAND, record));
        }

        public AgentRecord LoadAgent(int communityId, int id)
        {
            AgentRecord record = null;
            RunWithConnection((conn) =>
            record = conn.Query<AgentRecord>(QUERY_COMMAND, new { CommunityId = communityId, Id = id })
            .First());

            return record;
        }

        protected override string[] GetFields()
        {
            return ["CommunityId", "Id", "X", "Y", "Z", "DescriptorId"];
        }

        protected override string[] GetTypes()
        {
            return ["INTEGER", "INTEGER", "INTEGER", "INTEGER", "INTEGER", "INTEGER"];
        }

        protected override string[] GetKeys()
        {
            return ["CommunityId", "Id"];
        }

        protected override string GetTableName()
        {
            return "Agents";
        }
    }
}