// See https://aka.ms/new-console-template for more information

using System.Text;

    string filePath = "ais1/ais1.ais";

       

        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BinaryReader reader = new BinaryReader(fs);
                long fileSize = fs.Length;
                // 读取文件头部信息
                string riffHeader = Encoding.ASCII.GetString(reader.ReadBytes(4));
                int data = reader.ReadInt32();

                Console.WriteLine($"RIFF 標頭: {riffHeader}");
                Console.WriteLine($"數據: {data}");
                Console.WriteLine($"fileSize: {fileSize}");
                // 读取剩余的数据包内容
                byte[] packetData = reader.ReadBytes((int)fileSize); // 减去头部和整数所占的字节数

                // 输出数据包的十六进制表示
                Console.WriteLine("數據包內容（十六進制）：");
                foreach (byte b in packetData)
                {
                    Console.Write($"{b:X2} ");
                }

                // 关闭文件流
                fs.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"發生錯誤: {ex.Message}");
        }
while(true);