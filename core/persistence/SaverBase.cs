using System;
using System.Linq;
using Dapper;
using Godot;
using Microsoft.Data.Sqlite;

namespace Core.Persistence
{
    public abstract class SaverBase
    {
        private readonly string CREATE_TABLE_TEMPLATE = @"CREATE TABLE IF NOT EXISTS {0} (
                {1}
                PRIMARY KEY ({2})
            );";
        protected string CREATE_TABLE_COMMAND;

        private readonly string QUERY_TEMPLATE = @"SELECT * FROM {0} WHERE 
            {1}
        ";
        protected string QUERY_COMMAND;

        private static readonly string INSERT_TEMPLATE = @"INSERT OR REPLACE INTO {0} 
            ({1})
            VALUES
            ({2})
        ";
        protected string INSERT_COMMAND;

        private readonly string pathToDBFile;

        private void SetupTable()
        {
            RunWithConnection((connection) => connection.Execute(CREATE_TABLE_COMMAND));
        }

        private string GetCreateCommand()
        {
            string columns = "";

            for (int i = 0; i < GetFields().Length; i++)
            {
                columns += GetFields()[i] + " " + GetTypes()[i] + ",\n";
            }

            string keys = GetKeys().Join(", ");

            return string.Format(CREATE_TABLE_TEMPLATE, GetTableName(), columns, keys);
        }

        private string GetQueryCommand()
        {
            string conditions = "";

            bool first = true;
            foreach (var key in GetKeys())
            {
                if (!first)
                {
                    conditions += " and ";
                }

                conditions += key + " = @" + key;
                first = false;
            }

            return string.Format(QUERY_TEMPLATE, GetTableName(), conditions);
        }

        private string GetInsertCommand()
        {
            string fields = GetFields().Join(", ");

            string mapping = GetFields().Select((field) => "@" + field).ToArray().Join(", ");

            return string.Format(INSERT_TEMPLATE, GetTableName(), fields, mapping);
        }

        // TODO Always using new connection?
        protected void RunWithConnection(Action<SqliteConnection> action)
        {
            using var connection = new SqliteConnection($"DataSource={pathToDBFile}");
            action.Invoke(connection);
        }

        protected abstract string[] GetFields();
        protected abstract string[] GetTypes();
        protected abstract string[] GetKeys();
        protected abstract string GetTableName();

        public SaverBase(string pathToDBFile)
        {
            this.pathToDBFile = pathToDBFile;

            CREATE_TABLE_COMMAND = GetCreateCommand();
            QUERY_COMMAND = GetQueryCommand();
            INSERT_COMMAND = GetInsertCommand();

            SetupTable();
        }
    }
}