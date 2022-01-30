using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.IO;

public class Security : MonoBehaviour
{
    [DllImport("kernel32.dll")]
    private static extern long GetVolumeInformation(
    string PathName,
    StringBuilder VolumeNameBuffer,
    UInt32 VolumeNameSize,
    ref UInt32 VolumeSerialNumber,
    ref UInt32 MaximumComponentLength,
    ref UInt32 FileSystemFlags,
    StringBuilder FileSystemNameBuffer,
    UInt32 FileSystemNameSize);

    public string disk; // задать в инспекторе  "C:\"
    private string sn = "";
    public string folder = "/Data";
    public string fileName = "Settings.dat";
    public string code = "AG7XpyPfmwN28193";
    public float time = 2f;


    void Start()
    {
        Invoke("Set", time); //задержка 
        //Set();
        //DebugSave(); //записываем в файл значение переменной code
        //DebugLoad(); //считываем значение из файла
    }


    private void Getvolumeinformation() //считываем системную информацию
    {
        string drive_letter = disk;
        drive_letter = drive_letter.Substring(0, 1) + ":\\";

        uint serial_number = 0;
        uint max_component_length = 0;
        StringBuilder sb_volume_name = new StringBuilder(256);
        UInt32 file_system_flags = new UInt32();
        StringBuilder sb_file_system_name = new StringBuilder(256);

        if (GetVolumeInformation(drive_letter, sb_volume_name,
            (UInt32)sb_volume_name.Capacity, ref serial_number,
            ref max_component_length, ref file_system_flags,
            sb_file_system_name,
            (UInt32)sb_file_system_name.Capacity) == 0)
        {
            Debug.Log(message: "Error getting volume information.");
        }
        else
        {
            sn = serial_number.ToString(); //серийный номер
            Debug.Log(message: sn);
        }
    }

    private void DebugSave()
    {

        try //обработка исключений
        {

            sn = code;

            //кодируем в двоичный код
            byte[] buf = Encoding.UTF8.GetBytes(sn);
            StringBuilder sb = new StringBuilder(buf.Length * 8);
            foreach (byte b in buf)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            string binaryStr = sb.ToString();


            //создаем папку в директории проекта
            Directory.CreateDirectory(Application.dataPath + folder);

            //сохраняем в файл
            using (var stream = File.Open(Application.dataPath + folder + "/" + fileName, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    
                    writer.Write(binaryStr);
                    writer.Close();
                }
            }


        }
        catch
        {
            Debug.Log(message: "Ошибка чтения в файл"); //выводим сообщение об ошибке
        }

    }

    private void DebugLoad()
    {

        if (File.Exists(Application.dataPath + folder + "/" + fileName)) 
        {
            using (var stream = File.Open(Application.dataPath + folder + "/" + fileName, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    string binaryStr = reader.ReadString();
                    reader.Close();

                    sn = BinaryToString(binaryStr);

                    Debug.Log(message: sn);

                }
            }
        }
        else
        {
            Debug.Log(message: "Не удается найти файл"); //выводим сообщение об ошибке
        }

    }

    public static string BinaryToString(string data) //функция перевода двоичного когда в текст
    {
        List<Byte> byteList = new List<Byte>();

        for (int i = 0; i < data.Length; i += 8)
        {
            byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
        }

        return Encoding.ASCII.GetString(byteList.ToArray());
    }

    private void Set()
    {
        if (File.Exists(Application.dataPath + folder + "/" + fileName)) //проверяем наличие файла, если его нет выводим сообщение о приатстве.
        {
            using (var stream = File.Open(Application.dataPath + folder + "/" + fileName, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    string binaryStr = reader.ReadString();
                    reader.Close(); //закрываем файл

                    //двоичный код переобразовываем в строку
                    string resultText = BinaryToString(binaryStr);
                    
                    Getvolumeinformation(); //читаем серийный номер диска

                    if ((resultText == code) || (resultText == sn))
                    {
                        if (resultText == code)
                        {
                            Save();
                        }
                            

                    }
                    else
                    {
                        Debug.Log(message: "Пират!"); //выводим сообщение об ошибке
                        NativeWinAlert.Error("This copy of game is not genuine.", "Error");
                    }

                }
            }

        }
        else
        {
            Debug.Log(message: "Пират!"); //выводим сообщение об ошибке
            NativeWinAlert.Error("This copy of game is not genuine.", "Error");
        }

    }

    private void Save()
    {

        try //обработка исключений
        {

            //кодируем в двоичный код
            byte[] buf = Encoding.UTF8.GetBytes(sn);
            StringBuilder sb = new StringBuilder(buf.Length * 8);
            foreach (byte b in buf)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            string binaryStr = sb.ToString();

            //сохраняем в файл
            using (var stream = File.Open(Application.dataPath + folder + "/" + fileName, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {

                    writer.Write(binaryStr);
                    writer.Close(); //закрываем файл
                }
            }


        }
        catch
        {
            Debug.Log(message: "Ошибка чтения в файл"); //выводим сообщение об ошибке
        }

    }

    
    //фунция отвечающая за вывод системных сообщений
    public static class NativeWinAlert
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern System.IntPtr GetActiveWindow();

        public static System.IntPtr GetWindowHandle()
        {
            return GetActiveWindow();
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern int MessageBox(IntPtr hwnd, String lpText, String lpCaption, uint uType);

        /// <summary>
        /// Shows Error alert box with OK button.
        /// </summary>
        /// <param name="text">Main alert text / content.</param>
        /// <param name="caption">Message box title.</param>
        public static void Error(string text, string caption)
        {
            try
            {
                MessageBox(GetWindowHandle(), text, caption, (uint)(0x00000000L | 0x00000010L));
                Debug.Log("Игра закрылась");
                Application.Quit();    // закрыть приложение
            }
            catch (Exception ex) { }
        }
    }

}
