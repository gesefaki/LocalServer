using System.Data;
using Dapper;
using LocalServer.Server.Models;
using Microsoft.Data.Sqlite;

namespace LocalServer.Server.Repository;

public class FileRepository
{
    private readonly string _connectionString;

    public FileRepository(string connectionString = "Data Source=files.db")
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    private IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public void InitializeDatabase()
    {
        using var connection = CreateConnection();

        string sql = @"CREATE TABLE IF NOT EXISTS Files(
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            filename TEXT NOT NULL,
            content_type TEXT NOT NULL,
            method TEXT NOT NULL,
            size INT64 NOT NULL,
            save_path TEXT NOT NULL
        )";

        connection.Execute(sql);
    }

    public async Task<int> SaveFileAsync(FileModel fileModel)
    {
        using var connection = CreateConnection();
        string sql = @"
        INSERT INTO Files (filename, content_type, method, size, save_path)
        VALUES (@FileName, @ContentType, @Method, @Size, @SavePath)
        SELECT last_insert_rowid()";

        return await connection.ExecuteScalarAsync<int>(sql, fileModel);
    }

    public async Task<IEnumerable<FileModel>> GetAllAsync()
    {
        using var connection = CreateConnection();
        string sql = @"SELECT * FROM Files";
        return await connection.QueryAsync<FileModel>(sql);
    }

    public async Task<BlobInfo?> GetBlobInfoAsync(int id)
    {
        using var connection = CreateConnection();
        string sql = @"SELECT content_type, save_path FROM Files WHERE @id = id";

        return await connection.QueryFirstOrDefaultAsync<BlobInfo>(sql, new { @id = id });
    }

    public async Task<FileModel?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        string sql = @"SELECT * FROM Files WHERE @id = id LIMIT 1";

        return await connection.QueryFirstOrDefaultAsync<FileModel>(sql, new { @id = id });
    }

    public async Task<bool> TryDeleteAsync(int id)
    {
        using var connection = CreateConnection();

        string sql = @"DELETE FROM Files WHERE @id = id";

        var affectedRows = await connection.ExecuteAsync(sql, new { @id = id });
        return affectedRows > 0;
    }

}