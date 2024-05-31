// See https://aka.ms/new-console-template for more information

using System.Text;

string filePath = "RIFF-AIS/ais_v2_2.ais";//"RIFF-WAVE Audio/audio7.wav";//"RIFF-WAVE IQ/iq_v2_1.wav";//"RIFF-AIS/ais_v2_1.ais";
try
{
    using (FileStream fs = new FileStream(filePath, FileMode.Open))
    {
        BinaryReader reader = new BinaryReader(fs);
        long fileSize = fs.Length;
        // byte[] packetData = reader.ReadBytes((int)fileSize); // 减去头部和整数所占的字节数                                                          
        // 读取文件头部信息
        string Chunk_ID_RIFF = Encoding.ASCII.GetString(reader.ReadBytes(4));
        int Chunk_Size = reader.ReadInt32();
        string riff_Type = Encoding.ASCII.GetString(reader.ReadBytes(4));
        Console.WriteLine($"fileSize: {fileSize}");
        Console.WriteLine($"Chunk ID RIFF: {Chunk_ID_RIFF}");
        Console.WriteLine($"Chunk Size: {Chunk_Size}");
        Console.WriteLine($"riff Type: {riff_Type}");

        //// fmt
        Console.WriteLine();
        string Chunk_ID_fmt = Encoding.ASCII.GetString(reader.ReadBytes(4));
        int Chunk_Size_fmt = reader.ReadInt32();
        Console.WriteLine($"Chunk ID fmt: {Chunk_ID_fmt}");
        Console.WriteLine($"Chunk Size fmt: {Chunk_Size_fmt}");
        byte[] fmtData = reader.ReadBytes(Chunk_Size_fmt);
        switch (riff_Type){
            case "AIS ":
                // 定義一個新的byte[]來存儲前4個byte
                // byte[] AIS_Decode_Result_Format_Version_Byte = new byte[1];
                // Array.Copy(fmtData, 0, AIS_Decode_Result_Format_Version_Byte, 0, 1);
                int AIS_Decode_Result_Format_Version = (int)fmtData[0];
                Console.WriteLine($"AIS Decode Result Format Version: {AIS_Decode_Result_Format_Version}");
                int Scan_Number_per_Record = (int)fmtData[1];
                Console.WriteLine($"Scan Number per Record: {Scan_Number_per_Record}");
                int Header_Size = (int)fmtData[2];
                Console.WriteLine($"Header Size: {Scan_Number_per_Record}");
                int AIS_Information_Size = (int)fmtData[3];
                Console.WriteLine($"AIS Information Size: {AIS_Information_Size}");
                break;
            case "WAVE":
                break;
            case "SDR":
                break;
            case "ADSB":
                break;
        }
        //// IRIS
        Console.WriteLine();
        string Chunk_ID_IRIS = Encoding.ASCII.GetString(reader.ReadBytes(4));
        int Chunk_Size_IRIS = reader.ReadInt32();
        Console.WriteLine($"Chunk ID IRIS: {Chunk_ID_IRIS}");
        Console.WriteLine($"Chunk Size IRIS: {Chunk_Size_IRIS}");
        byte[] IRISData = reader.ReadBytes(Chunk_Size_IRIS);
        switch (riff_Type)
        {
            case "AIS ":
            case "WAVE":
                int Task_General_Satellite_ID = (int)IRISData[0];
                switch(Task_General_Satellite_ID)
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
                Task_General_UNIX_int= Task_General_UNIX_int*1000;                                                                               // 假設您要從 1970 年 1 月 1 日開始計算毫秒數
                DateTime Task_General_UNIX = new DateTime(1970, 1, 1).AddMilliseconds(Task_General_UNIX_int );
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
                Console.WriteLine($"NS: {Task_General_NS}");
                switch (Task_General_NS)
                {
                    case 0:
                        Console.WriteLine($"NS: {Task_General_NS}->5s");
                        break;
                    case 1:
                        Console.WriteLine($"Task General Task Name: {Task_General_NS}->10s");
                        break;
                    case 2:
                        Console.WriteLine($"Task General Task Name: {Task_General_NS}->20s");
                        break;
                    case 3:
                        Console.WriteLine($"Task General Task Name: {Task_General_NS}->40s");
                        break;
                }
                // 11,12,13,14
                int AD9361_antenna_source = (int)IRISData[15];
                Console.WriteLine($"AD9361 antenna source: {AD9361_antenna_source}");

                byte[] rx_rf_gain_Byte = new byte[4];
                Array.Copy(IRISData, 16, rx_rf_gain_Byte, 0, 4); // 16,17,18,19
                int rx_rf_gain = BitConverter.ToInt32(rx_rf_gain_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"rx_rf_gain: {rx_rf_gain}");

                byte[] rx_rf_bandwidth_Byte = new byte[4];
                Array.Copy(IRISData, 20, rx_rf_gain_Byte, 0, 4); // 20,21,22,23
                int rx_rf_bandwidth = BitConverter.ToInt32(rx_rf_bandwidth_Byte, 0);// 將 byte[] 轉換為整數
                Console.WriteLine($"rx_rf_bandwidth: {rx_rf_bandwidth}");

                byte[] rx_sampling_freq_Byte = new byte[4];
                Array.Copy(IRISData, 24, rx_rf_gain_Byte, 0, 4); // 24,25,26,27
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
            
            case "SDR":
                break;
            case "ADSB":
                break;
        }

        //// aux2
        Console.WriteLine();
        string Chunk_ID_aux2 = Encoding.ASCII.GetString(reader.ReadBytes(4));
        int Chunk_Size_aux2 = reader.ReadInt32();
        Console.WriteLine($"Chunk ID aux2: {Chunk_ID_aux2}");
        Console.WriteLine($"Chunk Size aux2: {Chunk_Size_aux2}");
        byte[] aux2Data = reader.ReadBytes(Chunk_Size_aux2);

        for(int i=0;i< Chunk_Size_aux2/72;i++){
            int k=i*72;
            int ARM_Temperature = (int)aux2Data[0+ k]; //0
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
        
        //// data
        string Chunk_ID_data = Encoding.ASCII.GetString(reader.ReadBytes(4));
        int Chunk_Size_data = reader.ReadInt32();
        Console.WriteLine($"Chunk ID data: {Chunk_ID_data}");
        Console.WriteLine($"Chunk Size data: {Chunk_Size_data}");
        byte[] dataData = reader.ReadBytes(Chunk_Size_data);
        // int type = reader.ReadInt32();
        // int mmsi = reader.ReadInt32();
        // int lon = reader.ReadInt32();
        // Console.WriteLine($"type: {type}");
        // Console.WriteLine($"type: {mmsi}");
        // Console.WriteLine($"type: {lon}");
        // 读取剩余的数据包内容
        // byte[] packetData = reader.ReadBytes((int)fileSize); // 减去头部和整数所占的字节数

        // // 输出数据包的十六进制表示
        // Console.WriteLine("數據包內容（十六進制）：");
        // foreach (byte b in packetData)
        // {
        //     Console.Write($"{b:X2} ");
        // }

        // 关闭文件流
        fs.Close();
    }
}
catch (Exception ex)
{
     Console.WriteLine($"發生錯誤: {ex.Message}");
}
while(true);
