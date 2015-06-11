using System;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace SURF
{
    public class NeuralNetwork
    {

        [XmlArray("Neurons")]
        public Neuron[] neurons;

        /**
         * Конструктор сети создает нейроны
         */
        public NeuralNetwork(int count)
        {
            neurons = new Neuron[count];

            for (int i = 0; i < neurons.Length; i++)
                neurons[i] = new Neuron();
        }

        /**
         * функция распознавания символа, используется для обучения
         * @param input - входной вектор
         * @return массив из нуллей и единиц, ответы нейронов
         */
        int[] handleHard(int[,] input)
        {
            int[] output = new int[neurons.Length];
            for (int i = 0; i < output.Length; i++)
                output[i] = neurons[i].transferHard(input);

            return output;
        }

        /**
         * функция распознавания символа, используется для конечново ответа
         * @param input -  входной вектор
         * @return массив из вероятностей, ответы нейронов
         */
        int[] handle(int[,] input)
        {
            int[] output = new int[neurons.Length];
            for (int i = 0; i < output.Length; i++)
                output[i] = neurons[i].transfer(input);

            return output;
        }

        /**
         * ответ сети
         * @param input - входной вектор
         * @return индекс нейронов предназначенный для конкретного символа
         */
        public int getAnswer(int[,] input)
        {
            int[] output = handle(input);
            int maxIndex = 0;
            for (int i = 1; i < output.Length; i++)
                if (output[i] > output[maxIndex])
                    maxIndex = i;

            return maxIndex;
        }

        /**
         * функция обучения
         * @param input - входной вектор
         * @param correctAnswer - правильный ответ
         */
        public void study(int[,] input, int correctAnswer)
        {
            int[] correctOutput = new int[neurons.Length];
            correctOutput[correctAnswer] = 1;

            int[] output = handleHard(input);
            while (!compareArrays(correctOutput, output))
            {
                for (int i = 0; i < neurons.Length; i++)
                {
                    int dif = correctOutput[i] - output[i];
                    neurons[i].changeWeights(input, dif);
                }
                output = handleHard(input);
            }
        }

        /**
         * сравнение двух вектор
         * @param true - если массивы одинаковые, false - если нет
         */
        bool compareArrays(int[] a, int[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;

            return true;
        }

        void prepareForSerialization()
        {
            foreach (Neuron n in neurons)
                n.prepareForSerialization();
        }

        void onDeserialize()
        {
            foreach (Neuron n in neurons)
                n.onDeserialize();
        }

        public void saveLocal(string fileName)
        {
            prepareForSerialization();

            XmlSerializer serializer = new XmlSerializer(this.GetType());
            using (Stream fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(fStream, this);
            }
        }

        public static NeuralNetwork fromXml(string fileName)
        {
            string xml = "";
            FileStream fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
            if (fStream.Length > 0)
            {
                byte[] tempData = new byte[fStream.Length];
                fStream.Read(tempData, 0, tempData.Length);

                xml = System.Text.Encoding.ASCII.GetString(tempData);
            }
            fStream.Close();

            if (string.IsNullOrEmpty(xml))
                throw new Exception("Файл пустой");


            NeuralNetwork data;

            XmlSerializer serializer = new XmlSerializer(typeof(NeuralNetwork));
            using (TextReader reader = new StringReader(xml))
            {
                data = serializer.Deserialize(reader) as NeuralNetwork;
            }

            data.onDeserialize();

            return data;
        }
    }
}
