using APIDataSyncXMLGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Serilog;
using Microsoft.Extensions.Primitives;

namespace APIDataSyncXMLGenerator
{
    public static class FileService
    {
        public static List<string> ReadFile(string filePath)
        {
            List<string> codes = new List<string>();

            try
            {
                foreach (var line in File.ReadLines(filePath))
                {
                    codes.Add(line);
                }
                Log.Information("Successfully read file: {FileName}", Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading file: {FilePath}", filePath);
            }

            return codes;
        }

        public static void MakeXMLFile(string inputFilePath, List<Product> products)
        {
            try
            {
                string resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "result");
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string resultFileName = $"{fileNameWithoutExtension}_{timestamp}.xml";
                string resultFilePath = Path.Combine(resultPath, resultFileName);

                if (!Directory.Exists(resultPath))
                {
                    Directory.CreateDirectory(resultPath);
                }

                if (products.Any())
                {
                    XmlWriterSettings settings = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 };

                    using (XmlWriter writer = XmlWriter.Create(resultFilePath, settings))
                    {
                        writer.WriteStartElement("Products");
                        foreach (var product in products)
                        {
                            writer.WriteStartElement("Product");

                            WriteRawElement(writer, "Id", product.Id.ToString());
                            WriteRawElement(writer, "CodeGaska", product.CodeGaska);
                            WriteRawElement(writer, "Name", product.Name);
                            WriteRawElement(writer, "Supplier", product.Supplier);

                            string imageUrls = string.Join(",", product.Images?.Select(img => img.Url) ?? new List<string>());
                            WriteRawElement(writer, "ImageUrls", imageUrls);

                            if (product.CrossNumbers != null && product.CrossNumbers.Any())
                            {
                                var crossNumbers = string.Join(", ", product.CrossNumbers
                                    .Where(cn => cn?.CrossNumber != null) // Ensure cn and cn.CrossNumber are not null
                                    .SelectMany(cn => cn.CrossNumber.Split(",")
                                        .Select(number => number.Trim())));

                                WriteRawElement(writer, "CrossNumbers", crossNumbers);
                            }


                            // Prepare the HTML description
                            var descriptionBuilder = new StringBuilder();
                            if (product.Parameters != null && product.Parameters.Any())
                            {
                                descriptionBuilder.Append("<p><b>Parametry: </b>");
                                descriptionBuilder.Append(string.Join(", ", product.Parameters?.Select(param => $"{param.AttributeName} : {param.AttributeValue}") ?? new List<string>()));
                                descriptionBuilder.Append("</p>");
                            }

                            if (product.CrossNumbers != null && product.CrossNumbers.Any())
                            {
                                descriptionBuilder.Append("<p><b>Numery referencyjne: </b>");
                                descriptionBuilder.Append(string.Join(", ", product.CrossNumbers
                                    .Where(cn => cn?.CrossNumber != null)
                                    .SelectMany(cn => cn.CrossNumber.Split(",")
                                        .Select(number => number.Trim()))));
                                descriptionBuilder.Append("</p>");
                            }

                            if (product.Applications != null && product.Applications.Any())
                            {
                                descriptionBuilder.Append("<p><b>Zastosowanie: </b>");
                                descriptionBuilder.Append(string.Join(", ", product.Applications.Select(app => app.Name)));
                                descriptionBuilder.Append("</p>");
                            }

                            WriteRawElement(writer, "ProductHTMLDescription", descriptionBuilder.ToString());

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement(); // Close Products
                    }

                    Log.Information("Made XML file. Check /result folder");
                    ArchiveFile(inputFilePath);
                }
                else
                {
                    Log.Error("Serialization failed. No products to save.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error making result file from file {Path.GetFileName(inputFilePath)} {Environment.NewLine}{ex}");
            }
        }

        private static void WriteRawElement(XmlWriter writer, string elementName, string value)
        {
            string sanitizedValue = RemoveInvalidXmlChars(value);
            writer.WriteStartElement(elementName);
            writer.WriteRaw($"<![CDATA[{sanitizedValue}]]>");
            writer.WriteEndElement();
        }

        private static string RemoveInvalidXmlChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            return new string(text.Where(c =>
                c == '\t' || c == '\n' || c == '\r' ||
                (c >= 32 && c <= 0xD7FF) ||
                (c >= 0xE000 && c <= 0xFFFD) ||
                (c >= 0x10000 && c <= 0x10FFFF)
            ).ToArray());
        }


        private static void ArchiveFile(string filePath)
        {
            try
            {
                string archivePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "archive");

                if (File.Exists(filePath))
                {
                    if (!Directory.Exists(archivePath))
                    {
                        Directory.CreateDirectory(archivePath);
                        Log.Warning("Created missing directory: {ArchivePath}", archivePath);
                    }

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                    string extension = Path.GetExtension(filePath);

                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string newFileName = $"{fileNameWithoutExtension}_{timestamp}{extension}";
                    string newFilePath = Path.Combine(archivePath, newFileName);

                    File.Move(filePath, newFilePath);
                    Log.Information("Successfully archived file: {FileName}", Path.GetFileName(filePath));
                }
                else
                {
                    Log.Warning("File not found for archiving: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error archiving file {FileName}", Path.GetFileName(filePath));
            }
        }

        private static string WrapWithCData(string value)
        {
            return $"<![CDATA[{value}]]>";
        }
    }
}
