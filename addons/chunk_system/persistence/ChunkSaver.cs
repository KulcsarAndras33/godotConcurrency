using Microsoft.Data.Sqlite;
using Dapper;
using Godot;
using System;
using System.Linq;

namespace ChunkSystem.Persistence
{
    public class ChunkSaver
    {
        private static readonly string TABLE_NAME = "Chunks";
        private static readonly string CREATE_TABLE_COMMAND = @$"CREATE TABLE IF NOT EXISTS {TABLE_NAME} (
                X INTEGER,
                Y INTEGER,
                Z INTEGER,
                Data BLOB,
                PRIMARY KEY (X, Y, Z)
            );";

        private static readonly string QUERY_COMMAND = @$"SELECT * FROM {TABLE_NAME} WHERE 
            X = @X and Y = @Y and Z = @Z
        ";

        private static readonly string INSERT_COMMAND = @$"INSERT OR REPLACE INTO {TABLE_NAME}
            (X, Y, Z, Data)
            VALUES
            (@X, @Y, @Z, @Data)
        ";

        private readonly string pathToDBFile;

        private void SetupChunkTable()
        {
            RunWithConnection((connection) => connection.Execute(CREATE_TABLE_COMMAND));
        }

        private void RunWithConnection(Action<SqliteConnection> action)
        {
            using var connection = new SqliteConnection($"DataSource={pathToDBFile}");
            action.Invoke(connection);
        }

        public ChunkSaver(string pathToDBFile)
        {
            this.pathToDBFile = pathToDBFile;

            SetupChunkTable();
        }

        public void SaveChunk(Vector3I coords, int[,,] data)
        {
            GD.Print($"Saving chunk at {coords}");
            ChunkRecord record = ChunkRecord.FromChunk(coords, data);
            RunWithConnection((conn) => conn.Execute(INSERT_COMMAND, record));
        }

        public ChunkRecord LoadChunk(Vector3I coords)
        {
            GD.Print($"Load chunk at {coords}");
            ChunkRecord record = null;
            RunWithConnection((conn) =>
            record = conn.Query<ChunkRecord>(QUERY_COMMAND, new { X = coords.X, Y = coords.Y, Z = coords.Z })
            .First());

            return record;
        }
    }
}