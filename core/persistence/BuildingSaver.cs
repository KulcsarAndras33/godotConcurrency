using System;
using System.Linq;
using Dapper;
using Godot;
using Microsoft.Data.Sqlite;

namespace Core.Persistence
{
    public class BuildingSaver
    {

        private static BuildingSaver Instance = null;
        private static readonly string[] FIELDS = ["CommunityId", "Id", "X", "Y", "Z", "DescriptorId", "BuiltLevel"];
        private static readonly string[] TYPES = ["INTEGER", "INTEGER", "INTEGER", "INTEGER", "INTEGER", "INTEGER", "INTEGER"];
        private static readonly string[] KEYS = ["CommunityId", "Id"];
        private static readonly string TABLE_NAME = "Buildings";
        private static readonly string CREATE_TABLE_TEMPLATE = "CREATE TABLE IF NOT EXISTS " + TABLE_NAME + @" (
                {0}
                PRIMARY KEY ({1})
            );";
        private static string CREATE_TABLE_COMMAND;

        private static readonly string QUERY_TEMPLATE = "SELECT * FROM " + TABLE_NAME + @" WHERE 
            {0}
        ";
        private static string QUERY_COMMAND;

        private static readonly string INSERT_TEMPLATE = "INSERT OR REPLACE INTO " + TABLE_NAME + @" 
            ({0})
            VALUES
            ({1})
        ";
        private static string INSERT_COMMAND;

        private readonly string pathToDBFile;

        private void SetupBuildingTable()
        {
            RunWithConnection((connection) => connection.Execute(CREATE_TABLE_COMMAND));
        }

        private static string GetCreateCommand()
        {
            string columns = "";

            for (int i = 0; i < FIELDS.Length; i++)
            {
                columns += FIELDS[i] + " " + TYPES[i] + ",\n";
            }

            string keys = KEYS.Join(", ");

            return string.Format(CREATE_TABLE_TEMPLATE, columns, keys);
        }

        private static string GetQueryCommand()
        {
            string conditions = "";

            bool first = true;
            foreach (var key in KEYS)
            {
                if (!first)
                {
                    conditions += " and ";
                }

                conditions += key + " = @" + key;
                first = false;
            }

            return string.Format(QUERY_TEMPLATE, conditions);
        }

        private static string GetInsertCommand()
        {
            string fields = FIELDS.Join(", ");

            string mapping = FIELDS.Select((field) => "@" + field).ToArray().Join(", ");

            return string.Format(INSERT_TEMPLATE, fields, mapping);
        }

        private void RunWithConnection(Action<SqliteConnection> action)
        {
            using var connection = new SqliteConnection($"DataSource={pathToDBFile}");
            action.Invoke(connection);
        }


        public static BuildingSaver GetInstance()
        {
            // TODO FIXME Magic string
            Instance ??= new BuildingSaver("testDB");

            return Instance;
        }

        public BuildingSaver(string pathToDBFile)
        {
            this.pathToDBFile = pathToDBFile;

            CREATE_TABLE_COMMAND = GetCreateCommand();
            QUERY_COMMAND = GetQueryCommand();
            INSERT_COMMAND = GetInsertCommand();

            SetupBuildingTable();
        }

        public void SaveBuilding(int communityId, int id, Vector3I coords, int descriptorId, int builtLevel)
        {
            BuildingRecord record = new()
            {
                CommunityId = communityId,
                Id = id,
                X = coords.X,
                Y = coords.Y,
                Z = coords.Z,
                DescriptorId = descriptorId,
                BuiltLevel = builtLevel
            };
            RunWithConnection((conn) => conn.Execute(INSERT_COMMAND, record));
        }

        public BuildingRecord LoadBuilding(int communityId, int id)
        {
            BuildingRecord record = null;
            RunWithConnection((conn) =>
            record = conn.Query<BuildingRecord>(QUERY_COMMAND, new { CommunityId = communityId, Id = id })
            .First());

            return record;
        }
    }
}