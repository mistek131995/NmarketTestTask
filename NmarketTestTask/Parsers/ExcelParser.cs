using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using NmarketTestTask.Models;
using System.IO;
using System.Threading;

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

                    houses.Add(new House()
                    {
                        Name = houseArray[i][j],
                        Flats = GetFlats(houseArray[i])
                    });

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
    


        /// <summary>
        /// Метод принимает общий список и возвращает заполненный список квартир
        /// </summary>
        /// <param name="array">Общий список объектов</param>
        /// <returns></returns>
        private List<Flat> GetFlats(List<string> array)
        {
            //Общий список квартир для заполнения
            List<Flat> flats = new List<Flat>();

            //Получаем номера квартир
            List<string> flatsNumbers = array.Where(x => x.Contains("№")).ToList();
            //Получаем цены квартир
            List<string> flatsPrice = array.Where(x => x.Contains("№") == false && x.Contains("Дом") == false).ToList();

            //Если колчиество квартир и цен не совпадает, выводим предупреждение
            if (flatsNumbers.Count != flatsPrice.Count)
            {
                Console.WriteLine($"В {array[0]} не совпадает количество значений цены и квартир. Ячеек цены {flatsPrice.Count}, ячеек квартир {flatsNumbers.Count}.", ConsoleColor.Red);
                Console.WriteLine("Высока вероятность того, что данные заполняться некорректно или приложение завершиться с ошибкой.\n Нажмите ввод, чтобы продолжить.", ConsoleColor.Red);
                Console.ReadLine();
            }

            //Заполняем список с квартирами
            for(int i = 0; i < flatsNumbers.Count; i++)
            {
                flats.Add(new Flat()
                {
                    Number = flatsNumbers[i],
                    Price = flatsPrice[i]
                });
            }

            return flats;
        }
    }
}
