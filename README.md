# RIFF_Resolve 使用教學
## 前置作業
請先確認是否有安裝.net環境。
測試方法，直接執行AIRS_image_process.exe，若是閃退，可能就是沒有安裝，
可至./前置作業中，執行並安裝windowsdesktop-runtime-8.0.6-win-x64.exe，
如果需要其他版本，可至以下網址下載:
https://dotnet.microsoft.com/zh-tw/download/dotnet/8.0

## 程式執行流程
1. 開啟 RIFF_Resolve.exe
2. 輸入要解析的**檔案編號**
3. 讓子彈飛一下
4. 請至./Export就可以找到解析完成的.txt檔

Ps1.若沒有看到**Import及Export**資料夾，請先開啟AIRS_image_process.exe，程式會初始化自動建立。

Ps2.如果看到以下訊息"**No files found in the ./Import directory. Would you like to search again?(y/s)**"，請先將要解析的CSV檔，放入./Import 資料夾中，然後按y，會重新搜尋資料夾。

## 版本更新
### V1.2 說明
1. 新增ADS-B CRC驗證機制
2. 新增SDR_Scan & SDR_SD 資料處理完成後，並去掉雜訊及FFFFFFFF，於Export/SDR匯出_output.txt檔案，供後處理應用。
3. 新增ADS-B 資料處理完成後，將每筆資料的有效bytes後一位Byte，加上CRC驗證結果，於Export/ADSB匯出_output.txt檔案，供後處理應用。
4. _output.txt檔案，前4個Byte皆為該包資料的資料筆數。

### V1.3 說明
1. 新增IQ資料轉換機制

### V1.4 說明
1. IQ資料轉換機制Bug修正
2. IQ資料解析heard

