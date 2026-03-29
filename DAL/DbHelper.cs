using Microsoft.Data.SqlClient;
using ItemProcessingSystemCore.Models;

namespace ItemProcessingSystemCore.DAL
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection not found in appsettings.");
        }

        public SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        public List<Item> GetAllItems()
        {
            var items = new List<Item>();
            using var conn = OpenConnection();
            var cmd = new SqlCommand("SELECT ItemId, Name, Weight, CreatedAt FROM Items ORDER BY ItemId", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                items.Add(MapItem(reader));
            return items;
        }

        public Item? GetItemById(int itemId)
        {
            using var conn = OpenConnection();
            var cmd = new SqlCommand("SELECT ItemId, Name, Weight, CreatedAt FROM Items WHERE ItemId = @id", conn);
            cmd.Parameters.AddWithValue("@id", itemId);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapItem(reader) : null;
        }

        public void InsertItem(Item item)
        {
            using var conn = OpenConnection();
            var cmd = new SqlCommand("INSERT INTO Items (Name, Weight) VALUES (@name, @weight)", conn);
            cmd.Parameters.AddWithValue("@name", item.Name ?? "");
            cmd.Parameters.AddWithValue("@weight", item.Weight);
            cmd.ExecuteNonQuery();
        }

        public bool UpdateItem(Item item)
        {
            using var conn = OpenConnection();
            var cmd = new SqlCommand("UPDATE Items SET Name = @name, Weight = @weight WHERE ItemId = @id", conn);
            cmd.Parameters.AddWithValue("@id", item.ItemId);
            cmd.Parameters.AddWithValue("@name", item.Name ?? "");
            cmd.Parameters.AddWithValue("@weight", item.Weight);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteItem(int itemId)
        {
            using var conn = OpenConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                var delRel = new SqlCommand(
                    "DELETE FROM ItemRelations WHERE ParentItemId = @id OR ChildItemId = @id", conn, tx);
                delRel.Parameters.AddWithValue("@id", itemId);
                delRel.ExecuteNonQuery();

                var delItem = new SqlCommand("DELETE FROM Items WHERE ItemId = @id", conn, tx);
                delItem.Parameters.AddWithValue("@id", itemId);
                int rows = delItem.ExecuteNonQuery();

                if (rows == 0)
                {
                    tx.Rollback();
                    return false;
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<ItemRelation> GetAllRelations()
        {
            var relations = new List<ItemRelation>();
            using var conn = OpenConnection();
            var cmd = new SqlCommand("SELECT ParentItemId, ChildItemId FROM ItemRelations", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                relations.Add(new ItemRelation
                {
                    ParentItemId = (int)reader["ParentItemId"],
                    ChildItemId = (int)reader["ChildItemId"]
                });
            }
            return relations;
        }

        public bool RelationExists(int parentItemId, int childItemId)
        {
            using var conn = OpenConnection();
            var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM ItemRelations WHERE ParentItemId = @parent AND ChildItemId = @child", conn);
            cmd.Parameters.AddWithValue("@parent", parentItemId);
            cmd.Parameters.AddWithValue("@child", childItemId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public void InsertRelation(int parentItemId, int childItemId)
        {
            using var conn = OpenConnection();
            var cmd = new SqlCommand(
                "INSERT INTO ItemRelations (ParentItemId, ChildItemId) VALUES (@parent, @child)", conn);
            cmd.Parameters.AddWithValue("@parent", parentItemId);
            cmd.Parameters.AddWithValue("@child", childItemId);
            cmd.ExecuteNonQuery();
        }

        private static Item MapItem(SqlDataReader reader)
        {
            return new Item
            {
                ItemId    = (int)reader["ItemId"],
                Name      = reader["Name"] as string,
                Weight    = (double)reader["Weight"],
                CreatedAt = reader["CreatedAt"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["CreatedAt"]
            };
        }
    }
}