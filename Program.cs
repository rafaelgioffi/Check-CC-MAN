using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Check_CC_MAN
{
    public class Program
    {
        static void Main(string[] args)
        {
            string LogFile = ConfigurationSettings.AppSettings["LogFile"];
            string Folder = ConfigurationSettings.AppSettings["FolderToProcessFiles"];
            string FolderToBackup = ConfigurationSettings.AppSettings["FolderToBackupFiles"];
            string FileToProcess = ConfigurationSettings.AppSettings["FileToProcess"];
            string[] filesInFolder;
            int fileQuantity = 0;
            int linha = 1;

            Log("\n################### Inicio do processamento ###################", true);
            try
            {
                filesInFolder = Directory.GetFiles(Folder, FileToProcess + "*");  //procura os CRDIFs que tiverem na pasta
                fileQuantity = filesInFolder.Length;  //quantidade de arquivos encontrados
            }
            catch (Exception ex)
            {
                Log($"\nNenhum arquivo {FileToProcess}.TXT encontrado! Ignorando o processamento...\n\n", false);
                return;
            }
            string FileValidName = "";    //nome do novo arquivo corrigido
            string FileInvalidName = "";    //nome do novo arquivo COM o(s) DN(s) incorretos            

            List<string> FileValid = new List<string>();
            List<string> FileInvalid = new List<string>();
            List<int> InvalidLines = new List<int>();

            if (fileQuantity > 0)
            {
                Log($"Iniciando o processamento de {fileQuantity} arquivos.", false);
                int counter = 1;
                foreach (var file in filesInFolder) //processa arquivo por arquivo encontrado na pasta...
                {
                    FileValid.Clear();
                    FileInvalid.Clear();

                    Log($"Processando o arquivo {counter}/{fileQuantity}...", false);
                    try
                    {
                        string[] allLines = File.ReadAllLines(file);
                        int lines = allLines.Length;
                        
                        for (int i = 0; i < lines; i++)
                        {
                            string[] actualLine = allLines[i].Split('#');

                            if (!string.IsNullOrEmpty(actualLine[0]) && !string.IsNullOrEmpty(actualLine[1]) && !string.IsNullOrEmpty(actualLine[2]) && !string.IsNullOrEmpty(actualLine[3]) && !string.IsNullOrEmpty(actualLine[4]) && !string.IsNullOrEmpty(actualLine[5]) && !string.IsNullOrEmpty(actualLine[6]) && !string.IsNullOrEmpty(actualLine[7]) && !string.IsNullOrEmpty(actualLine[8]) && !string.IsNullOrEmpty(actualLine[9]) && !string.IsNullOrEmpty(actualLine[10]) && !string.IsNullOrEmpty(actualLine[11]) && !string.IsNullOrEmpty(actualLine[12]) && !string.IsNullOrEmpty(actualLine[13]) && actualLine[14] == " " && actualLine[15] == " " && !string.IsNullOrEmpty(actualLine[16]) && !string.IsNullOrEmpty(actualLine[17]) && !string.IsNullOrEmpty(actualLine[18]) && !string.IsNullOrEmpty(actualLine[19]) && !string.IsNullOrEmpty(actualLine[20]) && !string.IsNullOrEmpty(actualLine[21]) && !string.IsNullOrEmpty(actualLine[22]) && !string.IsNullOrEmpty(actualLine[23]) && actualLine[24] == "        ")
                        {
                            FileValid.Add(allLines[i]);
                        }
                            else
                            {
                                FileInvalid.Add(allLines[i]);
                                InvalidLines.Add(i);
                            }
                        }

                        if (FileInvalid.Count > 0)  //só executa alguma ação se encontrar algum DN inválido...
                        {
                            for (int i = 0; i< FileInvalid.Count; i++)
                            {
                                Log($"Linha {i} suspeita");
                            }
                        /*
                            string[] actual = file.Split('\\');
                            string actualName = actual.Last();
                            actual = actualName.Split('.');
                            actualName = $"{actual[0]}.{actual[1]}";

                            //FileInvalidName = $"{actualName}.TXT.ERROR.D{DateTime.Now.ToString("yyyyMMdd")}.T{DateTime.Now.ToString("HHmmss")}";   //nome do arquivo com erros...
                            FileInvalidName = $"{actualName}.TXT.ERROR.{procDateFiltered}.{timeFile}";   //nome do arquivo com erros...

                            //cria as pastas caso não existam...
                            if (!Directory.Exists(FolderToBackup)) { Directory.CreateDirectory(FolderToBackup); }
                            if (!Directory.Exists(FolderNonProcessed)) { Directory.CreateDirectory(FolderNonProcessed); }

                            //Verifica se o arquivo já existe na pasta e caso exista, add 3s no timestamp...
                            if (File.Exists(FolderNonProcessed + FileInvalidName))
                            {
                                int tempTime = int.Parse(timeFile) + 3;
                                FileValidName = $"{actualName}.TXT.ERROR.{procDateFiltered}.{tempTime}";
                            }
                        
                            using (StreamWriter sw = new StreamWriter(FolderNonProcessed + FileInvalidName)) //cria o arquivo somente dos banidos...
                            {
                                foreach (string newLines in FileInvalid)
                                {
                                    sw.WriteLine(newLines);
                                }
                            }
                            Log($"Gerado o arquivo com os DNs inválidos em {FolderNonProcessed}{FileInvalidName}...", false);
                            Console.WriteLine($"Gerado o arquivo com os DNs inválidos em {FolderNonProcessed}{FileInvalidName}...");
                            MailBody += $"\n<tr>\n<td>{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} => Gerado o arquivo com os DNs inválidos em {FolderNonProcessed}{FileInvalidName}...\n</td>\n</tr>";

                            //Thread.Sleep(3000); //intervalo de 3s por segurança...

                            //string BkpOriginalFile = $"{actualName}.TXT.ORIGINAL.D{DateTime.Now.ToString("yyyyMMdd")}.T{DateTime.Now.ToString("HHmmss")}";
                            string BkpOriginalFile = $"{actualName}.TXT.ORIGINAL.{procDateFiltered}.{timeFile}";

                            //Verifica se o arquivo já existe na pasta e caso exista, add 3s no timestamp...
                            if (File.Exists(FolderToBackup + BkpOriginalFile))
                            {
                                int tempTime = int.Parse(timeFile) + 3;
                                BkpOriginalFile = $"{actualName}.TXT.ORIGINAL.{procDateFiltered}.{tempTime}";
                            }

                            File.Move(file, FolderToBackup + BkpOriginalFile);    //renomeia o arquivo original para não ser processado...
                            Log($"Arquivo {actualName}.TXT renomeado e movido para {FolderToBackup}{BkpOriginalFile}...", false);
                            Console.WriteLine($"Arquivo {actualName}.TXT renomeado e movido para {FolderToBackup}{BkpOriginalFile}...");
                            MailBody += $"\n<tr>\n<td>{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} => Arquivo {actualName}.TXT renomeado e movido para {FolderToBackup}{BkpOriginalFile}...\n</td>\n</tr>";

                            //Thread.Sleep(3000); //intervalo de 3s por segurança...

                            //FileWithoutBanName = $"{actualName}.TXT.SB.D{DateTime.Now.ToString("yyyyMMdd")}.T{DateTime.Now.ToString("HHmmss")}";   //nome do arquivo sem ignorados...
                            FileValidName = $"{actualName}.TXT";   //nome do arquivo sem ignorados...

                            while (File.Exists(Folder + FileValidName))
                            {
                                int tempTime = int.Parse(timeFile) + 3;
                                FileValidName = $"{actualName}.TXT.{procDateFiltered}.{tempTime}";
                            }

                            using (StreamWriter sw = new StreamWriter(Folder + FileValidName)) //cria o arquivo somente sem os banidos...
                            {
                                foreach (string newLines in FileValid)
                                {
                                    sw.WriteLine(newLines);
                                }
                            }
                            Log($"Gerado o arquivo válido em {Folder}{FileValidName}...", false);
                            Console.WriteLine($"Gerado o arquivo válido em {Folder}{FileValidName}...");
                            MailBody += $"\n<tr>\n<td>{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} => Gerado o arquivo válido em {Folder}{FileValidName}...\n</td>\n</tr>\n</table>";
                            MailBody += $"<p><b><i>Atenciosamente.</i></b></p>\n<p><b>Monitoring Team</b></p>\n<p>www.br.atos.net</p><img src='cid:assinatura' />";
                            MailBody += $"<p>This email and the documents attached are confidential and intended solely for the addressee; it may also be privileged. If you receive this e-mail in error, please notify the sender immediately and destroy it. As its integrity cannot be secured on the internet, the Atos group liability cannot be triggered for the message content. Although the snder endeavors to maintain a computer virus-free network, the sender does not warrant that this transmission is virus-free and will not be liable for any damages resulting from any virus transmitted.</p>\n</body>\n</html>";
                            EnviaEMail();
                        */
                        }
                        else
                        {
                            Log($"Nenhum DN inválido encontrado! Nenhuma alteração realizada no arquivo {file}");                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Erro ao processar o arquivo {file}. {ex.Message}");                        
                    }
                    counter++;
                }
            }
            else
            {
                Log($"Nenhum arquivo {FileToProcess} para processar em {Folder}");
                }

            Log("#################### Fim do processamento #####################\n", true);
            
            void Log(string message, bool special = false)
            {
                using (StreamWriter swLog = new StreamWriter(LogFile, true))
                {
                    if (special)
                    {
                        swLog.WriteLine(message);
                        Console.WriteLine(message);
                    }
                    else
                    {
                        swLog.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} => {message}");
                        Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} => {message}");
                    }
                }
            }

            
            
        }
    }
}
