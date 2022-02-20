using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using NmarketTestTask.Models;
using System.IO;
using System;

namespace NmarketTestTask.Parsers
{
    public class HtmlParser : IParser
    {
        private string HouseAttributeName;
        private string PriceAttributeName;
        private string NumberAttributeName;

        /// <summary>
        /// В класс передаем имена аттрибута класса столбцов
        /// </summary>
        /// <param name="houseAttributeName">Имя атрибута столбца дома</param>
        /// <param name="priceAttributeValue">Имя атрибута столбца цены</param>
        /// <param name="numberAttributeValue">Имя атрибута столбца номера квартиры</param>
        public HtmlParser(string houseAttributeName = "house", string priceAttributeValue = "price", string numberAttributeValue = "number")
        {
            HouseAttributeName = houseAttributeName;
            PriceAttributeName = priceAttributeValue;
            NumberAttributeName = numberAttributeValue;
        }


        /// <summary>
        /// Реальизация интерфейса, ну здесь и так все понятно...
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IList<House> GetHouses(string path)
        {
            //Загружаем документ
            var doc = new HtmlDocument();
            doc.Load(path);
            //Количество строк в таблице
            int columnCount = doc.DocumentNode.SelectNodes("//th").Count;
            //Количество стобцов в таблице -1 (Потому что мы не пишем заголовки)
            int rowCount = doc.DocumentNode.SelectNodes("//tr").Count - 1;
            //Получаем все элементы с тегом <td>
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(".//td");
            //Массив с готовыми домами
            List<House> houses = JoinRowsToHouse(nodes, columnCount, rowCount);

            Console.WriteLine($"Файл {Path.GetFileName(path)} успешно прочитан.");

            return houses;
        }


        /// <summary>
        /// Собираем из вложенного списка данных дома
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="columnCount"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        public List<House> JoinRowsToHouse(HtmlNodeCollection nodes, int columnCount, int rowCount)
        {
            //Вложенный список сгруппированый по домам
            List<List<HtmlNode>> houseRows = GetRowsFromHouses(nodes, columnCount, rowCount);
            //Список для заполнения
            List<House> houses = new List<House>();


            for (int i = 0; i < houseRows.Count; i++)
            {
                //Список с квартирами
                List<Flat> flats = new List<Flat>();
                //Выбираем из элемента вложенного списка все элементы с атрибутом number
                List<HtmlNode> flatNumber = houseRows[i].Where(x => x.Attributes["class"].Value == NumberAttributeName).ToList();
                //Выбираем из элемента вложенного списка все элементы с атрибутом price
                List<HtmlNode> flatPrice = houseRows[i].Where(x => x.Attributes["class"].Value == PriceAttributeName).ToList();

                //Цикл заполняет список квартир
                for (int j = 0; j < flatNumber.Count; j++)
                {
                    flats.Add(new Flat()
                    {
                        Number = flatNumber[j].InnerText,
                        Price = flatPrice[j].InnerText
                    });
                }

                //Добавляем дома и список квартир к нему
                houses.Add(new House
                {
                    Name = houseRows[i][0].InnerText,
                    Flats = flats
                });

            }

            return houses;
        }



        /// <summary>
        /// Метод соединяет строки в дома
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="columnCount"></param>
        /// <param name="rowCount"></param>
        /// <returns></returns>
        private List<List<HtmlNode>> GetRowsFromHouses(HtmlNodeCollection nodes, int columnCount, int rowCount)
        {

            List<List<HtmlNode>> rowHouse = GetSortNode(nodes, columnCount, rowCount);
            List<List<HtmlNode>> houseRows = new List<List<HtmlNode>>();

            //Перебираем элементы вложенного списка rowHouse
            for (int i = 0; i < rowHouse.Count; i++)
            {
                //Если массив пустой, добавляем первый дом
                if(houseRows.Count == 0)
                {
                    houseRows.Add(rowHouse[i]);
                    continue;
                }

                //Если дом уже существует, объединяем значения и обновляем элемент списка
                //Если элемент не найден, добавляем новый
                if (ExistHome(rowHouse[i], houseRows, out int indexExistHouse))
                {

                    List<HtmlNode> concatHome = houseRows[indexExistHouse].Concat(rowHouse[i]).ToList();
                    houseRows[indexExistHouse] = concatHome;

                } else
                {
                    houseRows.Add(rowHouse[i]);
                }

            }

            return houseRows;

        }


        /// <summary>
        /// Метод группирует элементы таблицы построчно
        /// </summary>
        /// <param name="nodes">Массив с нодами</param>
        /// <param name="columnCount">Количество столбцов</param>
        /// <param name="rowCount">Количество строк</param>
        private List<List<HtmlNode>> GetSortNode(HtmlNodeCollection nodes, int columnCount, int rowCount)
        {
            //Массив где храняться ноды построчно
            List<List<HtmlNode>> htmlNodes = new List<List<HtmlNode>>();
            
            //Шаг пропуска для групировки элементов
            int skipingStep = 0;

            for (int i = 0; i < rowCount; i++)
            {
                htmlNodes.Add(new List<HtmlNode>(nodes
                    .Skip(skipingStep)
                    .Take(columnCount)
                    .Where(x=> x.Attributes["class"].Value == HouseAttributeName || x.Attributes["class"].Value == NumberAttributeName || x.Attributes["class"].Value == PriceAttributeName)
                    ));

                skipingStep += columnCount;
            }

            return htmlNodes;
        }



        /// <summary>
        /// Метод проверят существует ли дом в списке
        /// </summary>
        /// <param name="rowHouse">Строка с элементами дома</param>
        /// <param name="house">Новый список с сортироваными домами</param>
        /// <param name="indexExistHouse">Возвращаемый индекс</param>
        /// <returns></returns>
        private bool ExistHome(List<HtmlNode> rowHouse, List<List<HtmlNode>> house, out int indexExistHouse)
        {
            bool houseExist = false;
            indexExistHouse = 0;

            //Цикл перебирает массив с сформированными домами в поисках совпдений
            for (int j = 0; j < house.Count; j++)
            {
                //Если есть дом с таким же именем, получаем true
                houseExist = rowHouse[0].InnerText == house[j][0].InnerText;

                if (houseExist)
                {
                    indexExistHouse = house.IndexOf(house[j]);
                }
            }

            return houseExist;

        }

    }
}
