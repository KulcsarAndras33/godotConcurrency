using Microsoft.Data.Sqlite;
using Dapper;

namespace ChunkSystem.Persistence
{
    public class ChunkSaver
    {
        private readonly string pathToDBFile;

        private void SetupChunkTable()
        {
            string command = @"CREATE TABLE IF NOT EXISTS Chunks (
                Coordinates BLOB PRIMARY KEY,
                Data BLOB
            );";

            using var connection = new SqliteConnection($"DataSource={pathToDBFile}");
            connection.Execute(command);
        }

        public ChunkSaver(string pathToDBFile)
        {
            this.pathToDBFile = pathToDBFile;

            SetupChunkTable();
        }
    }
}