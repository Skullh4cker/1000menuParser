using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using System.Text.Json;
using HtmlAgilityPack;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace TestParser
{
    class Program
    {
        static float counter = 0;
        //Область сканирования (от 1 до ITERATIONS_COUNT)
        static float ITERATIONS_COUNT = 5000;
        //Число потоков:
        static int THREADS_COUNT = 100;
        static string host = "https://1000.menu/cooking/";
        static char[] blacklistSymbols = new char[3] { '\t', '\n', '\r' };
        static string namePath = "//h1[@itemprop='name']";
        static string ingredientsListPath = "//div[@class='list-column no-shrink']";
        static string recipeIngredientPath = "//meta[@itemprop='recipeIngredient']";
        static string ingredientsNamePath = "//a[@class='name']";
        static string positiveRatingPath = "//a[@class='review-points px-1 py-1 inlbl va-m link-no-style font-small ok']";
        static string neutralRatingPath = "//a[@class='review-points px-1 py-1 inlbl va-m link-no-style font-small ']";
        static string negativeRatingPath = "//a[@class='review-points px-1 py-1 inlbl va-m link-no-style font-small err']";
        static string servingNumbersPath = "//input[@id='yield_num_input']";
        static string caloriesPath = "//span[@id='nutr_kcal']";
        static string ingredientsQuanityPath = "//span[@class='squant value']";
        static string ingredientsTypeSelectedPath = "//option[@value='1']";
        static string ingredientsTypeSelectedSecondPath = "//option[@value='5']";
        static string authorPath = "//span[@class='profile-thumbnail font-no-style mt-1']";
        //!!! ПОМЕНЯЙ АДРЕСА ВЫВОДА:
        static string filePath = "C:/Users/Skullhacker/Downloads/1000menu2.txt";
        static string filePath2 = "C:/Users/Skullhacker/Downloads/1000menu2.json";
        static List<Page> validPages = new List<Page>();
        static void Main(string[] args)
        {
            List<int> allLinks = new List<int>();
            for (int i = 1; i <= ITERATIONS_COUNT; i++)
            {
                allLinks.Add(i);
            }
            GetAllRequests(allLinks, THREADS_COUNT);
            validPages = validPages.OrderBy(x => int.Parse(x.Link.Substring(host.Length))).ToList();
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var page in validPages)
                {
                    writer.Write($"Name: {page.DishName}; Link: {page.Link}; Ingredients: ");
                    foreach (var ing in page.Ingredients)
                    {
                        if (ing.Quantity == 0)
                            writer.Write($"{ing.Name} ({ing.Type}), ");
                        else
                            writer.Write($"{ing.Name} ({ing.Quantity} {ing.Type}), ");
                    }
                    writer.WriteLine();
                }
            }
            var options1 = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            using (StreamWriter writer = new StreamWriter(filePath2))
            {
                foreach (var page in validPages)
                {
                    writer.WriteLine(JsonSerializer.Serialize(page, options1));
                }
            }
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
        static void GetAllRequests(List<int> pageIds, int numberOfThreads)
        {
            //Обычный:
            //foreach (var pageId in pageIds)
            //{
            //    OneRequest(pageId);
            //}

            //Потоки:
            Parallel.ForEach(pageIds, new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads }, (pageId) =>
            {
                PageRequest(pageId);
            });
        }
        static void PageRequest(int i)
        {

            string link = "";
            string dishName = "";
            int rating = 0;
            int servingNumbers = 0;
            int calories = 0;
            List<ExactIngredient> ingredients = new List<ExactIngredient>();
            string url = host + i;
            HtmlWeb site = new HtmlWeb();
            HtmlDocument document = new HtmlDocument();
            try
            {
                document = site.Load(url, "GET");
            }
            catch
            {
                document = site.Load(url, "GET");
            }
            if (CheckIfPageExist(document, authorPath))
            {
                link = url;
                dishName = FindInnerText(document, namePath)[0];
                ingredients = GetIngredients(document);
                try
                {
                    rating = int.Parse(FindInnerText(document, positiveRatingPath)[0]);
                }
                catch
                {
                    try
                    {
                        rating = int.Parse(FindInnerText(document, negativeRatingPath)[0]);
                    }
                    catch
                    {
                        rating = int.Parse(FindInnerText(document, neutralRatingPath)[0]);
                    }
                }
                servingNumbers = int.Parse(FindAttribute(document, servingNumbersPath, "value")[0]);
                calories = int.Parse(FindInnerText(document, caloriesPath)[0]);
                Page page = new Page(link, dishName, rating, servingNumbers, calories, ingredients);
                validPages.Add(page);
            }
            counter++;
            Console.WriteLine($"Прогресс: {counter}$ ({Math.Round((counter/ITERATIONS_COUNT)*100, 3)})%");
        }
        static List<ExactIngredient> GetIngredients(HtmlDocument doc)
        {
            var listNames = FindInnerText(doc, ingredientsNamePath);
            var listQuanity = FindInnerText(doc, ingredientsQuanityPath);
            var listUnitOfMeasurement = ParseMass(doc, listNames.Count);

            List<ExactIngredient> ingredients = new List<ExactIngredient>();
            for(int i = 0; i < listNames.Count; i++)
            {
                try
                {
                    if (float.TryParse(listQuanity[i].Replace(".", ","), out float quanity))
                        ingredients.Add(new ExactIngredient(listNames[i], listUnitOfMeasurement[i], quanity));
                    else
                        ingredients.Add(new ExactIngredient(listNames[i], listUnitOfMeasurement[i], 0));
                }
                catch
                {
                    Console.WriteLine($"Error");
                }
            }
            return ingredients;
        }
        static List<string> ParseMass(HtmlDocument doc, int count)
        {
            List<string> types = FindAttribute(doc, recipeIngredientPath, "content");
            if (types.Count == count)
            {
                for (int i = 0; i < types.Count; i++)
                {
                    types[i] = types[i].Split(" - ")[1];
                    if (char.IsDigit(types[i][0]))
                    {
                        types[i] = types[i].Substring(types[i].IndexOf(" "));
                    }
                    if (types[i].StartsWith(" "))
                        types[i] = types[i].Remove(0, 1);
                }
            }
            else
            {
                types = new List<string>();
                int j = 0;
                int counter = 0;
                for(int i = 0; i < doc.DocumentNode.SelectNodes(ingredientsListPath).Count; i++)
                {
                    if (doc.DocumentNode.SelectNodes(ingredientsListPath)[i].ChildNodes["meta"] != null)
                    {
                        var node = doc.DocumentNode.SelectNodes(recipeIngredientPath)[j];
                        types.Add(node.GetAttributeValue("content", string.Empty));
                        types[i] = types[i].Split(" - ")[1];
                        if (char.IsDigit(types[i][0]))
                        {
                            types[i] = types[i].Substring(types[i].IndexOf(" "));
                        }
                        if (types[i].StartsWith(" "))
                            types[i] = types[i].Remove(0, 1);
                        if (doc.DocumentNode.SelectNodes(ingredientsListPath)[i].ChildNodes[5].GetAttributeValue("class", string.Empty) == "type" || doc.DocumentNode.SelectNodes(ingredientsListPath)[i].ChildNodes[5].ChildNodes[0].GetAttributeValue("value", string.Empty) != "1")
                            counter++;
                        j++;
                    }
                    else
                    {
                        if (doc.DocumentNode.SelectNodes(ingredientsListPath)[i].ChildNodes[3].ChildNodes[0].GetAttributeValue("value", string.Empty) == "1")
                            types.Add(new string((doc.DocumentNode.SelectNodes(ingredientsTypeSelectedPath)[i - counter]).InnerText.Where(t => !blacklistSymbols.Contains(t)).ToArray()));
                        else
                            types.Add(new string((doc.DocumentNode.SelectNodes(ingredientsListPath)[i].ChildNodes[3].ChildNodes[0].InnerText).Where(t => !blacklistSymbols.Contains(t)).ToArray()));
                    }
                }
            }
            return types;
        }
        static List<string> FindInnerText(HtmlDocument doc, string path)
        {
            List<string> result = new List<string>();

            if (doc.DocumentNode.SelectNodes(path) != null)
            {
                foreach (var element in doc.DocumentNode.SelectNodes(path))
                {
                    result.Add(new string(element.InnerText.Where(t => !blacklistSymbols.Contains(t)).ToArray()));
                }
            }
            
            return result;
        }
        static List<string> FindAttribute(HtmlDocument doc, string path, string attribute)
        {
            List<string> result = new List<string>();

            if (doc.DocumentNode.SelectNodes(path) != null)
            {
                foreach (var element in doc.DocumentNode.SelectNodes(path))
                {
                    string content = element.GetAttributeValue(attribute, string.Empty);
                    result.Add(content);
                }
            }
            
            return result;
        }
        static bool CheckIfPageExist(HtmlDocument doc, string path)
        {
            try
            {
                return doc.DocumentNode.SelectNodes(path) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
