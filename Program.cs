// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using System.Text;
///初始化設定------------------------------------------------------------------------////
string InFolderPath = $"./Import"; // 指定要搜索的資料夾路徑
string OutFolderPath = $"./Export"; // 指定要搜索的資料夾路徑
int TaskNameIndex=0;
long rx_lo_freq=0;
int Task_General_NS =0;
string riff_Type="";
// CRC 表格
uint[] ModesChecksumTable = new uint[]
{
    0x3935ea, 0x1c9af5, 0xf1b77e, 0x78dbbf, 0xc397db, 0x9e31e9, 0xb0e2f0, 0x587178,
    0x2c38bc, 0x161c5e, 0x0b0e2f, 0xfa7d13, 0x82c48d, 0xbe9842, 0x5f4c21, 0xd05c14,
    0x682e0a, 0x341705, 0xe5f186, 0x72f8c3, 0xc68665, 0x9cb936, 0x4e5c9b, 0xd8d449,
    0x939020, 0x49c810, 0x24e408, 0x127204, 0x093902, 0x049c81, 0xfdb444, 0x7eda22,
    0x3f6d11, 0xe04c8c, 0x702646, 0x381323, 0xe3f395, 0x8e03ce, 0x4701e7, 0xdc7af7,
    0x91c77f, 0xb719bb, 0xa476d9, 0xadc168, 0x56e0b4, 0x2b705a, 0x15b82d, 0xf52612,
    0x7a9309, 0xc2b380, 0x6159c0, 0x30ace0, 0x185670, 0x0c2b38, 0x06159c, 0x030ace,
    0x018567, 0xff38b7, 0x80665f, 0xbfc92b, 0xa01e91, 0xaff54c, 0x57faa6, 0x2bfd53,
    0xea04ad, 0x8af852, 0x457c29, 0xdd4410, 0x6ea208, 0x375104, 0x1ba882, 0x0dd441,
    0xf91024, 0x7c8812, 0x3e4409, 0xe0d800, 0x706c00, 0x383600, 0x1c1b00, 0x0e0d80,
    0x0706c0, 0x038360, 0x01c1b0, 0x00e0d8, 0x00706c, 0x003836, 0x001c1b, 0xfff409,
    0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000,
    0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000,
    0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000, 0x000000
};


// try
{
    // 字串轉Byte
    // string hexString = "FF";
    // byte integerValue = Convert.ToByte(hexString, 16);
    // Console.Write($"{integerValue:X2} ");

    ///初始化設定------------------------------------------------------------------------////
    Init(InFolderPath,OutFolderPath);
    ////--------------------------------------------------------------------------------////

    //// 參數輸入，選擇資料來源檔名--------------------------------------------------////
    Console.WriteLine("All files under the ./Import directory.");
    // 取得Import資料夾裡面所有的檔案
    string[] files = Directory.GetFiles(InFolderPath, "*.*", SearchOption.AllDirectories);
    while (files.Length < 1)
    {
        Console.WriteLine("No files found in the ./Import ./Export directory. Would you like to search again?(y/s)");
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
    do
    {
        Console.Write("\nPlease select the file number:");
        FileNumber = int.Parse(Console.ReadLine());
        FileName = Path.GetFileName(files[FileNumber - 1]); // 取得選取的檔案檔名
    } while (FileNumber < 1 && FileNumber > files.Length);
        
    ////--------------------------------------------------------------------------------////
    using (FileStream fs = new FileStream($"./Import/{FileName}", FileMode.Open))
    {
       
        BinaryReader reader = new BinaryReader(fs);
        // 檢查目前的讀取位置和流的總長度
        // long currentPosition = fs.Position;
        long totalLength = fs.Length;
        Console.WriteLine($"fileSize: {totalLength}");
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(FileName);
            // 寫入檔案
            using (StreamWriter writer = new StreamWriter($"{OutFolderPath}/ParsedResult/{nameWithoutExtension}解析結果.txt"))
            {
                // RIFF
                string Chunk_ID_RIFF = Encoding.ASCII.GetString(reader.ReadBytes(4));
                int Chunk_Size = reader.ReadInt32();
                output($"Chunk ID : {Chunk_ID_RIFF}", writer);
                output($"Chunk Size : {Chunk_Size}", writer);
                riff_Type = RIFF(reader, 4, writer);
                SwitchChunk(reader, 4, writer, nameWithoutExtension,totalLength,FileName,fs); // fmt->IRIS->aux2->data
                // currentPosition = fs.Position;

                string Chunk_ID = "";
                while (Chunk_ID != "data") //currentPosition < totalLength ||
                {
                    Chunk_ID = SwitchChunk(reader, 4, writer, nameWithoutExtension,totalLength,FileName,fs); // fmt->IRIS->aux2->data
                    // currentPosition = fs.Position;
                }
            }
            fs.Close();
    }
}
// catch (Exception ex)
// {
//     Console.WriteLine($"發生錯誤: {ex.Message}");
// }
Console.WriteLine("\n\nPress any key to close.");
// 等待用戶輸入任意鍵
// Console.ReadLine();


void Init(string InFolderPath,string OutFolderPath)
{
        // 檢查資料夾是否存在
        if (!Directory.Exists(OutFolderPath))
        {
            Directory.CreateDirectory(OutFolderPath);
        }
        if (!Directory.Exists($"{OutFolderPath}/ParsedResult"))
        {
            Directory.CreateDirectory($"{OutFolderPath}/ParsedResult");
        }
        if (!Directory.Exists($"{OutFolderPath}/SDR"))
        {
            Directory.CreateDirectory($"{OutFolderPath}/SDR");
        }
        if (!Directory.Exists($"{OutFolderPath}/ADSB"))
        {
            Directory.CreateDirectory($"{OutFolderPath}/ADSB");
        }
        if (!Directory.Exists($"{OutFolderPath}/Audio"))
        {
            Directory.CreateDirectory($"{OutFolderPath}/Audio");
        }
        if (!Directory.Exists($"{OutFolderPath}/AIS"))
        {
            Directory.CreateDirectory($"{OutFolderPath}/AIS");
        }
        if (!Directory.Exists($"{OutFolderPath}/IQ"))
        {
            Directory.CreateDirectory($"{OutFolderPath}/IQ");
        }
        if (!Directory.Exists($"{OutFolderPath}/PulgIn"))
        {
            Directory.CreateDirectory($"{OutFolderPath}/PulgIn");
        }
}

string SwitchChunk(BinaryReader reader, int length,StreamWriter writer,string nameWithoutExtension,long totalLength,string FileName,FileStream fs)
{
    string Chunk_ID = Encoding.ASCII.GetString(reader.ReadBytes(length));
    int Chunk_Size = reader.ReadInt32();
    // Console.WriteLine();
    output($" ",writer);
    output($"Chunk ID : {Chunk_ID}",writer);
    output($"Chunk Size : {Chunk_Size}",writer);
    switch (Chunk_ID)
    {
        case "fmt ": 
            fmt(reader, Chunk_Size,writer);
            break;
        case "iris":
            IRIS(reader, Chunk_Size,writer);
            break;
        case "aux2":
            aux2(reader, Chunk_Size,writer);
            break;
        case "data":
            dataAsync(reader, Chunk_Size,writer,nameWithoutExtension,totalLength,FileName,fs);
            break;
    }
    return Chunk_ID;
}

string RIFF (BinaryReader reader, int length,StreamWriter writer){
    string riff_Type = Encoding.ASCII.GetString(reader.ReadBytes(length));
    output($"riff Type: {riff_Type}",writer);
    return riff_Type;
}

void fmt(BinaryReader reader, int length,StreamWriter writer)
{
    byte[] fmtData = reader.ReadBytes(length);
    switch (riff_Type)
    {
        case "AIS ":
            int AIS_Decode_Result_Format_Version = (int)fmtData[0];
            output($"AIS Decode Result Format Version: {AIS_Decode_Result_Format_Version}",writer);
            int Scan_Number_per_Record = (int)fmtData[1];
            output($"Scan Number per Record: {Scan_Number_per_Record}",writer);
            int Header_Size = (int)fmtData[2];
            output($"Header Size: {Header_Size}",writer);
            int AIS_Information_Size = (int)fmtData[3];
            output($"AIS Information Size: {AIS_Information_Size}",writer);
            break;
        case "WAVE":
            byte[] Format_Type_Byte = new byte[2];
            Array.Copy(fmtData, 0, Format_Type_Byte, 0, 2); // 0,1
            short Format_Type = BitConverter.ToInt16(Format_Type_Byte, 0);// 將 byte[] 轉換為整數
            output($"Format Type: {Format_Type}",writer);

            byte[] Channel_Count_Byte = new byte[2];
            Array.Copy(fmtData, 2, Channel_Count_Byte, 0, 2); // 2,3
            short Channel_Count = BitConverter.ToInt16(Channel_Count_Byte, 0);// 將 byte[] 轉換為整數          
            output($"Channel Count: {Channel_Count}",writer);

            byte[] Sample_Rate_Byte = new byte[4];
            Array.Copy(fmtData, 4, Sample_Rate_Byte, 0, 4); // 4,5,6,7
            int Sample_Rate = BitConverter.ToInt32(Sample_Rate_Byte, 0);// 將 byte[] 轉換為整數
            output($"Sample Rate: {Sample_Rate}",writer);

            byte[] Bytes_Per_Second_Byte = new byte[4];
            Array.Copy(fmtData, 8, Bytes_Per_Second_Byte, 0, 4); // 8,9,10,11
            int Bytes_Per_Second = BitConverter.ToInt32(Bytes_Per_Second_Byte, 0);// 將 byte[] 轉換為整數
            output($"Bytes Per Second: {Bytes_Per_Second}",writer);

            byte[] Block_Alignment_Byte = new byte[2];
            Array.Copy(fmtData, 12, Block_Alignment_Byte, 0, 2); // 12,13
            short Block_Alignment = BitConverter.ToInt16(Block_Alignment_Byte, 0);// 將 byte[] 轉換為整數
            output($"Block Alignment: {Block_Alignment}",writer);

            byte[] Bits_Per_Sample_Byte = new byte[2];
            Array.Copy(fmtData, 14, Bits_Per_Sample_Byte, 0, 2); // 14,15
            short Bits_Per_Sample = BitConverter.ToInt16(Bits_Per_Sample_Byte, 0);// 將 byte[] 轉換為整數
            output($"Bits Per Sample: {Bits_Per_Sample}",writer);

            break;
        case "SDR ":
            int SD_Format_Version = (int)fmtData[0]; // 0
            output($"SD Format Version: {SD_Format_Version}",writer);

            int Scan_Number_per_Record_SDR = (int)fmtData[1]; // 1
            output($"Scan Number per Record: {Scan_Number_per_Record_SDR}",writer);

            int Header_Size_SDR = (int)fmtData[2]; // 2
            output($"Header Size: {Header_Size_SDR}",writer);

            int Detected_Signal_Information_Size = (int)fmtData[3]; // 3
            output($"AIS Information Size: {Detected_Signal_Information_Size}",writer);

            byte[] FFT_Length_Byte = new byte[2];
            Array.Copy(fmtData, 4, FFT_Length_Byte, 0, 2); // 4,5
            short FFT_Length = BitConverter.ToInt16(FFT_Length_Byte, 0);// 將 byte[] 轉換為整數
            output($"FFT Length: {FFT_Length}",writer);

            byte[] FFT_Scan_Bandwidth_Byte = new byte[4];
            Array.Copy(fmtData, 6, FFT_Scan_Bandwidth_Byte, 0, 4); // 6,7,8,9
            int FFT_Scan_Bandwidth = BitConverter.ToInt32(FFT_Scan_Bandwidth_Byte, 0);// 將 byte[] 轉換為整數
            output($"FFT Scan Bandwidth: {FFT_Scan_Bandwidth}",writer);

            byte[] FFT_Scale_Range_Byte = new byte[2];
            Array.Copy(fmtData, 10, FFT_Scale_Range_Byte, 0, 2); // 10,11
            short FFT_Scale_Range = BitConverter.ToInt16(FFT_Scale_Range_Byte, 0);// 將 byte[] 轉換為整數
            output($"FFT Scale Range: {FFT_Length}",writer);

            byte[] FFT_Block_Alignment_Byte = new byte[2];
            Array.Copy(fmtData, 12, FFT_Block_Alignment_Byte, 0, 2); // 12,13
            short FFT_Block_Alignment = BitConverter.ToInt16(FFT_Block_Alignment_Byte, 0);// 將 byte[] 轉換為整數
            output($"FFT Block Alignment: {FFT_Block_Alignment}",writer);

            break;
        case "ADSB":
            int ADSB_Decode_Result_Format_Version = (int)fmtData[0];
            output($"ADS-B Decode Result Format Version: {ADSB_Decode_Result_Format_Version}",writer);

            int ADSB_Scan_Number_per_Record = (int)fmtData[1];
            output($"Scan Number per Record: {ADSB_Scan_Number_per_Record}",writer);

            int ADSB_Header_Size = (int)fmtData[2];
            output($"Header Size: {ADSB_Header_Size}",writer);

            break;
    }
}

void IRIS(BinaryReader reader, int length,StreamWriter writer){
    byte[] IRISData = reader.ReadBytes(length);
    switch (riff_Type)
    {
        case "AIS ":
        case "WAVE":
        case "SDR ":
        case "ADSB":
            int Task_General_Satellite_ID = (int)IRISData[0];
            switch (Task_General_Satellite_ID)
            {
                case 0:
                    output($"Task General Satellite ID: {Task_General_Satellite_ID}->TASA Satellite",writer);
                    break;
                case 1:
                    output($"Task General Satellite ID: {Task_General_Satellite_ID}->IRIS-F1",writer);
                    break;
                case 2:
                    output($"Task General Satellite ID: {Task_General_Satellite_ID}->IRIS-F2",writer);
                    break;
                case 3:
                    output($"Task General Satellite ID: {Task_General_Satellite_ID}->IRIS-F3",writer);
                    break;
            }

            int Task_General_Task_Name = (int)IRISData[1];
            TaskNameIndex=Task_General_Task_Name;
            switch (Task_General_Task_Name)
            {
                case 0:
                    output($"Task General Task Name: {Task_General_Task_Name}->RF Sampling",writer);
                    break;
                case 1:
                    output($"Task General Task Name: {Task_General_Task_Name}->Signal Detection",writer);
                    break;
                case 2:
                    output($"Task General Task Name: {Task_General_Task_Name}->AIS Decode",writer);
                    break;
                case 3:
                    output($"Task General Task Name: {Task_General_Task_Name}->ADS-B Decode",writer);
                    break;
                case 4:
                    output($"Task General Task Name: {Task_General_Task_Name}->Audio Record",writer);
                    break;
                case 5:
                    output($"Task General Task Name: {Task_General_Task_Name}->RF Scanning",writer);
                    break;
                case 6:
                    output($"Task General Task Name: {Task_General_Task_Name}->AIS Debug",writer);
                  
                    break;
                case 7:
                    output($"Task General Task Name: {Task_General_Task_Name}->ADS-B Debug",writer);
                    break;
                case 8:
                    output($"Task General Task Name: {Task_General_Task_Name}->Audio Debug",writer);
                    break;
            }
            // 定義一個新的byte[]來存儲前4個byte
            byte[] Task_General_UNIX_Byte = new byte[4];
            Array.Copy(IRISData, 2, Task_General_UNIX_Byte, 0, 4); // 2,3,4,5
            long Task_General_UNIX_int = BitConverter.ToInt32(Task_General_UNIX_Byte, 0);// 將 byte[] 轉換為整數    
            Task_General_UNIX_int = Task_General_UNIX_int * 1000;                                                                               // 假設您要從 1970 年 1 月 1 日開始計算毫秒數
            DateTime Task_General_UNIX = new DateTime(1970, 1, 1).AddMilliseconds(Task_General_UNIX_int);
            output($"UNIX(int): {Task_General_UNIX_int}",writer);
            output($"UNIX: {Task_General_UNIX}",writer);

            // 定義一個新的byte[]來存儲前4個byte
            byte[] Task_General_Booking_Time_Byte = new byte[4];
            Array.Copy(IRISData, 6, Task_General_Booking_Time_Byte, 0, 4); // 6,7,8,9
            long Task_General_Booking_Time_int = BitConverter.ToInt32(Task_General_Booking_Time_Byte, 0);// 將 byte[] 轉換為整數
            Task_General_Booking_Time_int = Task_General_Booking_Time_int * 1000;
            DateTime Task_General_Booking_Time = new DateTime(1970, 1, 1).AddMilliseconds(Task_General_UNIX_int);
            output($"Booking Time (UNIX): {Task_General_Booking_Time_int}",writer);

            Task_General_NS = (int)IRISData[10];
            output($"NS: {Task_General_NS}s",writer);
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
            output($"AD9361 antenna source: {AD9361_antenna_source}",writer);

            byte[] rx_rf_gain_Byte = new byte[4];
            Array.Copy(IRISData, 16, rx_rf_gain_Byte, 0, 4); // 16,17,18,19
            int rx_rf_gain = BitConverter.ToInt32(rx_rf_gain_Byte, 0);// 將 byte[] 轉換為整數
            output($"rx_rf_gain: {rx_rf_gain}",writer);

            byte[] rx_rf_bandwidth_Byte = new byte[4];
            Array.Copy(IRISData, 20, rx_rf_bandwidth_Byte, 0, 4); // 20,21,22,23
            int rx_rf_bandwidth = BitConverter.ToInt32(rx_rf_bandwidth_Byte, 0);// 將 byte[] 轉換為整數
            output($"rx_rf_bandwidth: {rx_rf_bandwidth}",writer);

            byte[] rx_sampling_freq_Byte = new byte[4];
            Array.Copy(IRISData, 24, rx_sampling_freq_Byte, 0, 4); // 24,25,26,27
            int rx_sampling_freq = BitConverter.ToInt32(rx_sampling_freq_Byte, 0);// 將 byte[] 轉換為整數
            output($"rx_sampling_freq: {rx_sampling_freq}",writer);

            byte[] rx_lo_freq_Byte = new byte[8];
            Array.Copy(IRISData, 28, rx_lo_freq_Byte, 0, 8); // 28,29,30,31,32,33,34,35
            rx_lo_freq = BitConverter.ToInt64(rx_lo_freq_Byte, 0);// 將 byte[] 轉換為整數
            output($"rx_lo_freq: {rx_lo_freq}",writer);

            int rx_gain_control_mode = (int)IRISData[36];
            output($"rx_gain_control_mode: {rx_gain_control_mode}",writer);

            byte[] FPGA_FS_Byte = new byte[2];
            Array.Copy(IRISData, 37, FPGA_FS_Byte, 0, 2); // 37,38
            short FPGA_FS = BitConverter.ToInt16(FPGA_FS_Byte, 0);// 將 byte[] 轉換為整數
            output($"FPGA_FS: {FPGA_FS}",writer);
            // 39,40
            int RIFF_WAVE_Content = (int)IRISData[41]; //41
            switch(RIFF_WAVE_Content){
                case 0:
                    output($"RIFF-WAVE Content: {RIFF_WAVE_Content}-->raw",writer);
                break;
                case 1:
                    output($"RIFF-WAVE Content: {RIFF_WAVE_Content}-->Debug",writer);                
                break;
                case 2:
                    output($"RIFF-WAVE Content: {RIFF_WAVE_Content}-->Audio",writer);
                break;
            }
            

            int Audio_Filter_Option = (int)IRISData[42]; //42
            
            switch(Audio_Filter_Option){
                case 0:
                    output($"Audio_Filter_Option: {Audio_Filter_Option}-->窄頻",writer);
                break;
                case 1:
                    output($"Audio_Filter_Option: {Audio_Filter_Option}-->寬頻",writer);
                break;
                case 2:
                    output($"Audio_Filter_Option: {Audio_Filter_Option}-->AM",writer);
                break;
            }
            byte[] Audio_Phase_Byte = new byte[2];
            Array.Copy(IRISData, 43, Audio_Phase_Byte, 0, 2); // 43,44
            short Audio_Phase = BitConverter.ToInt16(Audio_Phase_Byte, 0);// 將 byte[] 轉換為整數
            output($"Audio_Phase: {Audio_Phase}KHZ",writer);

            int SD_RIFF_SDR_Scan_Mode = (int)IRISData[45]; //45
            switch(Audio_Filter_Option){
                case 0:
                    output($"SD_RIFF-SDR_Scan_Mode: {SD_RIFF_SDR_Scan_Mode}-->Detect",writer);
                break;
                case 1:
                    output($"SD_RIFF-SDR_Scan_Mode: {SD_RIFF_SDR_Scan_Mode}-->Scan",writer);
                break;
            }
            
            byte[] SD_LO_Low_Byte = new byte[2];
            Array.Copy(IRISData, 46, SD_LO_Low_Byte, 0, 2); // 46,47
            short SD_LO_Low = BitConverter.ToInt16(SD_LO_Low_Byte, 0);// 將 byte[] 轉換為整數
            output($"SD_LO_Low: {SD_LO_Low}",writer);

            byte[] SD_LO_High_Byte = new byte[2];
            Array.Copy(IRISData, 48, SD_LO_Low_Byte, 0, 2); // 48,49
            short SD_LO_High = BitConverter.ToInt16(SD_LO_High_Byte, 0);// 將 byte[] 轉換為整數
            output($"SD_LO_High: {SD_LO_High}",writer);

            byte[] SD_SNR_th_Byte = new byte[4];
            Array.Copy(IRISData, 50, rx_rf_gain_Byte, 0, 4); // 50,51,52,53
            int SD_SNR_th = BitConverter.ToInt32(SD_SNR_th_Byte, 0);// 將 byte[] 轉換為整數
            output($"SD_SNR_th: {SD_SNR_th}",writer);

            byte[] SD_Window_Size_Byte = new byte[2];
            Array.Copy(IRISData, 54, SD_Window_Size_Byte, 0, 2); // 54,55
            short SD_Window_Size = BitConverter.ToInt16(SD_Window_Size_Byte, 0);// 將 byte[] 轉換為整數
            output($"SD_Window_Size: {SD_Window_Size}",writer);

            byte[] SD_SNR_cov_th_Byte = new byte[4];
            Array.Copy(IRISData, 56, SD_SNR_cov_th_Byte, 0, 4); // 56,57,58,59
            int SD_SNR_cov_th = BitConverter.ToInt32(SD_SNR_cov_th_Byte, 0);// 將 byte[] 轉換為整數
            output($"SD_SNR_cov_th: {SD_SNR_cov_th}",writer);
            break;         
    }
}

void aux2(BinaryReader reader, int length,StreamWriter writer){
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
                output($"ARM Temperature: {ARM_Temperature}",writer);

                int AD9361_Temperature = (int)aux2Data[1 + k]; //1
                output($"AD9361 Temperature: {AD9361_Temperature}",writer);

                byte[] PINT_Voltage_Byte = new byte[2];
                Array.Copy(aux2Data, 2 + k, PINT_Voltage_Byte, 0, 2); // 2,3
                short PINT_Voltage = BitConverter.ToInt16(PINT_Voltage_Byte, 0);// 將 byte[] 轉換為整數
                output($"PINT Voltage: {PINT_Voltage}",writer);

                byte[] PAVX_Voltage_Byte = new byte[2];
                Array.Copy(aux2Data, 4 + k, PAVX_Voltage_Byte, 0, 2); // 4,5
                short PAVX_Voltage = BitConverter.ToInt16(PAVX_Voltage_Byte, 0);// 將 byte[] 轉換為整數
                output($"PAVX Voltage: {PAVX_Voltage}",writer);

                byte[] PDDRO_Voltage_Byte = new byte[2];
                Array.Copy(aux2Data, 6 + k, PDDRO_Voltage_Byte, 0, 2); // 6,7
                short PDDRO_Voltage = BitConverter.ToInt16(PDDRO_Voltage_Byte, 0);// 將 byte[] 轉換為整數
                output($"PDDRO Voltage: {PDDRO_Voltage}",writer);
                //8,9
                int Fix_mode = (int)aux2Data[10 + k]; //10
                output($"Fix mode: {Fix_mode}",writer);

                int Number_of_SV_in_fix = (int)aux2Data[11 + k]; //11
                output($"Number of SV in fix: {Number_of_SV_in_fix}",writer);

                byte[] GNSS_week_Byte = new byte[2];
                Array.Copy(aux2Data, 12 + k, GNSS_week_Byte, 0, 2); // 12,13
                short GNSS_week = BitConverter.ToInt16(GNSS_week_Byte, 0);// 將 byte[] 轉換為整數
                output($"GNSS week: {GNSS_week}",writer);

                byte[] TOW_Byte = new byte[4];
                Array.Copy(aux2Data, 14 + k, TOW_Byte, 0, 4); // 14,15,16,17
                int TOW = BitConverter.ToInt32(TOW_Byte, 0);// 將 byte[] 轉換為整數
                output($"TOW: {TOW}",writer);

                byte[] Latitude_Byte = new byte[4];
                Array.Copy(aux2Data, 18 + k, Latitude_Byte, 0, 4); // 18,19,20,21
                int Latitude = BitConverter.ToInt32(Latitude_Byte, 0);// 將 byte[] 轉換為整數
                output($"Latitude: {Latitude}",writer);

                byte[] Longitude_Byte = new byte[4];
                Array.Copy(aux2Data, 22 + k, Longitude_Byte, 0, 4); // 22,23,24,25
                int Longitude = BitConverter.ToInt32(Longitude_Byte, 0);// 將 byte[] 轉換為整數
                output($"Longitude: {Longitude}",writer);

                byte[] Ellipsoid_altitude_Byte = new byte[4];
                Array.Copy(aux2Data, 26 + k, Ellipsoid_altitude_Byte, 0, 4); // 26,27,28,29
                int Ellipsoid_altitude = BitConverter.ToInt32(Ellipsoid_altitude_Byte, 0);// 將 byte[] 轉換為整數
                output($"Ellipsoid altitude: {Ellipsoid_altitude}",writer);

                byte[] Mean_sea_level_altitude_Byte = new byte[4];
                Array.Copy(aux2Data, 30 + k, Mean_sea_level_altitude_Byte, 0, 4); // 30,31,32,33
                int Mean_sea_level_altitude = BitConverter.ToInt32(Mean_sea_level_altitude_Byte, 0);// 將 byte[] 轉換為整數
                output($"Mean sea level altitude: {Mean_sea_level_altitude}",writer);

                byte[] GDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 34 + k, GDOP_Byte, 0, 2); // 34,35
                short GDOP = BitConverter.ToInt16(GDOP_Byte, 0);// 將 byte[] 轉換為整數
                output($"GDOP: {GDOP}",writer);

                byte[] PDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 36 + k, PDOP_Byte, 0, 2); // 36,37
                short PDOP = BitConverter.ToInt16(PDOP_Byte, 0);// 將 byte[] 轉換為整數
                output($"PDOP: {PDOP}",writer);

                byte[] HDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 38 + k, HDOP_Byte, 0, 2); // 38,39
                short HDOP = BitConverter.ToInt16(HDOP_Byte, 0);// 將 byte[] 轉換為整數
                output($"HDOP: {HDOP}",writer);

                byte[] VDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 40 + k, VDOP_Byte, 0, 2); // 40,41
                short VDOP = BitConverter.ToInt16(VDOP_Byte, 0);// 將 byte[] 轉換為整數
                output($"VDOP: {VDOP}",writer);

                byte[] TDOP_Byte = new byte[2];
                Array.Copy(aux2Data, 42 + k, TDOP_Byte, 0, 2); // 42,43
                short TDOP = BitConverter.ToInt16(TDOP_Byte, 0);// 將 byte[] 轉換為整數
                output($"TDOP: {TDOP}",writer);

                byte[] ECEF_X_Byte = new byte[4];
                Array.Copy(aux2Data, 44 + k, ECEF_X_Byte, 0, 4); // 44,45,46,47
                int ECEF_X = BitConverter.ToInt32(ECEF_X_Byte, 0);// 將 byte[] 轉換為整數
                output($"ECEF-X: {ECEF_X}",writer);

                byte[] ECEF_Y_Byte = new byte[4];
                Array.Copy(aux2Data, 48 + k, ECEF_Y_Byte, 0, 4); // 48,49,50,51
                int ECEF_Y = BitConverter.ToInt32(ECEF_Y_Byte, 0);// 將 byte[] 轉換為整數
                output($"ECEF-Y: {ECEF_Y}",writer);

                byte[] ECEF_Z_Byte = new byte[4];
                Array.Copy(aux2Data, 52 + k, ECEF_Z_Byte, 0, 4); // 52,53,54,55
                int ECEF_Z = BitConverter.ToInt32(ECEF_Z_Byte, 0);// 將 byte[] 轉換為整數
                output($"ECEF-Z: {ECEF_Z}",writer);

                byte[] ECEF_VX_Byte = new byte[4];
                Array.Copy(aux2Data, 56 + k, ECEF_VX_Byte, 0, 4); // 56,57,58,59
                int ECEF_VX = BitConverter.ToInt32(ECEF_VX_Byte, 0);// 將 byte[] 轉換為整數
                output($"ECEF-VX: {ECEF_VX}",writer);

                byte[] ECEF_VY_Byte = new byte[4];
                Array.Copy(aux2Data, 60 + k, ECEF_VY_Byte, 0, 4); // 60,61,62,63
                int ECEF_VY = BitConverter.ToInt32(ECEF_VY_Byte, 0);// 將 byte[] 轉換為整數
                output($"ECEF-VY: {ECEF_VY}",writer);

                byte[] ECEF_VZ_Byte = new byte[4];
                Array.Copy(aux2Data, 64 + k, ECEF_VZ_Byte, 0, 4); // 64,65,66,67
                int ECEF_VZ = BitConverter.ToInt32(ECEF_VZ_Byte, 0);// 將 byte[] 轉換為整數
                output($"ECEF-VZ: {ECEF_VZ}",writer);

                int Navigation_data_check_sum = (int)aux2Data[68 + k]; //68
                output($"Number of SV in fix: {Navigation_data_check_sum}",writer);

                int Default_leap_seconds = (int)aux2Data[69 + k]; //69
                output($"Default leap seconds: {Default_leap_seconds}",writer);

                int Current_leap_seconds = (int)aux2Data[70 + k]; //70
                output($"Current leap seconds: {Current_leap_seconds}",writer);

                int Valid = (int)aux2Data[71 + k]; //71
                output($"Valid: {Valid}",writer);
                output($" ",writer);
            }
            break;
    }
    
}

async Task dataAsync(BinaryReader reader, int length,StreamWriter writer,string nameWithoutExtension,long totalLength,string FileName,FileStream fs){
    byte[] dataData = reader.ReadBytes(length);
    if (totalLength>= 16777216){ // 解IQ資料
                fs.Close();
                // 執行 CMD 命令來執行 remove_16th_bit.exe
                string command = "cmd.exe";
                string arguments = $"/c cd \"./{OutFolderPath}/PulgIn\" && \"remove_16th_bit.exe\" \"../../Import/{FileName}\" \"../IQ/{nameWithoutExtension}_output.wav\"";
                ProcessStartInfo psi = new ProcessStartInfo(command, arguments);
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;

                Process process = new Process();
                process.StartInfo = psi;
                process.Start();
                await process.WaitForExitAsync(); // 使用非同步的 WaitForExit 方法等待命令執行完畢
                int exitCode = process.ExitCode;
                process.Close();
                // riff_Type="IQ";
    }else{
        switch (riff_Type)
        {
            case "AIS ":
                AIS(dataData,length,writer);
                break;
            case "WAVE":
                 output("數據包內容（十六進制）：",writer);
                break;
            case "SDR ":
                int count= length/528448;
                output($"共有{count}筆資料",writer);
                using (BinaryWriter outPutWriter = new BinaryWriter(File.Open($"{OutFolderPath}/SDR/{nameWithoutExtension}_output.txt", FileMode.Create)))
                {
                    switch(TaskNameIndex){
                        case 1: //定頻
                            SignalDetection(dataData,length,writer,outPutWriter);
                        break;
                        case 5: // 寬頻
                            RFScanning(dataData, count,writer,outPutWriter);
                        break;
                    }
                }
                break;
            case "ADSB":
                using (BinaryWriter outPutWriter = new BinaryWriter(File.Open($"{OutFolderPath}/ADSB/{nameWithoutExtension}_output.txt", FileMode.Create)))
                {
                    ADSB(dataData, length,writer,outPutWriter);
                }
                // fs.Close();
                // 執行 CMD 命令來執行 deADSB.exe
                string command = "cmd.exe";
                string arguments = $"/c cd \"./{OutFolderPath}/PulgIn\" && \"deADSB.exe\" --input ../ADSB/{nameWithoutExtension}_output.txt  --output ../ADSB/{nameWithoutExtension}_json";

                ProcessStartInfo psi = new ProcessStartInfo(command, arguments);
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;

                Process process = new Process();
                process.StartInfo = psi;
                process.Start();
                await process.WaitForExitAsync(); // 使用非同步的 WaitForExit 方法等待命令執行完畢
                int exitCode = process.ExitCode;
                process.Close();
                break;
        }
    }
}

void AIS(byte[] dataData, int length,StreamWriter writer)
{
    int count = 1;
    for (int i = 0; i < length; i = i + 16)
    {
        output($"Ship : {count}",writer);
        byte[] type_Byte = new byte[4];
        Array.Copy(dataData, i, type_Byte, 0, 4); // 0,1,2,3
        int type = BitConverter.ToInt32(type_Byte, 0);// 將 byte[] 轉換為整數
        output($"type: {type}",writer);
        output($"CRC: " + (type > 0 ? "pass" : "failed"),writer);
        byte[] mmsi_Byte = new byte[4];
        Array.Copy(dataData, i + 4, mmsi_Byte, 0, 4); // 4,5,6,7
        int mmsi = BitConverter.ToInt32(mmsi_Byte, 0);// 將 byte[] 轉換為整數
        output($"mmsi: {mmsi}",writer);

        byte[] lon_Byte = new byte[4];
        Array.Copy(dataData, i + 8, lon_Byte, 0, 4); // 8,9,10,11
        double lon = BitConverter.ToInt32(lon_Byte, 0) / 600000.0;// 將 byte[] 轉換為整數
        output($"lon: {lon}",writer);

        byte[] lat_Byte = new byte[4];
        Array.Copy(dataData, i + 12, lat_Byte, 0, 4); // 12,13,14,15
        double lat = BitConverter.ToInt32(lat_Byte, 0) / 600000.0;// 將 byte[] 轉換為整數
        output($"lat: {lat}\n",writer);
        count++;
    }
}

void SignalDetection(byte[] dataData,int length,StreamWriter writer,BinaryWriter outPutWriter){ // 定頻
    Console.WriteLine(rx_lo_freq);
    
    List<byte> hexData = new List<byte> ();
    byte[] NS_bytes = BitConverter.GetBytes(Task_General_NS); // 將整數轉換為 byte[]
    hexData.AddRange(NS_bytes);
    int i = 0,j=0;
    int count = 1;
    int index=0;
    while (i< length)
    {
        uint LO = ToUInt32(dataData, i);
        if (rx_lo_freq==LO)
        {
            j++;
            output($"第{count}筆,第{j}區塊,第{i}個byte:",writer);
            index=SDR_Chunk(i, dataData,writer);
            Console.WriteLine($"i={i},index={index}");
            byte[] Bytes = new byte[index-i];
            Array.Copy(dataData, i, Bytes, 0, Bytes.Length); // 0,1,2,3
            hexData.AddRange(Bytes);

            if (j==4){
                count++;
                j=0;
            } 
        }
        i =i+4;
    }
    outPutWriter.Write(hexData.ToArray());
}

int SDR_Chunk(int byteIndex, byte[] dataData,StreamWriter writer)
{
    int i= byteIndex;

    uint LO = ToUInt32(dataData,i);
    output($"LO: {LO}",writer);
    i =i+4;

    ushort Low_Threshold = ToUInt16(dataData, i);
    output($"Low_Threshold: {Low_Threshold}",writer);
    i=i+2;

    ushort High_Threshold = ToUInt16(dataData, i);
    output($"High_Threshold: {High_Threshold}",writer);
    i = i + 2;

    uint Covariance = ToUInt32(dataData, i);
    output($"Covariance: {Covariance}",writer);
    i = i + 4;
    // string FFT_str="";
    for(int k=0;k<65536;k++){
        ushort FFT = ToUInt16(dataData, i);
        // FFT_str+=$"{FFT},";
        //Console.WriteLine($"第{k}組, FFT_LOW: {FFT}");
        i = i + 2;
    }
    // FFT_str = FFT_str.Substring(0, FFT_str.Length - 1);
    // output($"FFT: {FFT_str}",writer);
    uint Signals_number = ToUInt32(dataData, i);
    output($"Signals_number: {Signals_number}",writer);
    i = i + 4;
    for (int k = 0; k < Signals_number; k++)
    {
        ushort Sig_start = ToUInt16(dataData, i);
        output($"Sig{k+1}_start: {Sig_start}",writer);
        i = i + 2;
        ushort Sig_end = ToUInt16(dataData, i);
        output($"Sig{k + 1}_end: {Sig_end}",writer);
        i = i + 2;
    }
    return i;
}

void RFScanning(byte[] dataData,int count,StreamWriter writer,BinaryWriter outPutWriter)
{ // 寬頻
    Console.WriteLine(rx_lo_freq);
    //Console.Write($"{count}: ");
    int i = 0;
    int byteindex=0;
    List<byte> hexData = new List<byte> ();
    byte[] count_bytes = BitConverter.GetBytes(count); // 將整數轉換為 byte[]
    hexData.AddRange(count_bytes);
    for(int index=1; index <= count; index++){
        uint LO = ToUInt32(dataData, i);
        if(LO>=(rx_lo_freq+8000000* (index-1)-10000)&& LO <= (rx_lo_freq + 8000000 * (index - 1) + 10000))
        {
            for (int j = 0; j < 4; j++)
            {
                output($"第{index}筆,第{j + 1}區塊: ",writer);
                //Console.WriteLine($"第{i}個byte, ");
                byteindex=i;
                i = SDR_Chunk(i, dataData,writer);
                byte[] Bytes = new byte[i-byteindex];
                Array.Copy(dataData, byteindex, Bytes, 0, Bytes.Length); // 0,1,2,3
                hexData.AddRange(Bytes);
                Console.WriteLine($"i={byteindex},index={i}");
            }
        }
        
        i= index * 528448;
        // 
    }
    outPutWriter.Write(hexData.ToArray());
}

void ADSB(byte[] dataData, int length,StreamWriter writer,BinaryWriter outPutWriter)
{
    // byte myByteInteger= dataData[0];
    // Console.WriteLine("Combined value before shifting: " + Convert.ToString(myByteInteger, 2).PadLeft(16, '0'));
    // 右移 3 個位元
    // myByteInteger >>= 3;
    // // Console.WriteLine("Combined value after shifting: " + Convert.ToString(myByteInteger, 2).PadLeft(16, '0'));
    // Console.WriteLine($"DF: {myByteInteger}");
    int i=0;
    int count=1;
    List<byte> hexData = new List<byte> ();
    
    while(i< length){
        int CRC=0;
        byte myByteInteger = dataData[i];
        myByteInteger >>= 3;
        switch (myByteInteger)
        {
            case 16:
            case 17:
            case 19:
            case 20:
            case 21:
            case 22:
                byte[] Bytes_14 = new byte[14];
                byte[] Bytes_16 = new byte[16];
                Array.Copy(dataData, i, Bytes_14, 0, 14);
                CRC=DetectCRC(Bytes_14,14*8);
                dataData[i+14]= (byte)(CRC == 0 ? 0x00 : 0x01);
                Array.Copy(dataData, i, Bytes_16, 0, 16); 
                hexData.AddRange(Bytes_16);
                i=i+16;
                break;
            default:
                byte[] Bytes_7 = new byte[7];
                byte[] Bytes_8 = new byte[8];
                Array.Copy(dataData, i, Bytes_7, 0, 7);
                CRC=DetectCRC(Bytes_7,7*8);
                dataData[i+7]= (byte)(CRC == 0 ? 0x00 : 0x01);
                Array.Copy(dataData, i, Bytes_8, 0, 8); 
                hexData.AddRange(Bytes_8);
                i = i + 8;
                break;
        }
        output($"第{count}筆,DF: {myByteInteger}, CRC: {CRC} ",writer);
        count++;
    }  
    byte[] count_bytes = BitConverter.GetBytes(count-1); // 將整數轉換為 byte[]
    hexData.InsertRange(0, count_bytes);
    outPutWriter.Write(hexData.ToArray());

}

uint ToUInt32(byte[] data, int startIndex)
{
    byte[] byteData = new byte[4];
    Array.Copy(data, startIndex, byteData, 0, 4);
    return BitConverter.ToUInt32(byteData, 0);
}

ushort ToUInt16(byte[] data, int startIndex)
{
    byte[] byteData = new byte[2];
    Array.Copy(data, startIndex, byteData, 0, 2);
    return BitConverter.ToUInt16(byteData, 0);
}

void output(string text,StreamWriter  writer){
    Console.WriteLine($"{text}");
    writer.WriteLine($"{text}");
}

// 檢查CRC
int DetectCRC(byte[] msg, int bitLength)
{
    uint crc = 0;
    uint crc2;
    string hexString = msg[bitLength/8-1].ToString("X");
    // Console.WriteLine(hexString);
    // bitLength 是該筆ADS-B的長度 (112 or 56)
    // 取得 crc
    crc = ((uint)msg[(bitLength / 8) - 3] << 16) |
            ((uint)msg[(bitLength / 8) - 2] << 8) |
            (uint)msg[(bitLength / 8) - 1];
    // Console.WriteLine($"{((uint)msg[(bitLength / 8) - 3] << 16)},{((uint)msg[(bitLength / 8) - 2] << 8)},{(uint)msg[(bitLength / 8) - 1]}={crc}");
    // 設定訊息的 crc
    //Msg.crc=crc;

    // 使用 modesChecksum 方法取得 crc2
    crc2 = ModesChecksum(msg, bitLength);

    // 比對 crc 與 crc2
    if (crc == crc2)
    {
        return 1;
    }
    else
    {
        return 0;
    }
}

// 計算 CRC
uint ModesChecksum(byte[] msg, int bits)
{
    uint crc = 0;
    int offset = (bits == 112) ? 0 : (112 - 56);
    int j;

    for (j = 0; j < bits; j++)
    {
        int byteIndex = j / 8;
        int bitIndex = j % 8;
        int bitmask = 1 << (7 - bitIndex);

        // 如果位元被設定，則與對應的表格項目進行 XOR
        if ((msg[byteIndex] & bitmask) != 0)
        {
            // Console.Write($"{crc} xor {ModesChecksumTable[j + offset]},j: {j},offset: {offset},j + offset: {j + offset}=");
            crc ^= ModesChecksumTable[j + offset];
            // Console.WriteLine(crc);
        }
    }

    return crc; // 回傳 24 位元的校驗碼
}
