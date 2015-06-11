using System;
using System.Xml.Serialization;

namespace SURF
{
    public class Neuron
    {
        [XmlAttribute("weight")]
        public string data;

        [XmlIgnore]
        public int[,] weight; // веса нейронов

        [XmlIgnore]
        public int minimum = 50; // порог

        [XmlIgnore]
        public int row = 128, column = 128;

        /**
         * Конструктор нейрона, создает веса и устанавливает случайные значения
         */
        public Neuron()
        {
            weight = new int[row, column];
            randomizeWeights();
        }

        /**
         * ответы нейронов, жесткая пороговая
         * @param input - входной вектор
         * @return ответ 0 или 1
         */
        public int transferHard(int[,] input)
        {
            int Power = 0;
            for (int r = 0; r < row; r++)
                for (int c = 0; c < column; c++)
                    Power += weight[r, c] * input[r, c];

            //Debug.Log("Power: " + Power);
            return Power >= minimum ? 1 : 0;
        }

        /**
         * ответы нейронов с вероятностями
         * @param input - входной вектор
         * @return Power - вероятность
         */
        public int transfer(int[,] input)
        {
            int Power = 0;
            for (int r = 0; r < row; r++)
                for (int c = 0; c < column; c++)
                    Power += weight[r, c] * input[r, c];

            //Debug.Log("Power: " + Power);
            return Power;
        }

        /**
         * устанавливает начальные произвольные значения весам 
         */
        void randomizeWeights()
        {
            Random rand = new Random();
            for (int r = 0; r < row; r++)
                for (int c = 0; c < column; c++)
                    weight[r, c] = rand.Next(0,10);
        }

        /**
         * изменяет веса нейронов
         * @param input - входной вектор
         * @param d - разница между выходом нейрона и нужным выходом
         */
        public void changeWeights(int[,] input, int d)
        {
            for (int r = 0; r < row; r++)
                for (int c = 0; c < column; c++)
                    weight[r, c] += d * input[r, c];
        }

        public void prepareForSerialization()
        {
            data = "";
            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < column; c++)
                {
                    data += weight[r, c] + " ";
                }
                data += "\n";
            }
        }

        public void onDeserialize()
        {
            weight = new int[row, column];

            string[] rows = data.Split(new char[] { '\n' });
            for (int r = 0; r < row; r++)
            {
                string[] columns = rows[r].Split(new char[] { ' ' });
                for (int c = 0; c < column; c++)
                {
                    weight[r, c] = int.Parse(columns[c]);
                }
            }
        }
    }
}
