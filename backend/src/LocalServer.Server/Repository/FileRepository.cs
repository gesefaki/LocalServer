using Dapper;
using LocalServer.Server.Models;
using Microsoft.Data.Sqlite;

namespace LocalServer.Server.Repository;

public class FileRepository : IFileRepository
{
    private readonly string _connectionString;

    public FileRepository(string connectionString = "Data Source=files.db")
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    private void InitializeDatabase()
    {
        using var connection = CreateConnection();

        string sql = @"CREATE TABLE IF NOT EXISTS Files(
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Filename TEXT NOT NULL,
            ContentType TEXT NOT NULL,
            Method TEXT NOT NULL,
            Size INT64 NOT NULL,
            SavePath TEXT NOT NULL
        )";

        connection.Execute(sql);
    }

    public async Task<int> SaveFileAsync(FileModel fileModel)
    {
        using var connection = CreateConnection();
        string sql = @"
        INSERT INTO Files (FileName, ContentType, Method, Size, SavePath)
        VALUES (@FileName, @ContentType, @Method, @Size, @SavePath);
        SELECT last_insert_rowid();";

        return await connection.ExecuteScalarAsync<int>(sql, fileModel);
    }

    public async Task<IEnumerable<FileModel>> GetAllAsync()
    {
        using var connection = CreateConnection();
        string sql = @"SELECT * FROM Files";
        return await connection.QueryAsync<FileModel>(sql);
    }

    public async Task<BlobInfo> GetBlobInfoAsync(int id)
    {
        using var connection = CreateConnection();
        string sql = @"SELECT ContentType, SavePath FROM Files WHERE id = @id";

        return await connection.QueryFirstOrDefaultAsync<BlobInfo>(sql, new { id }) ?? throw new ArgumentException($"Item with id {id} not found");
    }

    public async Task<FileModel?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        string sql = @"SELECT * FROM Files WHERE Id = @id LIMIT 1";

        return await connection.QueryFirstOrDefaultAsync<FileModel>(sql, new { id });
    }

    public async Task<bool> TryDeleteAsync(int id)
    {
        using var connection = CreateConnection();

        string sql = @"DELETE FROM Files WHERE Id = @id";

        var affectedRows = await connection.ExecuteAsync(sql, new { id });
        return affectedRows > 0;
    }

}