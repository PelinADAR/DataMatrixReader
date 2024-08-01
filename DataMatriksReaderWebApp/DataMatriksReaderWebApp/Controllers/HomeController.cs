
using Microsoft.AspNetCore.Mvc;
using System.DrawingCore;
using ZXing.Common;
using ZXing;
using ZXing.ZKWeb;
using System.Reflection.PortableExecutable;
using System;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Aspose.BarCode.BarCodeRecognition;
using System.Text;


namespace DataMatrixReaderWebApp
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadImage(IFormFile imageFile)
        {
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var filePath = Path.GetTempFileName();
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    string decodedText = ReadDataMatrixWithDataMat(filePath);

                    string outputFilePath = Path.Combine(Path.GetTempPath(), "BarcodeResults.txt");
                    WriteToTextFile(decodedText);

                    ViewBag.DecodedText = decodedText;
                    ViewBag.OutputFilePath = outputFilePath;
                }
                else
                {
                    ViewBag.Error = "Dosya se�ilmedi veya bo� dosya y�klendi.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Hata: " + ex.Message;
            }

            return View("Index");
        }



        //ZXing ile
        private string ReadDataMatrixWithZxing(string imagePath)
        {
            try
            {
                Bitmap bitmap = new Bitmap(imagePath);

               



                BarcodeReader reader = new BarcodeReader
                {
                    AutoRotate = true,
                    TryInverted = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.DATA_MATRIX },
                        PureBarcode = false
                    }
                };
                Result result = reader.Decode(bitmap);


                if (result != null)
                {
                    return result.Text;
                }
                else
                {
                    return "DataMatrix bulunamad� veya okunamad�.";
                }
            }
            catch (Exception ex)
            {
                return "Hata: " + ex.Message;
            }
        }

        //Aspose ile
        private string ReadDataMatrixWithAspose(string imagePath)
        {
            using (var reader = new BarCodeReader(imagePath, DecodeType.AllSupportedTypes))
            {
                foreach (var barcode in reader.ReadBarCodes())
                {
                    Console.WriteLine($"{barcode.CodeTypeName}: {barcode.CodeText}");
                    return barcode.CodeText;
                }
                return "Done";
            }


        }

        //Datamatrix.Net ile
        private string ReadDataMatrixWithDataMat(string imagePath)
        {
            try
            {
                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imagePath))
                {
                    DataMatrix.net.DmtxImageDecoder decoder = new DataMatrix.net.DmtxImageDecoder();

                    var results = decoder.DecodeImage(bitmap);

                    StringBuilder sb = new StringBuilder();
                    foreach (var result in results)
                    {
                        sb.AppendLine(result);
                    }

                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                return "Hata: " + ex.Message;
            }


        }


        private void WriteToTextFile(string text)
        {
            string filePath = Path.Combine(Environment.CurrentDirectory, "wwwroot/Results", "BarcodeResults.txt");
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string directoryPath = Path.GetDirectoryName(filePath);
                    string output = $"Date Created: {dateTime}\nFile Path: {directoryPath}\nOkunan de�er: {text}\n\n";
                    sw.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sonu� dosyaya yaz�lamad�", ex);
            }
        }


        public IActionResult DownloadFile()
        {
            string filePath = Path.Combine(Environment.CurrentDirectory, "wwwroot/Results", "BarcodeResults.txt");
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                    string fileName = "BarcodeResults.txt";

                    return File(fileBytes, "application/octet-stream", fileName);
                }
                else
                {
                    ViewBag.Error = "Dosya bulunamad�.";
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Hata: " + ex.Message;
                return View("Index");
            }
        }





    }
}