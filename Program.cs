// See https://aka.ms/new-console-template for more information

using System.Text;
///初始化設定------------------------------------------------------------------------////
string InFolderPath = $"./Import"; // 指定要搜索的資料夾路徑
//string filePath = "RIFF-WAVE Audio/sound.wav";
//string filePath = "RIFF-AIS/ais_v2_2.ais";
//string filePath = "RIFF-WAVE Audio/audio7.wav";
string filePath = "RIFF-WAVE IQ/iq_v2_1.wav";
//string filePath = "RIFF-AIS/ais_v2_1.ais";
//do{
    try
    {
        // 字串轉Byte
        // string hexString = "FF";
        // byte integerValue = Convert.ToByte(hexString, 16);
        // Console.Write($"{integerValue:X2} ");

        ///初始化設定------------------------------------------------------------------------////
        Init(InFolderPath);
        ////--------------------------------------------------------------------------------////

        //// 參數輸入，選擇資料來源檔名--------------------------------------------------////
        Console.WriteLine("All files under the ./Import directory.");
        // 取得Import資料夾裡面所有的檔案
        string[] files = Directory.GetFiles(InFolderPath, "*.*", SearchOption.AllDirectories);
        while (files.Length < 1)
        {
            Console.WriteLine("No files found in the ./Import directory. Would you like to search again?(y/s)");
            string respond = Console.ReadLine();
            switch (respond)
            {
                case "y":
                    files = Directory.GetFiles(InFolderPath, "*.*", SearchOption.AllDirectories);
                    break;
                case "n":
                    Environment.Exit(0);
                    break;
            }
        }
        int fileNo = 1;
        string FileName = "";
        foreach (string file in files) // 印出./Import底下所有檔案
        {
            Console.WriteLine($"    {fileNo}: {Path.GetFileName(file)}");
            fileNo++;
        }
        int FileNumber = 0; // 選擇第幾個檔案
        // int SearchIndex = -1;
        do
        {
            Console.Write("\nPlease select the file number:");
            FileNumber = int.Parse(Console.ReadLine());
            FileName = Path.GetFileName(files[FileNumber - 1]); // 取得選取的檔案檔名
            // SearchIndex = csvname.IndexOf(searchString); // 搜尋檔案副檔名是否為.csv
        } while (FileNumber < 1 && FileNumber > files.Length);
        
        ////--------------------------------------------------------------------------------////
        using (FileStream fs = new FileStream($"./Import/{FileName}", FileMode.Open))
        {
            BinaryReader reader = new BinaryReader(fs);
            // 檢查目前的讀取位置和流的總長度
            long currentPosition = fs.Position;
            long totalLength = fs.Length;
            //long fileSize = fs.Length;
            Console.WriteLine($"fileSize: {totalLength}");
            // byte[] packetData = reader.ReadBytes((int)fileSize);                                                    
            
            // RIFF
            string Chunk_ID_RIFF = Encoding.ASCII.GetString(reader.ReadBytes(4));
            int Chunk_Size = reader.ReadInt32();
            Console.WriteLine($"Chunk ID : {Chunk_ID_RIFF}");
            Console.WriteLine($"Chunk Size : {Chunk_Size}");
            string riff_Type = RIFF(reader,4);
            while (currentPosition < totalLength)
            {
                
                SwitchChunk(reader, riff_Type, 4); // fmt->IRIS->aux2->data
                currentPosition = fs.Position;
                // Console.WriteLine($"currentPosition: {currentPosition}");
            }
            // 读取剩余的数据包内容
            // byte[] packetData = reader.ReadBytes((int)fileSize); // 减去头部和整数所占的字节数

            // // 输出数据包的十六进制表示
            // Console.WriteLine("數據包內容（十六進制）：");
            // foreach (byte b in packetData)
            // {
            //     Console.Write($"{b:X2} ");
            // }

            fs.Close();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"發生錯誤: {ex.Message}");
    }
    Console.WriteLine("\n\nPress any key to close.");
    // 等待用戶輸入任意鍵
     Console.ReadKey();
//} while (true) ;

void Init(string InFolderPath)
{
    // 檢查資料夾是否存在
    if (!Directory.Exists(InFolderPath))
    {
        Directory.CreateDirectory(InFolderPath);
    }
}

void SwitchChunk(BinaryReader reader,string riff_Type, int length)
{
    string Chunk_ID = Encoding.ASCII.GetString(reader.ReadBytes(length));
    int Chunk_Size = reader.ReadInt32();
    Console.WriteLine();
    Console.WriteLine($"Chunk ID : {Chunk_ID}");
    Console.WriteLine($"Chunk Size : {Chunk_Size}");
    switch (Chunk_ID)
    {
        case "fmt ": 
            fmt(reader, riff_Type, Chunk_Size);
            break;
        case "iris":
            IRIS(reader, riff_Type, Chunk_Size);
            break;
        case "aux2":
            aux2(reader, riff_Type, Chunk_Size);
            break;
        case "data":
            data(reader, riff_Type, Chunk_Size);
            break;
    }
}

string RIFF (BinaryReader reader, int length){
    string riff_Type = Encoding.ASCII.GetString(reader.ReadBytes(length));
    Console.WriteLine($"riff Type: {riff_Type}");
    return riff_Type;
}

void fmt(BinaryReader reader, string riff_Type, int length)
{
    byte[] fmtData = reader.ReadBytes(length);
    switch (riff_Type)
    {
        case "AIS ":
            int AIS_Decode_Result_Format_Version = (int)fmtData[0];
            Console.WriteLine($"AIS Decode Result Format Version: {AIS_Decode_Result_Format_Version}");

            int Scan_Number_per_Record = (int)fmtData[1];
            Console.WriteLine($"Scan Number per Record: {Scan_Number_per_Record}");

            int Header_Size = (int)fmtData[2];
            Console.WriteLine($"Header Size: {Header_Size}");

            int AIS_Information_Size = (int)fmtData[3];
            Console.WriteLine($"AIS Information Size: {AIS_Information_Size}");
            break;
        case "WAVE":
            byte[] Format_Type_Byte = new byte[2];
            Array.Copy(fmtData, 0, Format_Type_Byte, 0, 2); // 0,1
            short Format_Type = BitConverter.ToInt16(Format_Type_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"Format Type: {Format_Type}");

            byte[] Channel_Count_Byte = new byte[2];
            Array.Copy(fmtData, 2, Channel_Count_Byte, 0, 2); // 2,3
            short Channel_Count = BitConverter.ToInt16(Channel_Count_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"Channel Count: {Channel_Count}");

            byte[] Sample_Rate_Byte = new byte[4];
            Array.Copy(fmtData, 4, Sample_Rate_Byte, 0, 4); // 4,5,6,7
            int Sample_Rate = BitConverter.ToInt32(Sample_Rate_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"Sample Rate: {Sample_Rate}");

            byte[] Bytes_Per_Second_Byte = new byte[4];
            Array.Copy(fmtData, 8, Bytes_Per_Second_Byte, 0, 4); // 8,9,10,11
            int Bytes_Per_Second = BitConverter.ToInt32(Bytes_Per_Second_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"Bytes Per Second: {Bytes_Per_Second}");

            byte[] Block_Alignment_Byte = new byte[2];
            Array.Copy(fmtData, 12, Block_Alignment_Byte, 0, 2); // 12,13
            short Block_Alignment = BitConverter.ToInt16(Block_Alignment_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"Block Alignment: {Block_Alignment}");

            byte[] Bits_Per_Sample_Byte = new byte[2];
            Array.Copy(fmtData, 14, Bits_Per_Sample_Byte, 0, 2); // 14,15
            short Bits_Per_Sample = BitConverter.ToInt16(Bits_Per_Sample_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"Bits Per Sample: {Block_Alignment}");
            break;
        case "SDR":
            int SD_Format_Version = (int)fmtData[0]; // 0
            Console.WriteLine($"SD Format Version: {SD_Format_Version}");

            int Scan_Number_per_Record_SDR = (int)fmtData[1]; // 1
            Console.WriteLine($"Scan Number per Record: {Scan_Number_per_Record_SDR}");

            int Header_Size_SDR = (int)fmtData[2]; // 2
            Console.WriteLine($"Header Size: {Header_Size_SDR}");

            int Detected_Signal_Information_Size = (int)fmtData[3]; // 3
            Console.WriteLine($"AIS Information Size: {Detected_Signal_Information_Size}");

            byte[] FFT_Length_Byte = new byte[2];
            Array.Copy(fmtData, 4, FFT_Length_Byte, 0, 2); // 4,5
            short FFT_Length = BitConverter.ToInt16(FFT_Length_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"FFT Length: {FFT_Length}");

            byte[] FFT_Scan_Bandwidth_Byte = new byte[4];
            Array.Copy(fmtData, 6, FFT_Scan_Bandwidth_Byte, 0, 4); // 6,7,8,9
            int FFT_Scan_Bandwidth = BitConverter.ToInt32(FFT_Scan_Bandwidth_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"FFT Scan Bandwidth: {FFT_Scan_Bandwidth}");

            byte[] FFT_Scale_Range_Byte = new byte[2];
            Array.Copy(fmtData, 10, FFT_Scale_Range_Byte, 0, 2); // 10,11
            short FFT_Scale_Range = BitConverter.ToInt16(FFT_Scale_Range_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"FFT Scale Range: {FFT_Length}");

            byte[] FFT_Block_Alignment_Byte = new byte[2];
            Array.Copy(fmtData, 12, FFT_Block_Alignment_Byte, 0, 2); // 12,13
            short FFT_Block_Alignment = BitConverter.ToInt16(FFT_Block_Alignment_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"FFT Block Alignment: {FFT_Block_Alignment}");

            break;
        case "ADSB":
            int ADSB_Decode_Result_Format_Version = (int)fmtData[0];
            Console.WriteLine($"ADS-B Decode Result Format Version: {ADSB_Decode_Result_Format_Version}");

            int ADSB_Scan_Number_per_Record = (int)fmtData[1];
            Console.WriteLine($"Scan Number per Record: {ADSB_Scan_Number_per_Record}");

            int ADSB_Header_Size = (int)fmtData[2];
            Console.WriteLine($"Header Size: {ADSB_Header_Size}");

            break;
    }

}

void IRIS(BinaryReader reader, string riff_Type, int length){
    byte[] IRISData = reader.ReadBytes(length);
    switch (riff_Type)
    {
        case "AIS ":
        case "WAVE":
        case "SDR":
        case "ADSB":
            int Task_General_Satellite_ID = (int)IRISData[0];
            switch (Task_General_Satellite_ID)
            {
                case 0:
                    Console.WriteLine($"Task General Satellite ID: {Task_General_Satellite_ID}->TASA Satellite");
                    break;
                case 1:
                    Console.WriteLine($"Task General Satellite ID: {Task_General_Satellite_ID}->IRIS-F1");
                    break;
                case 2:
                    Console.WriteLine($"Task General Satellite ID: {Task_General_Satellite_ID}->IRIS-F2");
                    break;
                case 3:
                    Console.WriteLine($"Task General Satellite ID: {Task_General_Satellite_ID}->IRIS-F3");
                    break;
            }

            int Task_General_Task_Name = (int)IRISData[1];
            switch (Task_General_Task_Name)
            {
                case 0:
                    Console.WriteLine($"Task General Task Name: {Task_General_Task_Name}->RF Sampling");
                    break;
                case 1:
                    Console.WriteLine($"Task General Task Name: {Task_General_Task_Name}->Signal Detection");
                    break;
                case 2:
                    Console.WriteLine($"Task General Task Name: {Task_General_Task_Name}->AIS Decode");
                    break;
                case 3:
                    Console.WriteLine($"Task General Task Name: {Task_General_Task_Name}->ADS-B Decode");
                    break;
                case 4:
                    Console.WriteLine($"Task General Task Name: {Task_General_Task_Name}->Audio Record");
                    break;
                case 5:
                    Console.WriteLine($"Task General Task Name: {Task_General_Task_Name}->RF Scanning");
                    break;
                case 6:
                    Console.WriteLine($"Task General Task Name: {Task_General_Task_Name}->AIS Debug");
                    break;
                case 7:
                    Console.WriteLine($"Task General Task Name: {Task_General_Task_Name}->ADS-B Debug");
                    break;
                case 8:
                    Console.WriteLine($"Task General Task Name: {Task_General_Task_Name}->Audio Debug");
                    break;
            }
            // 定義一個新的byte[]來存儲前4個byte
            byte[] Task_General_UNIX_Byte = new byte[4];
            Array.Copy(IRISData, 2, Task_General_UNIX_Byte, 0, 4); // 2,3,4,5
            long Task_General_UNIX_int = BitConverter.ToInt32(Task_General_UNIX_Byte, 0);// 將 byte[] 轉換為整數    
            Task_General_UNIX_int = Task_General_UNIX_int * 1000;                                                                               // 假設您要從 1970 年 1 月 1 日開始計算毫秒數
            DateTime Task_General_UNIX = new DateTime(1970, 1, 1).AddMilliseconds(Task_General_UNIX_int);
            Console.WriteLine($"UNIX(int): {Task_General_UNIX_int}");
            Console.WriteLine($"UNIX: {Task_General_UNIX}");

            // 定義一個新的byte[]來存儲前4個byte
            byte[] Task_General_Booking_Time_Byte = new byte[4];
            Array.Copy(IRISData, 6, Task_General_Booking_Time_Byte, 0, 4); // 6,7,8,9
            long Task_General_Booking_Time_int = BitConverter.ToInt32(Task_General_Booking_Time_Byte, 0);// 將 byte[] 轉換為整數
            Task_General_Booking_Time_int = Task_General_Booking_Time_int * 1000;
            DateTime Task_General_Booking_Time = new DateTime(1970, 1, 1).AddMilliseconds(Task_General_UNIX_int);
            Console.WriteLine($"Booking Time (UNIX): {Task_General_Booking_Time_int}");

            int Task_General_NS = (int)IRISData[10];
            Console.WriteLine($"NS: {Task_General_NS}s");
            // switch (Task_General_NS)
            // {
            //     case 0:
            //         Console.WriteLine($"NS: {Task_General_NS}->5s");
            //         break;
            //     case 1:
            //         Console.WriteLine($"Task General Task Name: {Task_General_NS}->10s");
            //         break;
            //     case 2:
            //         Console.WriteLine($"Task General Task Name: {Task_General_NS}->20s");
            //         break;
            //     case 3:
            //         Console.WriteLine($"Task General Task Name: {Task_General_NS}->40s");
            //         break;
            // }
            // 11,12,13,14
            int AD9361_antenna_source = (int)IRISData[15];
            Console.WriteLine($"AD9361 antenna source: {AD9361_antenna_source}");

            byte[] rx_rf_gain_Byte = new byte[4];
            Array.Copy(IRISData, 16, rx_rf_gain_Byte, 0, 4); // 16,17,18,19
            int rx_rf_gain = BitConverter.ToInt32(rx_rf_gain_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"rx_rf_gain: {rx_rf_gain}");

            byte[] rx_rf_bandwidth_Byte = new byte[4];
            Array.Copy(IRISData, 20, rx_rf_bandwidth_Byte, 0, 4); // 20,21,22,23
            int rx_rf_bandwidth = BitConverter.ToInt32(rx_rf_bandwidth_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"rx_rf_bandwidth: {rx_rf_bandwidth}");

            byte[] rx_sampling_freq_Byte = new byte[4];
            Array.Copy(IRISData, 24, rx_sampling_freq_Byte, 0, 4); // 24,25,26,27
            int rx_sampling_freq = BitConverter.ToInt32(rx_sampling_freq_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"rx_sampling_freq: {rx_sampling_freq}");

            byte[] rx_lo_freq_Byte = new byte[8];
            Array.Copy(IRISData, 28, rx_lo_freq_Byte, 0, 8); // 28,29,30,31,32,33,34,35
            long rx_lo_freq = BitConverter.ToInt64(rx_lo_freq_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"rx_lo_freq: {rx_lo_freq}");

            int rx_gain_control_mode = (int)IRISData[36];
            Console.WriteLine($"rx_gain_control_mode: {rx_gain_control_mode}");

            byte[] FPGA_FS_Byte = new byte[2];
            Array.Copy(IRISData, 37, FPGA_FS_Byte, 0, 2); // 37,38
            short FPGA_FS = BitConverter.ToInt16(FPGA_FS_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"FPGA_FS: {FPGA_FS}");
            // 39,40
            int RIFF_WAVE_Content = (int)IRISData[41]; //41
            Console.WriteLine($"RIFF-WAVE Content: {RIFF_WAVE_Content}");

            int Audio_Filter_BW = (int)IRISData[42]; //42
            Console.WriteLine($"Audio_Filter_BW: {Audio_Filter_BW}");

            byte[] Audio_Phase_Byte = new byte[2];
            Array.Copy(IRISData, 43, Audio_Phase_Byte, 0, 2); // 43,44
            short Audio_Phase = BitConverter.ToInt16(Audio_Phase_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"Audio_Phase: {Audio_Phase}");

            int SD_RIFF_SDR_Scan_Mode = (int)IRISData[45]; //45
            Console.WriteLine($"SD_RIFF-SDR_Scan_Mode: {SD_RIFF_SDR_Scan_Mode}");

            byte[] SD_LO_Low_Byte = new byte[2];
            Array.Copy(IRISData, 46, SD_LO_Low_Byte, 0, 2); // 46,47
            short SD_LO_Low = BitConverter.ToInt16(SD_LO_Low_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"SD_LO_Low: {SD_LO_Low}");

            byte[] SD_LO_High_Byte = new byte[2];
            Array.Copy(IRISData, 48, SD_LO_Low_Byte, 0, 2); // 48,49
            short SD_LO_High = BitConverter.ToInt16(SD_LO_High_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"SD_LO_High: {SD_LO_High}");

            byte[] SD_SNR_th_Byte = new byte[4];
            Array.Copy(IRISData, 50, rx_rf_gain_Byte, 0, 4); // 50,51,52,53
            int SD_SNR_th = BitConverter.ToInt32(SD_SNR_th_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"SD_SNR_th: {SD_SNR_th}");

            byte[] SD_Window_Size_Byte = new byte[2];
            Array.Copy(IRISData, 54, SD_Window_Size_Byte, 0, 2); // 54,55
            short SD_Window_Size = BitConverter.ToInt16(SD_Window_Size_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"SD_Window_Size: {SD_Window_Size}");

            byte[] SD_SNR_cov_th_Byte = new byte[4];
            Array.Copy(IRISData, 56, SD_SNR_cov_th_Byte, 0, 4); // 56,57,58,59
            int SD_SNR_cov_th = BitConverter.ToInt32(SD_SNR_cov_th_Byte, 0);// 將 byte[] 轉換為整數
            Console.WriteLine($"SD_SNR_cov_th: {SD_SNR_cov_th}");
            break;         
    }
}

void aux2(BinaryReader reader, string riff_Type, int length){
    byte[] aux2Data = reader.ReadBytes(length);
    switch (riff_Type)
    {
        case "AIS ":
        case "WAVE":
        case "SDR":
        case "ADSB":
            for (int i = 0; i < length / 72; i++)
            {
                int k = i * 72;
                int ARM_Temperature = (int)aux2Data[0 + k]; //0
                Console.WriteLine($"ARM Temperature: {ARM_Temperature}");

                int AD9361_Temperature = (int)aux2Data[1 + k]; //1
                Console.WriteLine($"AD9361 Temperature: {AD9361_Temperature}");

                byte[] PINT_Voltage_Byte = new byte[2];
                Array.Copy(aux2Data, 2 + k, PINT_Voltage_Byte, 0, 2); // 2,3
                short PINT_Voltage = BitConverter.ToInt16(PINT_Voltage_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"PINT Voltage: {PINT_Voltage}");

                byte[] PAVX_Voltage_Byte = new byte[2];
                Array.Copy(aux2Data, 4 + k, PAVX_Voltage_Byte, 0, 2); // 4,5
                short PAVX_Voltage = BitConverter.ToInt16(PAVX_Voltage_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"PAVX Voltage: {PAVX_Voltage}");

                byte[] PDDRO_Voltage_Byte = new byte[2];
                Array.Copy(aux2Data, 6 + k, PDDRO_Voltage_Byte, 0, 2); // 6,7
                short PDDRO_Voltage = BitConverter.ToInt16(PDDRO_Voltage_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"PDDRO Voltage: {PDDRO_Voltage}");
                //8,9
                int Fix_mode = (int)aux2Data[10 + k]; //10
                Console.WriteLine($"Fix mode: {Fix_mode}");

                int Number_of_SV_in_fix = (int)aux2Data[11 + k]; //11
                Console.WriteLine($"Number of SV in fix: {Number_of_SV_in_fix}");

                byte[] GNSS_week_Byte = new byte[2];
                Array.Copy(aux2Data, 12 + k, GNSS_week_Byte, 0, 2); // 12,13
                short GNSS_week = BitConverter.ToInt16(GNSS_week_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"GNSS week: {GNSS_week}");

                byte[] TOW_Byte = new byte[4];
                Array.Copy(aux2Data, 14 + k, TOW_Byte, 0, 4); // 14,15,16,17
                int TOW = BitConverter.ToInt32(TOW_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"TOW: {TOW}");

                byte[] Latitude_Byte = new byte[4];
                Array.Copy(aux2Data, 18 + k, Latitude_Byte, 0, 4); // 18,19,20,21
                int Latitude = BitConverter.ToInt32(Latitude_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"Latitude: {Latitude}");

                byte[] Longitude_Byte = new byte[4];
                Array.Copy(aux2Data, 22 + k, Longitude_Byte, 0, 4); // 22,23,24,25
                int Longitude = BitConverter.ToInt32(Longitude_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"Longitude: {Longitude}");

                byte[] Ellipsoid_altitude_Byte = new byte[4];
                Array.Copy(aux2Data, 26 + k, Ellipsoid_altitude_Byte, 0, 4); // 26,27,28,29
                int Ellipsoid_altitude = BitConverter.ToInt32(Ellipsoid_altitude_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"Ellipsoid altitude: {Ellipsoid_altitude}");

                byte[] Mean_sea_level_altitude_Byte = new byte[4];
                Array.Copy(aux2Data, 30 + k, Mean_sea_level_altitude_Byte, 0, 4); // 30,31,32,33
                int Mean_sea_level_altitude = BitConverter.ToInt32(Mean_sea_level_altitude_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"Mean sea level altitude: {Mean_sea_level_altitude}");

                byte[] GDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 34 + k, GDOP_Byte, 0, 2); // 34,35
                short GDOP = BitConverter.ToInt16(GDOP_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"GDOP: {GDOP}");

                byte[] PDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 36 + k, PDOP_Byte, 0, 2); // 36,37
                short PDOP = BitConverter.ToInt16(PDOP_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"PDOP: {PDOP}");

                byte[] HDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 38 + k, HDOP_Byte, 0, 2); // 38,39
                short HDOP = BitConverter.ToInt16(HDOP_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"HDOP: {HDOP}");

                byte[] VDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 40 + k, VDOP_Byte, 0, 2); // 40,41
                short VDOP = BitConverter.ToInt16(VDOP_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"VDOP: {VDOP}");

                byte[] TDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 42 + k, TDOP_Byte, 0, 2); // 42,43
                short TDOP = BitConverter.ToInt16(TDOP_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"TDOP: {TDOP}");

                byte[] ECEF_X_Byte = new byte[4];
                Array.Copy(aux2Data, 44 + k, ECEF_X_Byte, 0, 4); // 44,45,46,47
                int ECEF_X = BitConverter.ToInt32(ECEF_X_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"ECEF-X: {ECEF_X}");

                byte[] ECEF_Y_Byte = new byte[4];
                Array.Copy(aux2Data, 48 + k, ECEF_Y_Byte, 0, 4); // 48,49,50,51
                int ECEF_Y = BitConverter.ToInt32(ECEF_Y_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"ECEF-Y: {ECEF_Y}");

                byte[] ECEF_Z_Byte = new byte[4];
                Array.Copy(aux2Data, 52 + k, ECEF_Z_Byte, 0, 4); // 52,53,54,55
                int ECEF_Z = BitConverter.ToInt32(ECEF_Z_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"ECEF-Z: {ECEF_Z}");

                byte[] ECEF_VX_Byte = new byte[4];
                Array.Copy(aux2Data, 56 + k, ECEF_VX_Byte, 0, 4); // 56,57,58,59
                int ECEF_VX = BitConverter.ToInt32(ECEF_VX_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"ECEF-VX: {ECEF_VX}");

                byte[] ECEF_VY_Byte = new byte[4];
                Array.Copy(aux2Data, 60 + k, ECEF_VY_Byte, 0, 4); // 60,61,62,63
                int ECEF_VY = BitConverter.ToInt32(ECEF_VY_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"ECEF-VY: {ECEF_VY}");

                byte[] ECEF_VZ_Byte = new byte[4];
                Array.Copy(aux2Data, 64 + k, ECEF_VZ_Byte, 0, 4); // 64,65,66,67
                int ECEF_VZ = BitConverter.ToInt32(ECEF_VZ_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"ECEF-VZ: {ECEF_VZ}");

                int Navigation_data_check_sum = (int)aux2Data[68 + k]; //68
                Console.WriteLine($"Number of SV in fix: {Navigation_data_check_sum}");

                int Default_leap_seconds = (int)aux2Data[69 + k]; //69
                Console.WriteLine($"Default leap seconds: {Default_leap_seconds}");

                int Current_leap_seconds = (int)aux2Data[70 + k]; //70
                Console.WriteLine($"Current leap seconds: {Current_leap_seconds}");

                int Valid = (int)aux2Data[71 + k]; //71
                Console.WriteLine($"Valid: {Valid}");
                Console.WriteLine();
            }
            break;
    }
    
}

void data(BinaryReader reader, string riff_Type, int length){
    byte[] dataData = reader.ReadBytes(length);
    switch (riff_Type)
    {
        case "AIS ":
            for(int i = 0; i < length;i=i+16){
                byte[] type_Byte = new byte[4];
                Array.Copy(dataData, 0, type_Byte, 0, 4); // 0,1,2,3
                int type = BitConverter.ToInt32(type_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"type: {type}");

                byte[] mmsi_Byte = new byte[4];
                Array.Copy(dataData, 4, mmsi_Byte, 0, 4); // 4,5,6,7
                int mmsi = BitConverter.ToInt32(mmsi_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"mmsi: {mmsi}");

                byte[] lon_Byte = new byte[4];
                Array.Copy(dataData, 8, lon_Byte, 0, 4); // 8,9,10,11
                double lon = BitConverter.ToInt32(lon_Byte, 0)/600000.0;// 將 byte[] 轉換為整數
                Console.WriteLine($"lon: {lon}");

                byte[] lat_Byte = new byte[4];
                Array.Copy(dataData, 12, lat_Byte, 0, 4); // 12,13,14,15
                double lat = BitConverter.ToInt32(lat_Byte, 0)/600000.0;// 將 byte[] 轉換為整數
                Console.WriteLine($"lat: {lat}");
            }
            
            break;
        case "WAVE":
            Console.WriteLine("數據包內容（十六進制）：");
            // foreach (byte b in dataData)
            // {
            //     Console.Write($"{b:X2} ");
            // }
            break;
        case "SDR":
            break;
        case "ADSB":
            break;
    }
}

