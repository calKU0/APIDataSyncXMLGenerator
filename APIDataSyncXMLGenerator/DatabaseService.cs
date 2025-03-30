using APIDataSyncXMLGenerator.Models;
using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace APIDataSyncXMLGenerator
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _tableProducts;
        private readonly string _tableApplications;
        private readonly string _tableCrossNumbers;
        private readonly string _tableFiles;
        private readonly string _tableImages;
        private readonly string _tablePackages;
        private readonly string _tableParameters;

        public DatabaseService()
        {
            _connectionString = $"server={ConfigurationManager.AppSettings["DbIp"]};database={ConfigurationManager.AppSettings["DbName"]};user={ConfigurationManager.AppSettings["DbUsername"]};password={ConfigurationManager.AppSettings["DbPassword"]}";
            _tableProducts = ConfigurationManager.AppSettings["TableProducts"];
            _tableApplications = ConfigurationManager.AppSettings["TableApplications"];
            _tableCrossNumbers = ConfigurationManager.AppSettings["TableCrossNumbers"];
            _tableFiles = ConfigurationManager.AppSettings["TableFiles"];
            _tableImages = ConfigurationManager.AppSettings["TableImages"];
            _tablePackages = ConfigurationManager.AppSettings["TablePackages"];
            _tableParameters = ConfigurationManager.AppSettings["TableParameters"];
        }

        public async Task<int> GetIdFromName(string name)
        {
            int id = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = $"SELECT Id FROM {_tableProducts} WHERE codeGaska = @codeGaska";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@codeGaska", name);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && int.TryParse(result.ToString(), out int parsedId))
                        {
                            id = parsedId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error while retrieving ID for product code {name}");
            }
            return id;
        }

        public async Task<bool> InsertOrUpdateProductDetails(int productId, Product product)
        {

            int tablesUpdated = 0;
            tablesUpdated += await InsertOrUpdateApplications(productId, product.Applications);
            tablesUpdated += await InsertOrUpdateCrossNumbers(productId, product.CrossNumbers);
            tablesUpdated += await InsertOrUpdatePackages(productId, product.Packages);
            tablesUpdated += await InsertOrUpdateParameters(productId, product.Parameters);
            tablesUpdated += await InsertOrUpdateImages(productId, product.Images);
            tablesUpdated += await InsertOrUpdateFiles(productId, product.Files);

            bool success = tablesUpdated >= 6;

            return success;
        }

        public async Task<int> InsertOrUpdateApplications(int productId, List<ProductApplication>? applications)
        {
            if (applications == null || applications.Count == 0)
            {
                return 1;
            }

            int result = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    foreach (var app in applications.Where(a => !string.IsNullOrWhiteSpace(a.Name)))
                    {
                        string query = $@"INSERT INTO {_tableApplications} (Id, ParentID, Name)
                                         VALUES (@Id, @ParentID, @Name)
                                         ON DUPLICATE KEY UPDATE Name = @Name";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Id", app.Id);
                            command.Parameters.AddWithValue("@ParentID", productId);
                            command.Parameters.AddWithValue("@Name", app.Name);
                            result = await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating applications for Product ID {productId}");
            }
            return result;
        }
        public async Task<int> InsertOrUpdateCrossNumbers(int productId, List<ProductCrossNumber>? crossNumbers)
        {
            int result = 0;
            try
            {
                if (crossNumbers == null || crossNumbers.Count == 0) return 1;

                var validCrossNumbers = crossNumbers.Where(cn => !string.IsNullOrWhiteSpace(cn.CrossNumber)).ToList();
                if (validCrossNumbers.Count == 0) return 1;

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    foreach (var crossNumber in validCrossNumbers)
                    {
                        foreach (var splittedCN in crossNumber.CrossNumber.Split(","))
                        {
                            string query = $@"INSERT INTO {_tableCrossNumbers} (ProductId, CrossNumberValue, CrossManufacturer)
                                         VALUES (@ProductId, @CrossNumberValue, @CrossManufacturer)
                                         ON DUPLICATE KEY UPDATE CrossManufacturer = @CrossManufacturer";
                            using (var command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@ProductId", productId);
                                command.Parameters.AddWithValue("@CrossNumberValue", splittedCN);
                                command.Parameters.AddWithValue("@CrossManufacturer", crossNumber.CrossManufacturer);
                                int queryResult = await command.ExecuteNonQueryAsync();
                                if (queryResult == 2) queryResult = 1;
                                result = Math.Max(result, queryResult);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating cross numbers for Product ID {productId}");
            }
            return result;
        }

        public async Task<int> InsertOrUpdateFiles(int productId, List<ProductFile>? files)
        {
            if (files == null || files.Count == 0) return 1;

            int result = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    foreach (var file in files.Where(f => !string.IsNullOrWhiteSpace(f.Title) && !string.IsNullOrWhiteSpace(f.Url)))
                    {
                        string query = $@"INSERT INTO {_tableFiles} (ProductId, Title, Url)
                                         VALUES (@ProductId, @Title, @Url)
                                         ON DUPLICATE KEY UPDATE Url = @Url";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ProductId", productId);
                            command.Parameters.AddWithValue("@Title", file.Title);
                            command.Parameters.AddWithValue("@Url", file.Url);
                            result = await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating files for Product ID {productId}");
            }
            return result;
        }

        public async Task<int> InsertOrUpdateImages(int productId, List<ProductImage>? images)
        {
            if (images == null || images.Count == 0) return 1;

            int result = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    foreach (var image in images.Where(i => !string.IsNullOrWhiteSpace(i.Title) && !string.IsNullOrWhiteSpace(i.Url)))
                    {
                        string query = $@"INSERT INTO {_tableImages} (ProductId, Title, Url)
                                         VALUES (@ProductId, @Title, @Url)
                                         ON DUPLICATE KEY UPDATE Url = @Url";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ProductId", productId);
                            command.Parameters.AddWithValue("@Title", image.Title);
                            command.Parameters.AddWithValue("@Url", image.Url);
                            result = await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating images for Product ID {productId}");
            }
            return result;
        }

        public async Task<int> InsertOrUpdatePackages(int productId, List<ProductPackage>? packages)
        {
            int result = 0;
            try
            {
                if (packages == null || packages.Count == 0) return 1;

                var validPackages = packages.Where(f => !string.IsNullOrWhiteSpace(f.PackUnit)).ToList();
                if (validPackages.Count == 0) return 1;

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    foreach (var package in packages)
                    {
                        string query = $@"INSERT INTO {_tablePackages} (ProductId, PackUnit, PackQty, PackNettWeight, PackGrossWeight, PackEan, PackRequired)
                             VALUES (@ProductId, @PackUnit, @PackQty, @PackNettWeight, @PackGrossWeight, @PackEan, @PackRequired)
                             ON DUPLICATE KEY UPDATE 
                                PackUnit = @PackUnit,
                                PackQty = @PackQty,
                                PackNettWeight = @PackNettWeight,
                                PackGrossWeight = @PackGrossWeight,
                                PackRequired = @PackRequired";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ProductId", productId);
                            command.Parameters.AddWithValue("@PackUnit", package.PackUnit);
                            command.Parameters.AddWithValue("@PackQty", package.PackQty);
                            command.Parameters.AddWithValue("@PackNettWeight", package.PackNettWeight);
                            command.Parameters.AddWithValue("@PackGrossWeight", package.PackGrossWeight);
                            command.Parameters.AddWithValue("@PackEan", package.PackEan);
                            command.Parameters.AddWithValue("@PackRequired", package.PackRequired);
                            int queryResult = await command.ExecuteNonQueryAsync();

                            if (queryResult == 2) queryResult = 1;
                            result = queryResult;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating packages for Product ID {productId}");
            }
            return result;
        }

        public async Task<int> InsertOrUpdateParameters(int productId, List<ProductParameter>? parameters)
        {
            int result = 0;
            try
            {
                if (parameters == null || parameters.Count == 0) return 1;

                var validPackages = parameters.Where(f => !string.IsNullOrWhiteSpace(f.AttributeName)).ToList();
                if (validPackages.Count == 0) return 1;

                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    foreach (var param in parameters)
                    {
                        string query = $@"INSERT INTO {_tableParameters} (ProductId, AttributeId, AttributeName, AttributeValue)
                             VALUES (@ProductId, @AttributeId, @AttributeName, @AttributeValue)
                             ON DUPLICATE KEY UPDATE 
                                AttributeName = @AttributeName,
                                AttributeValue = @AttributeValue";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ProductId", productId);
                            command.Parameters.AddWithValue("@AttributeId", param.AttributeId);
                            command.Parameters.AddWithValue("@AttributeName", param.AttributeName);
                            command.Parameters.AddWithValue("@AttributeValue", param.AttributeValue);
                            int queryResult = await command.ExecuteNonQueryAsync();

                            if (queryResult == 2) queryResult = 1;
                            result = queryResult;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating parameters for Product ID {productId}");
            }
            return result;
        }
    }
}
