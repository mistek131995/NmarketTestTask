using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using NmarketTestTask.Models;
using System.IO;

namespace NmarketTestTask.Parsers
{
    public class ExcelParser : IParser
    {
        public IList<House> GetHouses(string path)
        {

            XLWorkbook workbook = new XLWorkbook(path);
            IXLWorksheet sheet = workbook.Worksheets.First();
            List<List<string>> houseArray = SplitAraay(sheet);
            List<House> houses = new List<House>();

            for (int i = 0; i < houseArray.Count; i++)
            {

                for (int j = houseArray[i].Count - 1; j >= 0; j--)
                {

                    if (!houseArray[i][j].Contains("Дом"))
                        continue;

                    House house = new House()
                    {
                        Name = houseArray[i][j],
                        Flats = GetFlats(houseArray[i])
                    };

                    houses.Add(house);

                }

            }

            Console.WriteLine($"Файл {Path.GetFileName(path)} успешно прочитан.");

            return houses;
        }



        /// <summary>
        /// Разделяет общий массив по домам
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private List<List<string>> SplitAraay(IXLWorksheet sheet)
        {
            //Получаем общий массив клеток
            //Выбираем клетки с данными
            //Получаем значение из этих клеток
            //Приводим все это к списку
            List<string> arrayValue = sheet.Cells(true)
                .Where(c => c.GetValue<string>() != string.Empty)
                .Select(c => c.GetValue<string>())
                .ToList();

            //Создаем массив массивов для сохранения разделения по домам
            List<List<string>> array = new List<List<string>>();

            //Счетчик домов для цикла
            int s = -1;

            //Разделяем общим массив по домам
            foreach (var value in arrayValue)
            {
                //Если встречается запись с частью "Дом", счетчик +1
                //Добавляем новый элемент списка с типом string
                if (value.Contains("Дом")){
                    s++;
                    array.Add(new List<string>());
                }

                //Добавляем значения в текущий дом
                array[s].Add(value);
            }

            return array;
        }
    

        private List<Flat> GetFlats(List<string> array)
        {
            List<Flat> flats = new List<Flat>();
            int addingCount = 0;

            foreach(string value in array)
            {
                if (value.Contains("Дом"))
                    continue;

                if (value.Contains("№"))
                {
                    flats.Add(new Flat());
                    flats[addingCount].Number = value;
                    addingCount++;
                }

            }

            addingCount = 0;

            foreach (string value in array)
            {
                if (value.Contains("Дом") || value.Contains("№"))
                    continue;


                flats[addingCount].Price = value;
                addingCount++;

            }

            return flats;
        }
    }
}
